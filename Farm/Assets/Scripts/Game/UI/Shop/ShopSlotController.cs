using System;
using UnityEngine;
using UnityEngine.UI;
using FarmGame.Shop;
using FarmGame.Item;
using FarmGame.GameConfig;
using FarmGame.GameConfig.Generated;

namespace FarmGame.UI
{
    /// <summary>
    /// 商店格子控制器
    /// 负责显示商品信息和处理点击交互
    /// </summary>
    public class ShopSlotController : MonoBehaviour
    {
        #region UI组件

        [Header("UI组件")]
        [SerializeField]
        private Image mIconImage; // 物品图标

        [SerializeField]
        private Text mNameText; // 物品名称

        [SerializeField]
        private Text mPriceText; // 价格文本

        [SerializeField]
        private Text mCountText; // 数量文本（出售模式用）

        [SerializeField]
        private Image mBackgroundImage; // 背景图片

        [SerializeField]
        private Button mSlotButton; // 格子按钮

        [SerializeField]
        private Image mSelectFrame; // 选中框

        #endregion

        #region 私有字段

        private int mItemId = -1;
        private string mItemName;
        private string mItemDescription;
        private int mPrice;
        private int mCount;
        private Sprite mItemIcon;
        private bool mIsSelected;
        private Color mDefaultBackgroundColor = Color.white;

        #endregion

        #region 公共属性

        /// <summary>物品ID</summary>
        public int ItemId => mItemId;

        /// <summary>物品名称</summary>
        public string ItemName => mItemName;

        /// <summary>物品描述</summary>
        public string ItemDescription => mItemDescription;

        /// <summary>价格</summary>
        public int Price => mPrice;

        /// <summary>数量</summary>
        public int Count => mCount;

        /// <summary>物品图标</summary>
        public Sprite ItemIcon => mItemIcon;

        /// <summary>是否被选中</summary>
        public bool IsSelected => mIsSelected;

        #endregion

        #region 事件

        /// <summary>
        /// 格子点击事件
        /// </summary>
        public event Action<ShopSlotController> OnSlotClicked;

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
            if (mSlotButton != null)
            {
                mSlotButton.onClick.RemoveListener(OnButtonClick);
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置商店商品数据（购买模式）
        /// </summary>
        /// <param name="itemData">商店商品数据</param>
        public void SetShopItem(ShopItemData itemData)
        {
            if (itemData == null || itemData.ItemConfig == null)
            {
                ClearSlot();
                return;
            }

            mItemId = itemData.ItemConfig.class_id;
            mItemName = itemData.ItemConfig.name;
            mItemDescription = itemData.ItemConfig.description;
            mPrice = itemData.BuyPrice;
            mCount = -1; // 购买模式不显示数量

            // 更新图标
            LoadIcon(itemData.ItemConfig.icon);

            // 更新显示
            UpdateDisplay();
        }

        /// <summary>
        /// 设置背包物品数据（出售模式）
        /// </summary>
        /// <param name="item">物品实体</param>
        /// <param name="sellPrice">出售价格</param>
        public void SetInventoryItem(ItemEntity item, int sellPrice)
        {
            if (item == null)
            {
                ClearSlot();
                return;
            }

            mItemId = item.ConfigId;
            mCount = item.Count;
            mPrice = sellPrice;

            // 从配置获取物品信息
            var configInfo = item.ConfigInfo;
            if (configInfo.IsValid)
            {
                mItemName = configInfo.Name;
                mItemDescription = configInfo.Description;
                LoadIcon(configInfo.Icon);
            }
            else
            {
                mItemName = "未知物品";
                mItemDescription = "";
                mItemIcon = null;
            }

            // 更新显示
            UpdateDisplay();
        }

        /// <summary>
        /// 设置选中状态
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

            // 如果没有选中框，使用背景色表示
            if (mSelectFrame == null && mBackgroundImage != null)
            {
                mBackgroundImage.color = selected ? new Color(1f, 0.8f, 0.3f, 1f) : mDefaultBackgroundColor;
            }
        }

        /// <summary>
        /// 清空格子
        /// </summary>
        public void ClearSlot()
        {
            mItemId = -1;
            mItemName = "";
            mItemDescription = "";
            mPrice = 0;
            mCount = 0;
            mItemIcon = null;

            if (mIconImage != null)
            {
                mIconImage.sprite = null;
                mIconImage.gameObject.SetActive(false);
            }

            if (mNameText != null)
            {
                mNameText.text = "";
            }

            if (mPriceText != null)
            {
                mPriceText.text = "";
            }

            if (mCountText != null)
            {
                mCountText.text = "";
                mCountText.gameObject.SetActive(false);
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 加载图标
        /// </summary>
        private void LoadIcon(string iconPath)
        {
            if (string.IsNullOrEmpty(iconPath))
            {
                mItemIcon = null;
                return;
            }

            mItemIcon = Resources.Load<Sprite>(iconPath);
        }

        /// <summary>
        /// 更新显示
        /// </summary>
        private void UpdateDisplay()
        {
            // 更新图标
            if (mIconImage != null)
            {
                if (mItemIcon != null)
                {
                    mIconImage.sprite = mItemIcon;
                    mIconImage.color = Color.white;
                    mIconImage.gameObject.SetActive(true);
                }
                else
                {
                    mIconImage.gameObject.SetActive(false);
                }
            }

            // 更新名称
            if (mNameText != null)
            {
                mNameText.text = mItemName;
            }

            // 更新价格
            if (mPriceText != null)
            {
                mPriceText.text = $"{mPrice}G";
            }

            // 更新数量（仅出售模式显示）
            if (mCountText != null)
            {
                if (mCount > 0)
                {
                    mCountText.text = $"x{mCount}";
                    mCountText.gameObject.SetActive(true);
                }
                else
                {
                    mCountText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 按钮点击处理
        /// </summary>
        private void OnButtonClick()
        {
            OnSlotClicked?.Invoke(this);
        }

        #endregion
    }
}
