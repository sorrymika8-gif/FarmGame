using UnityEngine;
using UnityEngine.UI;
using QFramework;
using FarmGame.Shop;
using FarmGame.Item;
using FarmGame.Player;
using FarmGame.GameConfig;
using FarmGame.GameConfig.Generated;
using System.Collections.Generic;

namespace FarmGame.UI
{
    /// <summary>
    /// 商店UI面板数据
    /// </summary>
    public class ShopPanelData : UIPanelData
    {
        /// <summary>商店类型</summary>
        public ShopType ShopType { get; set; } = ShopType.SeedShop;

        /// <summary>玩家背包组件</summary>
        public InventoryComponent PlayerInventory { get; set; }
    }

    /// <summary>
    /// 商店UI面板
    /// 左侧商品列表 + 右侧详情面板的混合式布局
    /// 支持购买和出售功能
    /// </summary>
    public partial class ShopPanel : UIPanel
    {
        #region 商店模式枚举

        /// <summary>
        /// 商店模式：购买或出售
        /// </summary>
        private enum ShopMode
        {
            Buy,    // 购买模式
            Sell    // 出售模式
        }

        #endregion

        #region UI组件 - 主界面

        [Header("主界面组件")]
        [SerializeField]
        private Button mBackgroundMask; // 背景遮罩

        [SerializeField]
        private Text mTitleText; // 标题文本

        [SerializeField]
        private Button mCloseButton; // 关闭按钮

        [SerializeField]
        private Text mGoldText; // 金币显示

        #endregion

        #region UI组件 - 标签切换

        [Header("标签切换")]
        [SerializeField]
        private Button mBuyTabButton; // 购买标签按钮

        [SerializeField]
        private Button mSellTabButton; // 出售标签按钮

        [SerializeField]
        private Image mBuyTabBg; // 购买标签背景

        [SerializeField]
        private Image mSellTabBg; // 出售标签背景

        #endregion

        #region UI组件 - 商品列表

        [Header("商品列表")]
        [SerializeField]
        private Transform mSlotContainer; // 格子容器

        [SerializeField]
        private GameObject mSlotPrefab; // 格子预制体

        [SerializeField]
        private ScrollRect mScrollRect; // 滚动视图

        #endregion

        #region UI组件 - 详情面板

        [Header("详情面板")]
        [SerializeField]
        private GameObject mDetailPanel; // 详情面板容器

        [SerializeField]
        private Image mDetailIcon; // 详情图标

        [SerializeField]
        private Text mDetailNameText; // 详情名称

        [SerializeField]
        private Text mDetailDescText; // 详情描述

        [SerializeField]
        private Text mDetailPriceText; // 详情价格

        [SerializeField]
        private Text mDetailOwnedText; // 拥有数量

        #endregion

        #region UI组件 - 操作区域

        [Header("操作区域")]
        [SerializeField]
        private Button mDecreaseButton; // 减少数量按钮

        [SerializeField]
        private Button mIncreaseButton; // 增加数量按钮

        [SerializeField]
        private Text mQuantityText; // 数量文本

        [SerializeField]
        private InputField mQuantityInput; // 数量输入框

        [SerializeField]
        private Text mTotalPriceText; // 总价文本

        [SerializeField]
        private Button mActionButton; // 操作按钮（购买/出售）

        [SerializeField]
        private Text mActionButtonText; // 操作按钮文本

        #endregion

        #region 私有字段

        private ShopPanelData mData;
        private ShopMode mCurrentMode = ShopMode.Buy;
        private List<ShopSlotController> mSlots = new List<ShopSlotController>();
        private ShopSlotController mSelectedSlot;

        // 当前选中商品信息
        private int mSelectedItemId = -1;
        private int mSelectedPrice = 0;
        private int mCurrentQuantity = 1;
        private int mMaxQuantity = 99;

        // 标签页颜色
        private Color mTabActiveColor = new Color(1f, 0.9f, 0.6f, 1f);
        private Color mTabInactiveColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        #endregion

