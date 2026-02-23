using System;
using UnityEngine;
using UnityEngine.UI;
using FarmGame.Item;
using FarmGame.GameConfig;
using FarmGame.GameConfig.Generated;

namespace FarmGame.UI
{
    /// <summary>
    /// 物品格子控制器
    /// 负责单个物品格子的UI显示、点击交互和选中状态
    /// </summary>
    public class InventorySlotController : MonoBehaviour
    {
        #region UI组件

        [Header("UI组件")]
        [SerializeField]
        private Image mIconImage; // 物品图标

        [SerializeField]
        private Text mCountText; // 物品数量

        [SerializeField]
        private Text mNameText; // 物品名称

        [SerializeField]
        private Text mDescriptionText; // 物品描述

        [SerializeField]
        private GameObject mEmptyVisual; // 空状态显示

        [SerializeField]
        private GameObject mOccupiedVisual; // 有物品状态显示

        [SerializeField]
        private Image mBackgroundImage; // 背景图片（用于高亮等效果）

        [SerializeField]
        private Button mSlotButton; // 格子按钮（用于点击交互）

        [SerializeField]
        private Image mSelectFrame; // 选中框（高亮边框）

        #endregion

        #region 私有字段

        private int mSlotIndex = -1; // 格子索引
        private ItemEntity mCurrentItem; // 当前物品
        private Color mDefaultBackgroundColor = Color.white; // 默认背景色
        private bool mIsSelected = false; // 是否被选中

        #endregion

        #region 事件

        /// <summary>
        /// 格子点击事件，参数为被点击的格子控制器
        /// </summary>
        public event Action<InventorySlotController> OnSlotClicked;

        #endregion

        #region 生命周期

        private void Awake()
        {
            // 保存默认背景色
            if (mBackgroundImage != null)
            {
                mDefaultBackgroundColor = mBackgroundImage.color;
            }

            // 绑定按钮点击事件
            if (mSlotButton != null)
            {
                mSlotButton.onClick.AddListener(OnButtonClick);
            }

            // 初始隐藏选中框
            SetSelected(false);
        }

