using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using FarmGame.Farm;
using FarmGame.Item;
using FarmGame.GameConfig;
using FarmGame.GameConfig.Generated;

namespace FarmGame.UI
{
    /// <summary>
    /// 农场右键菜单数据
    /// </summary>
    public class FarmContextMenuData : UIPanelData
    {
        /// <summary>
        /// 目标土地
        /// </summary>
        public SoilEntity Soil { get; set; }

        /// <summary>
        /// 玩家背包
        /// </summary>
        public InventoryComponent Inventory { get; set; }

        /// <summary>
        /// 菜单显示的屏幕位置
        /// </summary>
        public Vector2 ScreenPosition { get; set; }
    }

    /// <summary>
    /// 农场种子选择面板
    /// 点击未播种土地时弹出，用于选择种子进行种植
    /// </summary>
    public class FarmContextMenuPanel : UIPanel
    {
        #region 序列化字段

        [Header("基础")]
        [SerializeField] private Button mBackgroundMask;

        [Header("种子选择面板")]
        [SerializeField] private GameObject mSeedPanel;
        [SerializeField] private Transform mSeedContainer;
        [SerializeField] private GameObject mSeedItemPrefab;
        [SerializeField] private GameObject mEmptyHint;

        #endregion

        #region 私有字段

        private FarmContextMenuData mData;
        private List<SeedItem> mSeedItems = new List<SeedItem>();

        #endregion

        #region UIPanel 生命周期

        protected override void OnInit(IUIData uiData = null)
        {
            // 绑定背景遮罩点击关闭
            if (mBackgroundMask != null)
            {
                mBackgroundMask.onClick.RemoveAllListeners();
                mBackgroundMask.onClick.AddListener(CloseSelf);
            }

            // 默认隐藏种子面板
            if (mSeedPanel != null)
                mSeedPanel.SetActive(false);
        }

        protected override void OnOpen(IUIData uiData = null)
        {
            Debug.Log("[FarmContextMenuPanel] OnOpen 被调用");
            
            mData = uiData as FarmContextMenuData;
            if (mData == null || mData.Soil == null)
            {
                Debug.LogWarning("[FarmContextMenuPanel] 无效的数据");
                CloseSelf();
                return;
            }

            Debug.Log($"[FarmContextMenuPanel] 土地位置: {mData.Soil.GridPos}, HasPlant: {mData.Soil.HasPlant}");

            var soil = mData.Soil;

            // 未播种 → 直接显示种子选择面板
            if (!soil.HasPlant)
            {
                Debug.Log("[FarmContextMenuPanel] 未播种，显示种子选择面板");
                ShowSeedSelection();
            }
            else
            {
                // 有作物 → 关闭菜单（由 CropDetailBubblePanel 处理）
                Debug.Log("[FarmContextMenuPanel] 土地有作物，关闭菜单");
                CloseSelf();
            }
        }

        protected override void OnClose()
        {
            ClearSeedItems();
            mData = null;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 显示种子选择面板
        /// </summary>
        private void ShowSeedSelection()
        {
            if (mSeedPanel == null)
            {
                Debug.LogWarning("[FarmContextMenuPanel] SeedPanel 未设置");
                return;
            }

            // 设置SeedPanel位置
            SetSeedPanelPosition(mData.ScreenPosition);
            
            RefreshSeedItems();
            mSeedPanel.SetActive(true);
        }

        /// <summary>
        /// 设置种子面板位置
        /// </summary>
        private void SetSeedPanelPosition(Vector2 screenPos)
        {
            var seedRect = mSeedPanel.GetComponent<RectTransform>();
            if (seedRect == null) return;

            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos,
                canvas.worldCamera,
                out Vector2 localPoint
            );

            // 确保面板不超出屏幕边界
            var canvasRect = canvas.transform as RectTransform;
            var panelSize = seedRect.sizeDelta;

            // 调整X位置
            if (localPoint.x + panelSize.x > canvasRect.rect.width / 2)
                localPoint.x -= panelSize.x;

            // 调整Y位置
            if (localPoint.y - panelSize.y < -canvasRect.rect.height / 2)
                localPoint.y += panelSize.y;

            seedRect.anchoredPosition = localPoint;
        }

        /// <summary>
        /// 刷新种子列表
        /// </summary>
        private void RefreshSeedItems()
        {
            ClearSeedItems();

            if (mData?.Inventory == null)
            {
                Debug.LogWarning("[FarmContextMenuPanel] 背包组件为空");
                return;
            }

            // 从背包中筛选种子类型物品
            var seedItems = mData.Inventory.GetAllItems()
                .Where(item =>
                {
                    var configInfo = ItemConfigHelper.GetConfigInfo(item.ConfigId);
                    return configInfo.IsValid && (ItemType)configInfo.ItemType == ItemType.Seed;
                })
                .ToList();

            // 显示/隐藏空状态提示
            if (mEmptyHint != null)
            {
                mEmptyHint.SetActive(seedItems.Count == 0);
            }

            if (seedItems.Count == 0)
            {
                Debug.Log("[FarmContextMenuPanel] 背包中没有种子");
                return;
            }

            // 创建种子条目
            foreach (var item in seedItems)
            {
                CreateSeedItem(item);
            }
        }

        /// <summary>
        /// 创建种子条目
        /// </summary>
        private void CreateSeedItem(ItemEntity item)
        {
            if (mSeedItemPrefab == null || mSeedContainer == null)
            {
                Debug.LogWarning("[FarmContextMenuPanel] SeedItem预制体或容器未设置");
                return;
            }

            var configInfo = ItemConfigHelper.GetConfigInfo(item.ConfigId);
            if (!configInfo.IsValid) return;

            var go = Instantiate(mSeedItemPrefab, mSeedContainer);
            var seedItem = go.GetComponent<SeedItem>();

            if (seedItem != null)
            {
                seedItem.Setup(configInfo, item.Count, OnSeedSelected);
                mSeedItems.Add(seedItem);
            }
        }

        /// <summary>
        /// 清除种子条目
        /// </summary>
        private void ClearSeedItems()
        {
            foreach (var item in mSeedItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            mSeedItems.Clear();
        }

        /// <summary>
        /// 种子选择回调
        /// </summary>
        private void OnSeedSelected(int seedItemId)
        {
            ExecutePlant(seedItemId);
        }

        /// <summary>
        /// 执行种植
        /// </summary>
        private void ExecutePlant(int seedItemId)
        {
            if (mData?.Soil == null || mData.Inventory == null) return;

            bool success = FarmManager.Instance.Plant(mData.Soil, seedItemId, mData.Inventory);
            if (success)
            {
                Debug.Log($"[FarmContextMenuPanel] 种植成功: itemId={seedItemId}");
            }

            CloseSelf();
        }

        #endregion
    }
}

