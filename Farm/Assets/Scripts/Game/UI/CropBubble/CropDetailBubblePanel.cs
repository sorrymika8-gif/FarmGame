using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using QFramework;
using Cysharp.Threading.Tasks;
using FarmGame.Farm;
using FarmGame.GameConfig;
using FarmGame.GameConfig.Generated;
using FarmGame.Item;
using FarmGame.Core;
using FarmGame.Core.LLMDescription;

namespace FarmGame.UI
{
    /// <summary>
    /// 作物详情气泡数据
    /// </summary>
    public class CropDetailBubbleData : UIPanelData
    {
        /// <summary>
        /// 作物实体
        /// </summary>
        public PlantEntity Plant { get; set; }

        /// <summary>
        /// 关联的土地实体
        /// </summary>
        public SoilEntity Soil { get; set; }

        /// <summary>
        /// 作物世界坐标
        /// </summary>
        public Vector3 WorldPosition { get; set; }

        /// <summary>
        /// 玩家背包（用于收获）
        /// </summary>
        public InventoryComponent Inventory { get; set; }
    }

    /// <summary>
    /// 作物详情气泡面板
    /// 点击作物后显示详细信息
    /// </summary>
    public class CropDetailBubblePanel : UIPanel
    {
        #region 序列化字段

        [Header("背景遮罩")]
        [SerializeField] private Button mBackgroundMask;

        [Header("气泡容器")]
        [SerializeField] private RectTransform mBubbleContainer;

        [Header("基本信息")]
        [Tooltip("作物名称")]
        [SerializeField] private TextMeshProUGUI mNameText;

        [Tooltip("作物图标")]
        [SerializeField] private Image mIconImage;

        [Tooltip("生长阶段文本")]
        [SerializeField] private TextMeshProUGUI mStageText;

        [Header("进度信息")]
        [Tooltip("进度条填充")]
        [SerializeField] private Image mProgressFill;

        [Tooltip("进度百分比文本")]
        [SerializeField] private TextMeshProUGUI mProgressText;

        [Tooltip("预计成熟时间文本")]
        [SerializeField] private TextMeshProUGUI mTimeText;

        [Header("作物描述")]
        [Tooltip("LLM生成的作物状态描述")]
        [SerializeField] private TextMeshProUGUI mDescriptionText;

        [Header("收获预览")]
        [Tooltip("收获物容器")]
        [SerializeField] private Transform mHarvestPreviewContainer;

        [Tooltip("收获物项预制体")]
        [SerializeField] private GameObject mHarvestItemPrefab;

        [Header("操作按钮")]
        [Tooltip("收获按钮")]
        [SerializeField] private Button mHarvestButton;

        [Tooltip("收获按钮文本")]
        [SerializeField] private TextMeshProUGUI mHarvestButtonText;

        #endregion

        #region 私有字段

        private CropDetailBubbleData mData;
        private SeedConfig mConfig;
        private Camera mMainCamera;
        private List<GameObject> mHarvestItems = new List<GameObject>();

        /// <summary>
        /// 动态创建的描述区域容器
        /// </summary>
        private GameObject mDescriptionArea;
        
        /// <summary>
        /// 气泡垂直偏移量
        /// </summary>
        private const float BUBBLE_VERTICAL_OFFSET = 80f;
        
        /// <summary>
        /// 气泡是否显示在上方
        /// </summary>
        private bool mShowAbove = true;

        /// <summary>
        /// 作物阶段名称
        /// </summary>
        private static readonly string[] STAGE_NAMES = { "幼苗期", "生长期", "成熟期" };

        /// <summary>
        /// 各阶段颜色
        /// </summary>
        private static readonly Color[] STAGE_COLORS = 
        {
            new Color(0.5f, 0.85f, 0.5f),  // 幼苗 - 浅绿
            new Color(0.3f, 0.7f, 0.3f),   // 中期 - 中绿
            new Color(1f, 0.85f, 0.2f)     // 成熟 - 金色
        };

        /// <summary>
        /// LLM描述加载取消令牌
        /// </summary>
        private CancellationTokenSource mDescriptionCts;