        private void OnDestroy()
        {
            // 移除按钮事件监听
            if (mSlotButton != null)
            {
                mSlotButton.onClick.RemoveListener(OnButtonClick);
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 初始化格子
        /// </summary>
        /// <param name="slotIndex">格子索引</param>
        public void Initialize(int slotIndex)
        {
            mSlotIndex = slotIndex;
            ClearSlot();
            SetSelected(false);
        }

        /// <summary>
        /// 设置格子选中状态
        /// </summary>
        /// <param name="selected">是否选中</param>
        public void SetSelected(bool selected)
        {
            mIsSelected = selected;

            // 显示/隐藏选中框
            if (mSelectFrame != null)
            {
                mSelectFrame.gameObject.SetActive(selected);
            }

            // 如果没有选中框，使用背景色表示选中状态
            if (mSelectFrame == null && mBackgroundImage != null)
            {
                mBackgroundImage.color = selected ? new Color(1f, 0.8f, 0.3f, 1f) : mDefaultBackgroundColor;
            }
        }

        /// <summary>
        /// 获取是否被选中
        /// </summary>
        public bool IsSelected => mIsSelected;

        /// <summary>
        /// 更新格子显示
        /// </summary>
        /// <param name="item">物品实体</param>
        public void UpdateSlot(ItemEntity item)
        {
            mCurrentItem = item;

            if (item == null || item.Count <= 0)
            {
                ClearSlot();
                return;
            }

            // 获取物品配置（支持从多个配置表查询）
            var configInfo = item.ConfigInfo;
            if (!configInfo.IsValid)
            {
                Debug.LogWarning($"[InventorySlot] Item config not found for configId: {item.ConfigId}");
                ClearSlot();
                return;
            }

            // 显示有物品状态
            SetOccupiedState(true);

            // 更新图标
            UpdateIcon(configInfo);

            // 更新数量
            UpdateCount(item.Count);

            // 更新名称
            UpdateName(configInfo.Name);

            // 更新描述
            UpdateDescription(configInfo.Description);

            // 重置背景色
            ResetBackground();
        }

        /// <summary>
        /// 清空格子
        /// </summary>
        public void ClearSlot()
        {
            mCurrentItem = null;

            // 显示空状态
            SetOccupiedState(false);

            // 清空所有文本
            if (mCountText != null)
                mCountText.text = "";
            
            if (mNameText != null)
                mNameText.text = "";
            
            if (mDescriptionText != null)
                mDescriptionText.text = "";

            // 清空图标
            if (mIconImage != null)
            {
                mIconImage.sprite = null;
                mIconImage.gameObject.SetActive(false);
            }

            // 重置背景色
            ResetBackground();
        }

        /// <summary>
        /// 高亮格子
        /// </summary>
        /// <param name="color">高亮颜色</param>
        public void HighlightSlot(Color color)
        {
            if (mBackgroundImage != null)
            {
                mBackgroundImage.color = color;
            }
        }

        /// <summary>
        /// 获取当前物品
        /// </summary>
        /// <returns>当前物品实体</returns>
        public ItemEntity GetCurrentItem()
        {
            return mCurrentItem;
        }

        /// <summary>
        /// 获取格子索引
        /// </summary>
        /// <returns>格子索引</returns>
        public int GetSlotIndex()
        {
            return mSlotIndex;
        }

        /// <summary>
        /// 检查格子是否为空
        /// </summary>
        /// <returns>是否为空</returns>
        public bool IsEmpty()
        {
            return mCurrentItem == null || mCurrentItem.Count <= 0;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 按钮点击处理
        /// </summary>
        private void OnButtonClick()
        {
            // 触发点击事件
            OnSlotClicked?.Invoke(this);
        }

        /// <summary>
        /// 设置占用状态
        /// </summary>
        /// <param name="occupied">是否被占用</param>
        private void SetOccupiedState(bool occupied)
        {
            if (mEmptyVisual != null)
                mEmptyVisual.SetActive(!occupied);
            
            if (mOccupiedVisual != null)
                mOccupiedVisual.SetActive(occupied);
        }

        /// <summary>
        /// 更新图标
        /// </summary>
        /// <param name="configInfo">物品配置信息</param>
        private void UpdateIcon(ItemConfigInfo configInfo)
        {
            if (mIconImage == null)
                return;

            // 根据配置加载图标资源
            if (!string.IsNullOrEmpty(configInfo.Icon))
            {
                var sprite = Resources.Load<Sprite>(configInfo.Icon);
                if (sprite != null)
                {
                    mIconImage.sprite = sprite;
                    mIconImage.color = Color.white;
                }
                else
                {
                    // 图标资源不存在，使用类型颜色代替
                    SetPlaceholderIcon(configInfo);
                }
            }
            else
            {
                // 没有配置图标路径，使用类型颜色代替
                SetPlaceholderIcon(configInfo);
            }

            mIconImage.gameObject.SetActive(true);
        }

        /// <summary>
        /// 设置占位图标（根据物品类型显示不同颜色）
        /// </summary>
        /// <param name="configInfo">物品配置信息</param>
        private void SetPlaceholderIcon(ItemConfigInfo configInfo)
        {
            if (mIconImage == null) return;

            // 根据物品类型设置不同颜色
            Color typeColor = (ItemType)configInfo.ItemType switch
            {
                ItemType.Seed => new Color(0.4f, 0.8f, 0.4f, 1f),    // 绿色 - 种子
                ItemType.Product => new Color(1f, 0.6f, 0.2f, 1f),   // 橙色 - 农产品
                ItemType.Tool => new Color(0.5f, 0.5f, 0.8f, 1f),    // 蓝紫色 - 工具
                _ => new Color(0.7f, 0.7f, 0.7f, 1f)                  // 灰色 - 未知
            };

            mIconImage.sprite = null;
            mIconImage.color = typeColor;
        }

        /// <summary>
        /// 更新数量
        /// </summary>
        /// <param name="count">物品数量</param>
        private void UpdateCount(int count)
        {
            if (mCountText == null)
                return;

            if (count > 1)
            {
                mCountText.text = $"x{count}";
                mCountText.gameObject.SetActive(true);
            }
            else
            {
                mCountText.text = "";
                mCountText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 更新名称
        /// </summary>
        /// <param name="name">物品名称</param>
        private void UpdateName(string name)
        {
            if (mNameText == null)
                return;

            mNameText.text = name;
            mNameText.gameObject.SetActive(!string.IsNullOrEmpty(name));
        }

        /// <summary>
        /// 更新描述
        /// </summary>
        /// <param name="description">物品描述</param>
        private void UpdateDescription(string description)
        {
            if (mDescriptionText == null)
                return;

            mDescriptionText.text = description;
            mDescriptionText.gameObject.SetActive(!string.IsNullOrEmpty(description));
        }

        /// <summary>
        /// 重置背景色
        /// </summary>
        private void ResetBackground()
        {
            if (mBackgroundImage != null)
            {
                mBackgroundImage.color = mDefaultBackgroundColor;
            }
        }

        #endregion

        #region 编辑器方法

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器预览更新
        /// </summary>
        public void PreviewUpdate()
        {
            // 在编辑器中预览效果
            if (Application.isPlaying) return;

            // 检查UI组件是否赋值
            if (mIconImage == null)
                mIconImage = GetComponentInChildren<Image>();

            if (mCountText == null)
                mCountText = GetComponentInChildren<Text>();
        }
#endif

        #endregion
    }
}