        #region UIPanel生命周期

        /// <summary>
        /// 初始化商店面板
        /// </summary>
        protected override void OnInit(IUIData uiData = null)
        {
            mData = uiData as ShopPanelData ?? new ShopPanelData();

            // 初始化UI组件
            InitializeUIComponents();

            // 初始化详情面板
            InitializeDetailPanel();

            // 订阅事件
            SubscribeEvents();

            // 默认显示购买模式
            SwitchMode(ShopMode.Buy);
        }

        /// <summary>
        /// 打开商店面板
        /// </summary>
        protected override void OnOpen(IUIData uiData = null)
        {
            // 更新标题
            UpdateTitle();

            // 刷新金币显示
            RefreshGoldDisplay();

            // 刷新商品列表
            RefreshItemList();
        }

        /// <summary>
        /// 关闭商店面板
        /// </summary>
        protected override void OnClose()
        {
            // 取消事件订阅
            UnsubscribeEvents();

            // 清除选中状态
            ClearSelection();

            // 清理格子
            ClearSlots();
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化UI组件
        /// </summary>
        private void InitializeUIComponents()
        {
            // 背景遮罩点击关闭
            if (mBackgroundMask != null)
            {
                mBackgroundMask.onClick.AddListener(OnCloseClicked);
            }

            // 关闭按钮
            if (mCloseButton != null)
            {
                mCloseButton.onClick.AddListener(OnCloseClicked);
            }

            // 标签按钮
            if (mBuyTabButton != null)
            {
                mBuyTabButton.onClick.AddListener(OnBuyTabClicked);
            }

            if (mSellTabButton != null)
            {
                mSellTabButton.onClick.AddListener(OnSellTabClicked);
            }

            // 数量调整按钮
            if (mDecreaseButton != null)
            {
                mDecreaseButton.onClick.AddListener(OnDecreaseQuantity);
            }

            if (mIncreaseButton != null)
            {
                mIncreaseButton.onClick.AddListener(OnIncreaseQuantity);
            }

            // 数量输入框
            if (mQuantityInput != null)
            {
                mQuantityInput.onEndEdit.AddListener(OnQuantityInputChanged);
            }

            // 操作按钮
            if (mActionButton != null)
            {
                mActionButton.onClick.AddListener(OnActionButtonClicked);
            }
        }

        /// <summary>
        /// 初始化详情面板
        /// </summary>
        private void InitializeDetailPanel()
        {
            if (mDetailPanel != null)
            {
                mDetailPanel.SetActive(false);
            }
            SetActionButtonEnabled(false);
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        private void SubscribeEvents()
        {
            ShopManager.Instance.OnItemBought += OnItemBought;
            ShopManager.Instance.OnItemSold += OnItemSold;
        }

        /// <summary>
        /// 取消事件订阅
        /// </summary>
        private void UnsubscribeEvents()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnItemBought -= OnItemBought;
                ShopManager.Instance.OnItemSold -= OnItemSold;
            }
        }

        #endregion

        #region 标签切换

        /// <summary>
        /// 切换商店模式
        /// </summary>
        private void SwitchMode(ShopMode mode)
        {
            mCurrentMode = mode;

            // 更新标签显示
            UpdateTabDisplay();

            // 清除选中
            ClearSelection();

            // 刷新列表
            RefreshItemList();
        }

        /// <summary>
        /// 更新标签显示
        /// </summary>
        private void UpdateTabDisplay()
        {
            if (mBuyTabBg != null)
            {
                mBuyTabBg.color = mCurrentMode == ShopMode.Buy ? mTabActiveColor : mTabInactiveColor;
            }

            if (mSellTabBg != null)
            {
                mSellTabBg.color = mCurrentMode == ShopMode.Sell ? mTabActiveColor : mTabInactiveColor;
            }

            // 更新操作按钮文本
            if (mActionButtonText != null)
            {
                mActionButtonText.text = mCurrentMode == ShopMode.Buy ? "购买" : "出售";
            }
        }