        #endregion

        #region UIPanel 生命周期

        protected override void OnInit(IUIData uiData = null)
        {
            mMainCamera = Camera.main;

            // 绑定背景遮罩点击关闭
            if (mBackgroundMask != null)
            {
                mBackgroundMask.onClick.RemoveAllListeners();
                mBackgroundMask.onClick.AddListener(CloseSelf);
            }

            // 绑定收获按钮
            if (mHarvestButton != null)
            {
                mHarvestButton.onClick.RemoveAllListeners();
                mHarvestButton.onClick.AddListener(OnHarvestClicked);
            }

            // 如果没有序列化的描述文本，动态创建描述区域
            if (mDescriptionText == null)
            {
                CreateDescriptionArea();
            }
        }

        protected override void OnOpen(IUIData uiData = null)
        {
            mData = uiData as CropDetailBubbleData;
            if (mData == null || mData.Plant == null)
            {
                Debug.LogWarning("[CropDetailBubblePanel] 无效的数据");
                CloseSelf();
                return;
            }

            mConfig = mData.Plant.PlantData;
            if (mConfig == null)
            {
                Debug.LogWarning("[CropDetailBubblePanel] 无法获取作物配置");
                CloseSelf();
                return;
            }

            // 设置气泡位置
            SetBubblePosition(mData.WorldPosition);

            // 更新显示
            UpdateDisplay();

            // 异步加载 LLM 描述
            LoadDescriptionAsync().Forget();
        }

        protected override void OnClose()
        {
            // 取消正在进行的 LLM 请求
            mDescriptionCts?.Cancel();
            mDescriptionCts?.Dispose();
            mDescriptionCts = null;

            ClearHarvestItems();
            mData = null;
            mConfig = null;
        }

