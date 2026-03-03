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
            
            // 立即设置LayoutElement，确保在父布局计算前生效
            SetupLayoutElement();
        }
        
        /// <summary>
        /// 设置LayoutElement组件
        /// </summary>
        private void SetupLayoutElement()
        {
            var layoutElement = GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = gameObject.AddComponent<LayoutElement>();
            }
            layoutElement.preferredHeight = 60f;
            layoutElement.minHeight = 60f;
            layoutElement.flexibleHeight = 0f;
            layoutElement.flexibleWidth = 1f;
            layoutElement.ignoreLayout = false;
            
            // 直接设置RectTransform高度
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 60f);
            }
        }

        /// <summary>
        /// 转换为横向列表布局（图标在左，名称在中，价格在右）
        /// </summary>
        public void ConvertToHorizontalListLayout()
        {
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) return;
            
            // 设置锚点为水平拉伸，保持固定高度（由ShopPanel设置）
            // 不在这里修改锚点，由ShopPanel控制
            
            // 设置固定高度
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 60f);

            // 设置固定高度，宽度自适应
            var layoutElement = GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = gameObject.AddComponent<LayoutElement>();
            }
            layoutElement.preferredHeight = 60f;
            layoutElement.minHeight = 60f;
            layoutElement.flexibleHeight = 0f; // 不允许垂直拉伸
            layoutElement.flexibleWidth = 1f;
            layoutElement.ignoreLayout = false; // 确保参与布局

            // 首先设置 ignoreLayout 的元素，避免它们干扰布局
            // 背景图片不参与布局
            if (mBackgroundImage != null)
            {
                var bgLayout = mBackgroundImage.GetComponent<LayoutElement>();
                if (bgLayout == null)
                {
                    bgLayout = mBackgroundImage.gameObject.AddComponent<LayoutElement>();
                }
                bgLayout.ignoreLayout = true;
                
                var bgRect = mBackgroundImage.GetComponent<RectTransform>();
                if (bgRect != null)
                {
                    bgRect.anchorMin = Vector2.zero;
                    bgRect.anchorMax = Vector2.one;
                    bgRect.sizeDelta = Vector2.zero;
                    bgRect.anchoredPosition = Vector2.zero;
                }
                // 确保背景在最底层
                mBackgroundImage.transform.SetAsFirstSibling();
            }

            // 选中框不参与布局
            if (mSelectFrame != null)
            {
                var selectLayout = mSelectFrame.GetComponent<LayoutElement>();
                if (selectLayout == null)
                {
                    selectLayout = mSelectFrame.gameObject.AddComponent<LayoutElement>();
                }
                selectLayout.ignoreLayout = true;
                
                var selectRect = mSelectFrame.GetComponent<RectTransform>();
                if (selectRect != null)
                {
                    selectRect.anchorMin = Vector2.zero;
                    selectRect.anchorMax = Vector2.one;
                    selectRect.sizeDelta = Vector2.zero;
                    selectRect.anchoredPosition = Vector2.zero;
                }
                // 选中框在背景之上
                mSelectFrame.transform.SetSiblingIndex(1);
            }

            // 不使用HorizontalLayoutGroup，直接设置子元素位置
            // 调整图标布局
            if (mIconImage != null)
            {
                var iconRect = mIconImage.GetComponent<RectTransform>();
                if (iconRect != null)
                {
                    iconRect.anchorMin = new Vector2(0, 0.5f);
                    iconRect.anchorMax = new Vector2(0, 0.5f);
                    iconRect.pivot = new Vector2(0, 0.5f);
                    iconRect.anchoredPosition = new Vector2(10, 0);
                    iconRect.sizeDelta = new Vector2(50, 50);
                }
                var iconLayout = mIconImage.GetComponent<LayoutElement>();
                if (iconLayout != null) DestroyImmediate(iconLayout);
            }

            // 调整名称布局
            if (mNameText != null)
            {
                var nameRect = mNameText.GetComponent<RectTransform>();
                if (nameRect != null)
                {
                    nameRect.anchorMin = new Vector2(0, 0);
                    nameRect.anchorMax = new Vector2(1, 1);
                    nameRect.pivot = new Vector2(0.5f, 0.5f);
                    nameRect.offsetMin = new Vector2(70, 5); // 左边距离图标
                    nameRect.offsetMax = new Vector2(-90, -5); // 右边留给价格
                }
                mNameText.alignment = TextAnchor.MiddleLeft;
                mNameText.fontSize = 18;
                var nameLayout = mNameText.GetComponent<LayoutElement>();
                if (nameLayout != null) DestroyImmediate(nameLayout);
            }

            // 调整价格布局
            if (mPriceText != null)
            {
                var priceRect = mPriceText.GetComponent<RectTransform>();
                if (priceRect != null)
                {
                    priceRect.anchorMin = new Vector2(1, 0.5f);
                    priceRect.anchorMax = new Vector2(1, 0.5f);
                    priceRect.pivot = new Vector2(1, 0.5f);
                    priceRect.anchoredPosition = new Vector2(-10, 0);
                    priceRect.sizeDelta = new Vector2(80, 30);
                }
                mPriceText.alignment = TextAnchor.MiddleRight;
                mPriceText.fontSize = 16;
                var priceLayout = mPriceText.GetComponent<LayoutElement>();
                if (priceLayout != null) DestroyImmediate(priceLayout);
            }

            // 隐藏数量文本
            if (mCountText != null)
            {
                mCountText.gameObject.SetActive(false);
            }
            
            // 移除HorizontalLayoutGroup（如果存在）
            var horizontalLayout = GetComponent<HorizontalLayoutGroup>();
            if (horizontalLayout != null)
            {
                DestroyImmediate(horizontalLayout);
            }
            
            Debug.Log($"[ShopSlot] 布局转换完成，LayoutElement.preferredHeight={layoutElement.preferredHeight}, minHeight={layoutElement.minHeight}");
        }

        /// <summary>
        /// 适配格子内部元素尺寸（保留兼容旧模式）
        /// </summary>
        private void AdaptInternalElements()
        {
            // 横向列表模式下不需要这个适配
        }

        private void Start()
        {
            // 延迟一帧再次适配，确保GridLayoutGroup已设置好尺寸
            StartCoroutine(DelayedAdapt());
        }

        private System.Collections.IEnumerator DelayedAdapt()
        {
            yield return null; // 等待下一帧
            AdaptInternalElements();
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
            if (itemData == null || !itemData.ItemConfigInfo.IsValid)
            {
                ClearSlot();
                return;
            }

            mItemId = itemData.ItemId;
            mItemName = itemData.ItemName;
            mItemDescription = itemData.ItemDescription;
            mPrice = itemData.BuyPrice;
            mCount = -1; // 购买模式不显示数量

            // 更新图标
            LoadIcon(itemData.ItemIcon);

            // 更新显示
            UpdateDisplay();
            
            // 设置内部元素布局
            ConvertToHorizontalListLayout();
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
            
            // 设置内部元素布局
            ConvertToHorizontalListLayout();
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