        #endregion

        #region 商品列表

        /// <summary>
        /// 刷新商品列表
        /// </summary>
        private void RefreshItemList()
        {
            ClearSlots();

            if (mCurrentMode == ShopMode.Buy)
            {
                RefreshBuyList();
            }
            else
            {
                RefreshSellList();
            }
        }

        /// <summary>
        /// 刷新购买列表
        /// </summary>
        private void RefreshBuyList()
        {
            var shopItems = ShopManager.Instance.GetShopItems(mData.ShopType);

            foreach (var itemData in shopItems)
            {
                CreateShopSlot(itemData);
            }
        }

        /// <summary>
        /// 刷新出售列表（玩家背包物品）
        /// </summary>
        private void RefreshSellList()
        {
            var inventory = mData.PlayerInventory;
            if (inventory == null)
            {
                inventory = PlayerManager.Instance?.Player?.Inventory;
            }

            if (inventory == null) return;

            var items = inventory.GetAllItems();
            foreach (var item in items)
            {
                if (item == null || item.Count <= 0) continue;

                int sellPrice = ShopManager.Instance.GetSellPrice(item.ConfigId);
                if (sellPrice > 0)
                {
                    CreateInventorySlot(item, sellPrice);
                }
            }
        }

        /// <summary>
        /// 创建商店商品格子
        /// </summary>
        private void CreateShopSlot(ShopItemData itemData)
        {
            if (mSlotPrefab == null || mSlotContainer == null) return;

            var slotObj = Instantiate(mSlotPrefab, mSlotContainer);
            var slotController = slotObj.GetComponent<ShopSlotController>();

            if (slotController != null)
            {
                slotController.SetShopItem(itemData);
                slotController.OnSlotClicked += OnSlotSelected;
                mSlots.Add(slotController);
            }
        }

        /// <summary>
        /// 创建背包物品格子（用于出售）
        /// </summary>
        private void CreateInventorySlot(ItemEntity item, int sellPrice)
        {
            if (mSlotPrefab == null || mSlotContainer == null) return;

            var slotObj = Instantiate(mSlotPrefab, mSlotContainer);
            var slotController = slotObj.GetComponent<ShopSlotController>();

            if (slotController != null)
            {
                slotController.SetInventoryItem(item, sellPrice);
                slotController.OnSlotClicked += OnSlotSelected;
                mSlots.Add(slotController);
            }
        }

        /// <summary>
        /// 清理所有格子
        /// </summary>
        private void ClearSlots()
        {
            foreach (var slot in mSlots)
            {
                if (slot != null)
                {
                    slot.OnSlotClicked -= OnSlotSelected;
                    Destroy(slot.gameObject);
                }
            }
            mSlots.Clear();
        }

        #endregion

        #region 选中与详情

        /// <summary>
        /// 格子选中回调
        /// </summary>
        private void OnSlotSelected(ShopSlotController slot)
        {
            // 取消之前的选中
            if (mSelectedSlot != null && mSelectedSlot != slot)
            {
                mSelectedSlot.SetSelected(false);
            }

            // 设置新选中
            mSelectedSlot = slot;
            mSelectedSlot.SetSelected(true);

            // 更新详情面板
            UpdateDetailPanel(slot);
        }

        /// <summary>
        /// 更新详情面板
        /// </summary>
        private void UpdateDetailPanel(ShopSlotController slot)
        {
            if (mDetailPanel != null)
            {
                mDetailPanel.SetActive(true);
            }

            mSelectedItemId = slot.ItemId;
            mSelectedPrice = slot.Price;

            // 更新图标
            if (mDetailIcon != null)
            {
                mDetailIcon.sprite = slot.ItemIcon;
                mDetailIcon.gameObject.SetActive(slot.ItemIcon != null);
            }

            // 更新名称
            if (mDetailNameText != null)
            {
                mDetailNameText.text = slot.ItemName;
            }

            // 更新描述
            if (mDetailDescText != null)
            {
                mDetailDescText.text = slot.ItemDescription;
            }

            // 更新价格
            if (mDetailPriceText != null)
            {
                string priceLabel = mCurrentMode == ShopMode.Buy ? "购买价格" : "出售价格";
                mDetailPriceText.text = $"{priceLabel}: {mSelectedPrice} 金币";
            }

            // 更新拥有数量
            UpdateOwnedCount();

            // 计算最大可购买/出售数量
            CalculateMaxQuantity();

            // 重置数量为1
            SetQuantity(1);

            // 启用操作按钮
            SetActionButtonEnabled(true);
        }