        private void Update()
        {
            // 更新气泡位置（跟随作物）
            if (mData != null && mMainCamera != null)
            {
                SetBubblePosition(mData.WorldPosition);
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 设置气泡位置 (智能跟随土地)
        /// </summary>
        private void SetBubblePosition(Vector3 worldPos)
        {
            if (mBubbleContainer == null || mMainCamera == null) return;

            // 世界坐标转屏幕坐标
            Vector3 screenPos = mMainCamera.WorldToScreenPoint(worldPos);

            // 转换为Canvas坐标
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos,
                canvas.worldCamera,
                out Vector2 localPoint
            );

            // 确保气泡不超出屏幕边界
            var canvasRect = canvas.transform as RectTransform;
            var bubbleSize = mBubbleContainer.sizeDelta;
            
            // 判断格子在屏幕上半还是下半
            float screenMidY = canvasRect.rect.height / 2f * 0.3f; // 使用 30% 作为分割线，偏向上方显示
            mShowAbove = localPoint.y < screenMidY;

            // 调整X位置 - 确保不超出屏幕左右
            float halfWidth = bubbleSize.x / 2f;
            float maxX = canvasRect.rect.width / 2f - halfWidth - 10f;
            localPoint.x = Mathf.Clamp(localPoint.x, -maxX, maxX);

            // 调整Y位置 - 根据显示方向
            if (mShowAbove)
            {
                // 气泡显示在格子上方
                localPoint.y = localPoint.y + BUBBLE_VERTICAL_OFFSET;
                
                // 检查是否超出屏幕顶部
                float maxY = canvasRect.rect.height / 2f - bubbleSize.y - 10f;
                if (localPoint.y > maxY)
                {
                    localPoint.y = maxY;
                }
            }
            else
            {
                // 气泡显示在格子下方
                localPoint.y = localPoint.y - bubbleSize.y - BUBBLE_VERTICAL_OFFSET;
                
                // 检查是否超出屏幕底部
                float minY = -canvasRect.rect.height / 2f + 10f;
                if (localPoint.y < minY)
                {
                    localPoint.y = minY;
                }
            }

            mBubbleContainer.anchoredPosition = localPoint;
        }

        /// <summary>
        /// 更新显示
        /// </summary>
        private void UpdateDisplay()
        {
            if (mData?.Plant == null || mConfig == null) return;

            var plant = mData.Plant;

            // 更新名称
            if (mNameText != null)
            {
                mNameText.text = mConfig.name ?? "未知作物";
            }

            // 更新图标
            UpdateIcon();

            // 更新阶段
            UpdateStage();

            // 更新进度
            UpdateProgress();

            // 更新预计时间
            UpdateEstimatedTime();

            // 更新收获预览
            UpdateHarvestPreview();

            // 更新收获按钮
            UpdateHarvestButton();
        }

        /// <summary>
        /// 更新图标
        /// </summary>
        private void UpdateIcon()
        {
            if (mIconImage == null || mConfig == null) return;

            if (!string.IsNullOrEmpty(mConfig.icon))
            {
                Sprite sprite = null;
                
                // 尝试多种路径格式
                string[] pathFormats = new string[]
                {
                    "prefabs/" + mConfig.icon,           // prefabs/plants/pasture_seed
                    mConfig.icon,                         // plants/pasture_seed
                    "Sprites/Farm/" + mConfig.icon,      // Sprites/Farm/plants/pasture_seed
                    "Sprites/" + mConfig.icon,           // Sprites/plants/pasture_seed
                };

                foreach (var path in pathFormats)
                {
                    sprite = ResourceManager.Instance?.Load<Sprite>(path);
                    if (sprite != null)
                    {
                        break;
                    }
                }

                if (sprite != null)
                {
                    mIconImage.sprite = sprite;
                    mIconImage.gameObject.SetActive(true);
                    return;
                }
            }

            // 图标加载失败时仍然显示，使用默认颜色
            mIconImage.gameObject.SetActive(true);
            mIconImage.color = new Color(0.5f, 0.8f, 0.5f); // 绿色占位
        }

        /// <summary>
        /// 更新阶段显示
        /// </summary>
        private void UpdateStage()
        {
            if (mStageText == null || mData?.Plant == null) return;

            int stageIndex = Mathf.Clamp(mData.Plant.CurrentStageIndex, 0, STAGE_NAMES.Length - 1);
            mStageText.text = STAGE_NAMES[stageIndex];
            mStageText.color = STAGE_COLORS[stageIndex];
        }

        /// <summary>
        /// 更新进度显示
        /// </summary>
        private void UpdateProgress()
        {
            if (mData?.Plant == null || mConfig == null) return;

            float progress = mConfig.need_maturity > 0
                ? Mathf.Clamp01(mData.Plant.CurrentMaturity / mConfig.need_maturity)
                : 0f;

            // 更新进度条
            if (mProgressFill != null)
            {
                mProgressFill.fillAmount = progress;

                // 根据进度设置颜色
                if (mData.Plant.IsMature)
                {
                    mProgressFill.color = new Color(1f, 0.85f, 0.2f); // 金色
                }
                else
                {
                    mProgressFill.color = Color.Lerp(
                        new Color(0.5f, 0.85f, 0.5f),
                        new Color(0.3f, 0.7f, 0.3f),
                        progress
                    );
                }
            }

            // 更新进度文本
            if (mProgressText != null)
            {
                mProgressText.text = $"{progress * 100:F0}%";
            }
        }

        /// <summary>
        /// 更新预计成熟时间
        /// </summary>
        private void UpdateEstimatedTime()
        {
            if (mTimeText == null || mData?.Plant == null || mConfig == null) return;

            if (mData.Plant.IsMature)
            {
                mTimeText.text = "已成熟，可以收获！";
                mTimeText.color = new Color(1f, 0.85f, 0.2f);
            }
            else
            {
                // 计算剩余成熟度
                float remaining = mConfig.need_maturity - mData.Plant.CurrentMaturity;
                
                // 根据生长速度计算时间（假设每秒增加 maturity_speed * Time.deltaTime）
                if (mConfig.maturity_speed > 0)
                {
                    float secondsRemaining = remaining / mConfig.maturity_speed;
                    
                    if (secondsRemaining < 60)
                    {
                        mTimeText.text = $"预计 {secondsRemaining:F0} 秒后成熟";
                    }
                    else if (secondsRemaining < 3600)
                    {
                        mTimeText.text = $"预计 {secondsRemaining / 60:F0} 分钟后成熟";
                    }
                    else
                    {
                        mTimeText.text = $"预计 {secondsRemaining / 3600:F1} 小时后成熟";
                    }
                }
                else
                {
                    mTimeText.text = "生长中...";
                }
                
                mTimeText.color = Color.white;
            }
        }

        /// <summary>
        /// 更新收获物预览
        /// </summary>
        private void UpdateHarvestPreview()
        {
            ClearHarvestItems();

            if (mHarvestPreviewContainer == null || mConfig == null) return;
            if (mConfig.bonus_item == null || mConfig.bonus_item.Length == 0) return;

            for (int i = 0; i < mConfig.bonus_item.Length; i++)
            {
                int itemId = mConfig.bonus_item[i];
                int amount = (mConfig.bonus_amount != null && i < mConfig.bonus_amount.Length) 
                    ? mConfig.bonus_amount[i] 
                    : 1;

                CreateHarvestPreviewItem(itemId, amount);
            }
        }

        /// <summary>
        /// 创建收获物预览项
        /// </summary>
        private void CreateHarvestPreviewItem(int itemId, int amount)
        {
            if (mHarvestPreviewContainer == null) return;

            GameObject itemGo;
            if (mHarvestItemPrefab != null)
            {
                itemGo = Instantiate(mHarvestItemPrefab, mHarvestPreviewContainer);
            }
            else
            {
                // 简单创建一个显示项
                itemGo = new GameObject($"HarvestItem_{itemId}");
                
                var text = itemGo.AddComponent<TextMeshProUGUI>();
                text.fontSize = 14;
                text.alignment = TextAlignmentOptions.Center;

                // 设置父级并配置 RectTransform
                itemGo.transform.SetParent(mHarvestPreviewContainer, false);
                var rectTransform = itemGo.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(80, 30);
            }

            // 尝试获取物品配置并显示 (使用安全方式避免 KeyNotFoundException)
            string itemName;
            if (FarmGame.Item.ItemConfigHelper.TryGetConfigInfo(itemId, out var configInfo))
            {
                itemName = configInfo.Name;
            }
            else
            {
                itemName = $"物品{itemId}";
                Debug.LogWarning($"[CropDetailBubblePanel] 找不到物品配置: id={itemId}");
            }

            var textComp = itemGo.GetComponentInChildren<TextMeshProUGUI>();
            if (textComp != null)
            {
                textComp.text = $"{itemName} x{amount}";
            }

            mHarvestItems.Add(itemGo);
        }

        /// <summary>
        /// 清除收获物预览项
        /// </summary>
        private void ClearHarvestItems()
        {
            foreach (var item in mHarvestItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            mHarvestItems.Clear();
        }

        /// <summary>
        /// 动态创建描述区域
        /// 在收获预览上方显示 LLM 生成的作物状态描述
        /// </summary>
        private void CreateDescriptionArea()
        {
            if (mBubbleContainer == null) return;

            // 找到 HarvestPreviewArea 的位置，将描述区域插入其上方
            RectTransform harvestPreviewArea = null;
            for (int i = 0; i < mBubbleContainer.childCount; i++)
            {
                var child = mBubbleContainer.GetChild(i);
                if (child.name.Contains("HarvestPreview"))
                {
                    harvestPreviewArea = child as RectTransform;
                    break;
                }
            }

            // 将 HarvestPreviewArea 向下移动，为描述区域腾出空间
            if (harvestPreviewArea != null)
            {
                var pos = harvestPreviewArea.anchoredPosition;
                harvestPreviewArea.anchoredPosition = new Vector2(pos.x, pos.y - 50); // 向下移动50px
            }

            // 创建描述区域容器
            mDescriptionArea = new GameObject("DescriptionArea");
            mDescriptionArea.transform.SetParent(mBubbleContainer, false);

            var rectTransform = mDescriptionArea.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0, -140);
            rectTransform.sizeDelta = new Vector2(-20, 50);

            // 如果找到了 HarvestPreviewArea，把描述区域放在它前面
            if (harvestPreviewArea != null)
            {
                int siblingIndex = harvestPreviewArea.GetSiblingIndex();
                mDescriptionArea.transform.SetSiblingIndex(siblingIndex);
            }

            // 创建描述文本
            var textGo = new GameObject("DescriptionText");
            textGo.transform.SetParent(mDescriptionArea.transform, false);

            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 1);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.pivot = new Vector2(0.5f, 1f);
            textRect.anchoredPosition = new Vector2(0, -5);
            textRect.sizeDelta = new Vector2(-20, 0); // 宽度留边距，高度由 ContentSizeFitter 控制

            mDescriptionText = textGo.AddComponent<TextMeshProUGUI>();
            mDescriptionText.fontSize = 14;
            mDescriptionText.color = new Color(0.7f, 0.7f, 0.7f, 1f); // 浅灰色
            mDescriptionText.alignment = TextAlignmentOptions.TopLeft;
            mDescriptionText.enableWordWrapping = true;
            mDescriptionText.overflowMode = TextOverflowModes.Overflow; // 溢出模式，配合自适应
            mDescriptionText.text = "正在生成描述...";
            
            // 添加 ContentSizeFitter 实现自适应高度
            var textFitter = textGo.AddComponent<ContentSizeFitter>();
            textFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 默认显示
            mDescriptionArea.SetActive(true);
        }