        /// <summary>
        /// 更新拥有数量显示
        /// </summary>
        private void UpdateOwnedCount()
        {
            if (mDetailOwnedText == null) return;

            var inventory = mData.PlayerInventory ?? PlayerManager.Instance?.Player?.Inventory;
            if (inventory != null && mSelectedItemId > 0)
            {
                int count = inventory.GetItemCount(mSelectedItemId);
                mDetailOwnedText.text = $"拥有: {count}";
            }
            else
            {
                mDetailOwnedText.text = "拥有: 0";
            }
        }

        /// <summary>
        /// 计算最大可交易数量
        /// </summary>
        private void CalculateMaxQuantity()
        {
            if (mCurrentMode == ShopMode.Buy)
            {
                // 购买模式：根据金币计算
                int playerGold = PlayerManager.Instance?.Gold ?? 0;
                if (mSelectedPrice > 0)
                {
                    mMaxQuantity = Mathf.Max(1, playerGold / mSelectedPrice);
                }
                else
                {
                    mMaxQuantity = 99;
                }
            }
            else
            {
                // 出售模式：根据拥有数量
                var inventory = mData.PlayerInventory ?? PlayerManager.Instance?.Player?.Inventory;
                if (inventory != null)
                {
                    mMaxQuantity = Mathf.Max(1, inventory.GetItemCount(mSelectedItemId));
                }
                else
                {
                    mMaxQuantity = 1;
                }
            }

            // 限制最大99
            mMaxQuantity = Mathf.Min(mMaxQuantity, 99);
        }

        /// <summary>
        /// 清除选中状态
        /// </summary>
        private void ClearSelection()
        {
            if (mSelectedSlot != null)
            {
                mSelectedSlot.SetSelected(false);
                mSelectedSlot = null;
            }

            mSelectedItemId = -1;
            mSelectedPrice = 0;
            mCurrentQuantity = 1;

            if (mDetailPanel != null)
            {
                mDetailPanel.SetActive(false);
            }

            SetActionButtonEnabled(false);
        }

        #endregion

        #region 数量调整

        /// <summary>
        /// 设置数量
        /// </summary>
        private void SetQuantity(int quantity)
        {
            mCurrentQuantity = Mathf.Clamp(quantity, 1, mMaxQuantity);

            // 更新显示
            if (mQuantityText != null)
            {
                mQuantityText.text = mCurrentQuantity.ToString();
            }

            if (mQuantityInput != null)
            {
                mQuantityInput.text = mCurrentQuantity.ToString();
            }

            // 更新总价
            UpdateTotalPrice();

            // 更新按钮状态
            UpdateQuantityButtonStates();
        }

        /// <summary>
        /// 更新总价显示
        /// </summary>
        private void UpdateTotalPrice()
        {
            if (mTotalPriceText != null)
            {
                int total = mSelectedPrice * mCurrentQuantity;
                mTotalPriceText.text = $"总计: {total} 金币";
            }
        }

        /// <summary>
        /// 更新数量按钮状态
        /// </summary>
        private void UpdateQuantityButtonStates()
        {
            if (mDecreaseButton != null)
            {
                mDecreaseButton.interactable = mCurrentQuantity > 1;
            }

            if (mIncreaseButton != null)
            {
                mIncreaseButton.interactable = mCurrentQuantity < mMaxQuantity;
            }
        }

        #endregion

        #region UI刷新

        /// <summary>
        /// 更新标题
        /// </summary>
        private void UpdateTitle()
        {
            if (mTitleText == null) return;

            string shopName = mData.ShopType switch
            {
                ShopType.SeedShop => "种子商店",
                ShopType.ToolShop => "工具商店",
                ShopType.GeneralShop => "杂货商店",
                _ => "商店"
            };

            mTitleText.text = shopName;
        }

        /// <summary>
        /// 刷新金币显示
        /// </summary>
        private void RefreshGoldDisplay()
        {
            if (mGoldText != null)
            {
                int gold = PlayerManager.Instance?.Gold ?? 0;
                mGoldText.text = $"金币: {gold}";
            }
        }

        /// <summary>
        /// 设置操作按钮状态
        /// </summary>
        private void SetActionButtonEnabled(bool enabled)
        {
            if (mActionButton != null)
            {
                mActionButton.interactable = enabled;
            }
        }

        #endregion

        #region 事件回调

        /// <summary>
        /// 关闭按钮点击
        /// </summary>
        private void OnCloseClicked()
        {
            CloseSelf();
        }

        /// <summary>
        /// 购买标签点击
        /// </summary>
        private void OnBuyTabClicked()
        {
            if (mCurrentMode != ShopMode.Buy)
            {
                SwitchMode(ShopMode.Buy);
            }
        }

        /// <summary>
        /// 出售标签点击
        /// </summary>
        private void OnSellTabClicked()
        {
            if (mCurrentMode != ShopMode.Sell)
            {
                SwitchMode(ShopMode.Sell);
            }
        }

        /// <summary>
        /// 减少数量按钮点击
        /// </summary>
        private void OnDecreaseQuantity()
        {
            SetQuantity(mCurrentQuantity - 1);
        }

        /// <summary>
        /// 增加数量按钮点击
        /// </summary>
        private void OnIncreaseQuantity()
        {
            SetQuantity(mCurrentQuantity + 1);
        }

        /// <summary>
        /// 数量输入框变更
        /// </summary>
        private void OnQuantityInputChanged(string text)
        {
            if (int.TryParse(text, out int quantity))
            {
                SetQuantity(quantity);
            }
            else
            {
                SetQuantity(mCurrentQuantity);
            }
        }

        /// <summary>
        /// 操作按钮点击（购买/出售）
        /// </summary>
        private void OnActionButtonClicked()
        {
            if (mSelectedItemId <= 0) return;

            bool success;
            if (mCurrentMode == ShopMode.Buy)
            {
                success = ShopManager.Instance.BuyItem(mSelectedItemId, mCurrentQuantity, (int)mData.ShopType);
                if (!success)
                {
                    ShowMessage("购买失败，金币不足或背包已满");
                }
            }
            else
            {
                success = ShopManager.Instance.SellItem(mSelectedItemId, mCurrentQuantity);
                if (!success)
                {
                    ShowMessage("出售失败");
                }
            }
        }

        /// <summary>
        /// 物品购买成功回调
        /// </summary>
        private void OnItemBought(int itemId, int count, int cost)
        {
            RefreshGoldDisplay();
            UpdateOwnedCount();
            CalculateMaxQuantity();
            SetQuantity(Mathf.Min(mCurrentQuantity, mMaxQuantity));
            ShowMessage($"购买成功！花费 {cost} 金币");
        }

        /// <summary>
        /// 物品出售成功回调
        /// </summary>
        private void OnItemSold(int itemId, int count, int earning)
        {
            RefreshGoldDisplay();
            RefreshItemList(); // 重新加载出售列表
            ClearSelection();
            ShowMessage($"出售成功！获得 {earning} 金币");
        }

        /// <summary>
        /// 显示提示消息
        /// </summary>
        private void ShowMessage(string message)
        {
            Debug.Log($"[ShopPanel] {message}");
            // TODO: 可以扩展为显示Toast或弹窗
        }

        #endregion
    }
}