        /// <summary>
        /// 更新收获按钮
        /// </summary>
        private void UpdateHarvestButton()
        {
            if (mHarvestButton == null) return;

            bool canHarvest = mData?.Plant?.IsMature == true;
            mHarvestButton.gameObject.SetActive(canHarvest);

            if (mHarvestButtonText != null)
            {
                mHarvestButtonText.text = "收获";
            }
        }

        /// <summary>
        /// 收获按钮点击
        /// </summary>
        private void OnHarvestClicked()
        {
            if (mData?.Soil == null || mData?.Inventory == null)
            {
                Debug.LogWarning("[CropDetailBubblePanel] 无法收获：数据不完整");
                return;
            }

            // 调用 FarmManager 收获
            var farmManager = FarmManager.Instance;
            if (farmManager != null)
            {
                bool success = farmManager.Harvest(mData.Soil, mData.Inventory);
                if (success)
                {
                    Debug.Log("[CropDetailBubblePanel] 收获成功！");
                    CloseSelf();
                }
                else
                {
                    Debug.LogWarning("[CropDetailBubblePanel] 收获失败");
                }
            }
            else
            {
                Debug.LogError("[CropDetailBubblePanel] FarmManager 未初始化");
            }
        }

        /// <summary>
        /// 异步加载 LLM 生成的描述
        /// 描述会追加显示在阶段文本下方
        /// </summary>
        private async UniTaskVoid LoadDescriptionAsync()
        {
            // 创建新的取消令牌
            mDescriptionCts?.Cancel();
            mDescriptionCts?.Dispose();
            mDescriptionCts = new CancellationTokenSource();

            try
            {
                // 检查服务和数据有效性
                var service = LLMDescriptionService.Instance;
                if (service == null || mData?.Plant == null)
                {
                    return;
                }

                // 调用 LLM 生成描述
                var description = await service.GenerateDescriptionAsync(mData.Plant)
                    .AttachExternalCancellation(mDescriptionCts.Token);

                // 检查面板是否仍然打开
                if (mData == null || mDescriptionCts == null || mDescriptionCts.IsCancellationRequested)
                {
                    return;
                }

                // 更新独立的描述文本区域
                if (mDescriptionText != null && !string.IsNullOrEmpty(description))
                {
                    Debug.Log($"[CropDetailBubblePanel] LLM返回描述: '{description}' (长度={description.Length})");
                    mDescriptionText.text = description;
                    mDescriptionText.gameObject.SetActive(true);
                    if (mDescriptionArea != null)
                    {
                        mDescriptionArea.SetActive(true);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 被取消，正常情况（面板关闭）
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[CropDetailBubblePanel] 加载描述失败: {e.Message}");
            }
        }

        #endregion
    }
}
