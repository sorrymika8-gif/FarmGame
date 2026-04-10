using UnityEngine;
using UnityEngine.UI;
using QFramework;
using FarmGame.Shop;
using FarmGame.Item;
using FarmGame.Player;
using FarmGame.GameConfig;
using FarmGame.GameConfig.Generated;
using System.Collections;
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

        #region UI组件 - 适配相关

        [Header("适配相关")]
        [SerializeField]
        private RectTransform mMainPanel; // 主面板RectTransform

        [SerializeField]
        private GridLayoutGroup mGridLayout; // 格子容器GridLayoutGroup

        #endregion

        #region UI组件 - 店主显示

        [Header("店主显示")]
        [SerializeField]
        private Image mShopkeeperPortrait; // 店主立绘

        [SerializeField]
        private Text mShopkeeperDialogue; // 店主对话文本

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

        // 适配参数
        private const float MIN_PANEL_WIDTH = 600f;
        private const float MAX_PANEL_WIDTH = 1200f;
        private const float MIN_PANEL_HEIGHT = 400f;
        private const float MAX_PANEL_HEIGHT = 800f;
        private const float PANEL_WIDTH_RATIO = 0.85f;  // 面板占屏幕宽度比例
        private const float PANEL_HEIGHT_RATIO = 0.80f; // 面板占屏幕高度比例

        // 店主相关
        private const string SHOPKEEPER_ICON_PATH = "prefabs/npcs/ellie/ellie_icon";

        #endregion

        #region UIPanel生命周期

        /// <summary>
        /// 初始化商店面板
        /// </summary>
        protected override void OnInit(IUIData uiData = null)
        {
            mData = uiData as ShopPanelData ?? new ShopPanelData();

            // 自动获取适配相关组件
            AutoFindAdaptComponents();

            // 应用屏幕适配
            ApplyScreenAdaptation();

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

            // 加载店主立绘
            LoadShopkeeperPortrait();
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
        /// 自动查找适配相关的组件（如果未在Inspector中指定）
        /// </summary>
        private void AutoFindAdaptComponents()
        {
            // 查找MainPanel（主面板）
            if (mMainPanel == null)
            {
                var mainPanelObj = transform.Find("MainPanel");
                if (mainPanelObj != null)
                {
                    mMainPanel = mainPanelObj.GetComponent<RectTransform>();
                }
            }

            // 查找GridLayoutGroup（格子容器）
            if (mGridLayout == null && mSlotContainer != null)
            {
                mGridLayout = mSlotContainer.GetComponent<GridLayoutGroup>();
            }

            // 修复ScrollView缺失的组件
            FixScrollViewComponents();
        }

        /// <summary>
        /// 修复ScrollView缺失的组件（ContentSizeFitter, Mask, Viewport）
        /// </summary>
        private void FixScrollViewComponents()
        {
            if (mScrollRect == null)
            {
                Debug.LogWarning("[ShopPanel] ScrollRect为空，无法修复ScrollView组件");
                return;
            }

            var scrollViewObj = mScrollRect.gameObject;

            // 1. 修复Mask组件 - 确保内容被正确裁剪
            var mask = scrollViewObj.GetComponent<Mask>();
            var rectMask2D = scrollViewObj.GetComponent<RectMask2D>();
            if (mask == null && rectMask2D == null)
            {
                // 优先使用RectMask2D，性能更好
                rectMask2D = scrollViewObj.AddComponent<RectMask2D>();
                Debug.Log("[ShopPanel] 已添加RectMask2D组件到ScrollView");
            }

            // 2. 修复Content的ContentSizeFitter - 确保Content高度能自动调整
            if (mSlotContainer != null)
            {
                var contentSizeFitter = mSlotContainer.GetComponent<ContentSizeFitter>();
                if (contentSizeFitter == null)
                {
                    contentSizeFitter = mSlotContainer.gameObject.AddComponent<ContentSizeFitter>();
                    contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    Debug.Log("[ShopPanel] 已添加ContentSizeFitter组件到Content");
                }

                // 确保Content的RectTransform设置正确
                var contentRect = mSlotContainer.GetComponent<RectTransform>();
                if (contentRect != null)
                {
                    // 设置锚点为顶部拉伸
                    contentRect.anchorMin = new Vector2(0, 1);
                    contentRect.anchorMax = new Vector2(1, 1);
                    contentRect.pivot = new Vector2(0, 1);
                    contentRect.anchoredPosition = Vector2.zero;
                }
            }

            // 3. 修复ScrollRect的Viewport引用
            if (mScrollRect.viewport == null)
            {
                // 直接使用ScrollView自身的RectTransform作为viewport
                mScrollRect.viewport = scrollViewObj.GetComponent<RectTransform>();
                Debug.Log("[ShopPanel] 已设置ScrollRect.viewport引用");
            }

            Debug.Log("[ShopPanel] ScrollView组件修复完成");
        }

        /// <summary>
        /// 应用屏幕适配
        /// </summary>
        private void ApplyScreenAdaptation()
        {
            if (mMainPanel == null)
            {
                Debug.LogWarning("[ShopPanel] MainPanel未找到，无法进行屏幕适配");
                return;
            }

            // 获取Canvas的尺寸
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            var canvasRect = canvas.GetComponent<RectTransform>();
            float canvasWidth = canvasRect.rect.width;
            float canvasHeight = canvasRect.rect.height;

            // 如果Canvas尺寸无效，使用屏幕尺寸
            if (canvasWidth <= 0 || canvasHeight <= 0)
            {
                canvasWidth = Screen.width;
                canvasHeight = Screen.height;
            }

            // 计算合适的面板尺寸
            float targetWidth = Mathf.Clamp(canvasWidth * PANEL_WIDTH_RATIO, MIN_PANEL_WIDTH, MAX_PANEL_WIDTH);
            float targetHeight = Mathf.Clamp(canvasHeight * PANEL_HEIGHT_RATIO, MIN_PANEL_HEIGHT, MAX_PANEL_HEIGHT);

            // 应用到MainPanel
            mMainPanel.sizeDelta = new Vector2(targetWidth, targetHeight);

            // 调整GridLayoutGroup的格子大小
            AdaptGridLayout(targetWidth, targetHeight);

            Debug.Log($"[ShopPanel] 屏幕适配完成: Canvas({canvasWidth}x{canvasHeight}) -> Panel({targetWidth}x{targetHeight})");
        }

        /// <summary>
        /// 适配列表布局（单个商品占满一行）
        /// </summary>
        private void AdaptGridLayout(float panelWidth, float panelHeight)
        {
            // 设置ScrollView为左侧60%
            AdaptScrollViewForListLayout();

            // 设置DetailPanel为右侧40%
            AdaptDetailPanelForListLayout();

            // 将GridLayoutGroup转换为VerticalLayoutGroup（列表模式）
            ConvertToVerticalLayout();
        }

        /// <summary>
        /// 设置ScrollView为左侧60%布局
        /// </summary>
        private void AdaptScrollViewForListLayout()
        {
            if (mScrollRect == null) return;

            var scrollViewRect = mScrollRect.GetComponent<RectTransform>();
            if (scrollViewRect != null)
            {
                // 重置pivot为左下角，方便计算
                scrollViewRect.pivot = new Vector2(0, 0);
                
                // 左侧区域占60%宽度，从底部到顶部（留出标题栏空间）
                scrollViewRect.anchorMin = new Vector2(0, 0);
                scrollViewRect.anchorMax = new Vector2(0.6f, 1);
                
                // offsetMin = (left, bottom), offsetMax = (right, top)
                // 左边距10，底部边距10，右边距-5，顶部边距-70（给标题栏留空间）
                scrollViewRect.offsetMin = new Vector2(10, 10);
                scrollViewRect.offsetMax = new Vector2(-5, -70);
                
                Debug.Log($"[ShopPanel] ScrollView布局设置完成: anchorMin={scrollViewRect.anchorMin}, anchorMax={scrollViewRect.anchorMax}, offsetMin={scrollViewRect.offsetMin}, offsetMax={scrollViewRect.offsetMax}, rect={scrollViewRect.rect}");
            }
        }

        /// <summary>
        /// 设置DetailPanel为右侧40%布局
        /// </summary>
        private void AdaptDetailPanelForListLayout()
        {
            if (mDetailPanel == null) return;

            var detailRect = mDetailPanel.GetComponent<RectTransform>();
            if (detailRect != null)
            {
                // 重置pivot
                detailRect.pivot = new Vector2(0, 0);
                
                // 右侧区域占40%宽度
                detailRect.anchorMin = new Vector2(0.6f, 0);
                detailRect.anchorMax = new Vector2(1, 1);
                
                detailRect.offsetMin = new Vector2(5, 10);
                detailRect.offsetMax = new Vector2(-10, -70);
            }
        }

        /// <summary>
        /// 将GridLayoutGroup转换为VerticalLayoutGroup（单列表模式）
        /// </summary>
        private void ConvertToVerticalLayout()
        {
            if (mSlotContainer == null) return;

            // 移除现有的GridLayoutGroup，使用DestroyImmediate确保同帧内不会冲突
            var existingGrid = mSlotContainer.GetComponent<GridLayoutGroup>();
            if (existingGrid != null)
            {
                DestroyImmediate(existingGrid);
                mGridLayout = null;
            }

            // 添加VerticalLayoutGroup
            var verticalLayout = mSlotContainer.GetComponent<VerticalLayoutGroup>();
            if (verticalLayout == null)
            {
                verticalLayout = mSlotContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            // 配置垂直布局
            verticalLayout.childAlignment = TextAnchor.UpperCenter;
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = false;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.spacing = 5f;
            verticalLayout.padding = new RectOffset(5, 5, 5, 5);

            Debug.Log("[ShopPanel] 已转换为垂直列表布局");
        }

        /// <summary>
        /// 初始化详情面板（右侧固定显示模式）
        /// </summary>
        private void InitializeDetailPanel()
        {
            if (mDetailPanel != null)
            {
                // 设置右侧详情面板的初始状态
                SetupDetailPanelAsRightSide();
                // 初始显示“请选择商品”提示
                ShowSelectItemHint();
            }
            SetActionButtonEnabled(false);
        }

        /// <summary>
        /// 设置详情面板为右侧固定显示样式
        /// </summary>
        private void SetupDetailPanelAsRightSide()
        {
            if (mDetailPanel == null) return;

            // 确保详情面板始终显示（作为右侧固定区域）
            mDetailPanel.SetActive(true);

            // 确保有背景
            EnsureDetailPanelBackground();

            Debug.Log("[ShopPanel] 详情面板已设置为右侧固定显示模式");
        }

        /// <summary>
        /// 显示“请选择商品”的提示
        /// </summary>
        private void ShowSelectItemHint()
        {
            // 隐藏详细信息，显示提示
            if (mDetailIcon != null) mDetailIcon.gameObject.SetActive(false);
            if (mDetailNameText != null) mDetailNameText.text = "请选择商品";
            if (mDetailDescText != null) mDetailDescText.text = "点击左侧列表中的商品\n查看详细信息";
            if (mDetailPriceText != null) mDetailPriceText.text = "";
            if (mDetailOwnedText != null) mDetailOwnedText.text = "";
        }

        /// <summary>
        /// 确保详情面板有背景
        /// </summary>
        private void EnsureDetailPanelBackground()
        {
            if (mDetailPanel == null) return;

            // 检查是否已有背景图片
            var bgImage = mDetailPanel.GetComponent<Image>();
            if (bgImage == null)
            {
                bgImage = mDetailPanel.AddComponent<Image>();
                bgImage.color = new Color(0.15f, 0.22f, 0.18f, 0.7f); // 深绿色背景
            }
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
            
            // 确保VerticalLayoutGroup存在
            EnsureVerticalLayout();

            if (mCurrentMode == ShopMode.Buy)
            {
                RefreshBuyList();
            }
            else
            {
                RefreshSellList();
            }
            
            // 使用协程延迟刷新布局，确保布局组件已生效
            StartCoroutine(DelayedForceRebuildLayout());
        }
        
        /// <summary>
        /// 确保垂直布局组件存在
        /// </summary>
        private void EnsureVerticalLayout()
        {
            if (mSlotContainer == null) return;
            
            // 移除GridLayoutGroup
            var existingGrid = mSlotContainer.GetComponent<GridLayoutGroup>();
            if (existingGrid != null)
            {
                DestroyImmediate(existingGrid);
                mGridLayout = null;
            }
            
            // 确保VerticalLayoutGroup存在
            var verticalLayout = mSlotContainer.GetComponent<VerticalLayoutGroup>();
            if (verticalLayout == null)
            {
                verticalLayout = mSlotContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            
            // 配置垂直布局
            verticalLayout.childAlignment = TextAnchor.UpperLeft;
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = false; // 不控制高度，让格子保持自己设置的高度
            verticalLayout.childScaleWidth = false;
            verticalLayout.childScaleHeight = false;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.spacing = 5f;
            verticalLayout.padding = new RectOffset(5, 5, 5, 5);
            
            // 确保ContentSizeFitter存在且配置正确
            var contentSizeFitter = mSlotContainer.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null)
            {
                contentSizeFitter = mSlotContainer.gameObject.AddComponent<ContentSizeFitter>();
            }
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        
        /// <summary>
        /// 延迟强制重建布局（等待帧末和下一帧）
        /// </summary>
        private IEnumerator DelayedForceRebuildLayout()
        {
            // 等待帧末，让所有Instantiate和布局组件生效
            yield return new WaitForEndOfFrame();
            
            // 等待下一帧开始
            yield return null;
            
            ForceRebuildLayout();
            
            // 再等一帧后再次刷新（确保ContentSizeFitter已计算）
            yield return null;
            
            ForceRebuildLayout();
        }
        
        /// <summary>
        /// 强制重建布局
        /// </summary>
        private void ForceRebuildLayout()
        {
            if (mSlotContainer == null) return;
            
            var contentRect = mSlotContainer.GetComponent<RectTransform>();
            
            // 首先强制设置每个格子的高度和锚点
            foreach (var slot in mSlots)
            {
                var slotRect = slot.GetComponent<RectTransform>();
                var layoutElem = slot.GetComponent<LayoutElement>();
                
                if (slotRect != null)
                {
                    // 确保锚点正确设置（水平拉伸）
                    slotRect.anchorMin = new Vector2(0, 1);
                    slotRect.anchorMax = new Vector2(1, 1);
                    slotRect.pivot = new Vector2(0.5f, 1);
                    // 强制设置sizeDelta（宽度0因为会水平拉伸，高度60）
                    slotRect.sizeDelta = new Vector2(0, 60f);
                }
                
                if (layoutElem != null)
                {
                    layoutElem.minHeight = 60f;
                    layoutElem.preferredHeight = 60f;
                    layoutElem.flexibleHeight = 0f;
                    layoutElem.ignoreLayout = false;
                }
            }
            
            // 先禁用再启用ContentSizeFitter，强制重新计算
            var contentSizeFitter = mSlotContainer.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter != null)
            {
                contentSizeFitter.enabled = false;
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                contentSizeFitter.enabled = true;
            }
            
            // 禁用再启用 VerticalLayoutGroup
            var verticalLayout = mSlotContainer.GetComponent<VerticalLayoutGroup>();
            if (verticalLayout != null)
            {
                verticalLayout.enabled = false;
                verticalLayout.enabled = true;
            }
            
            // 标记布局需要重建
            if (contentRect != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(contentRect);
            }
            
            // 强制Canvas立即更新
            Canvas.ForceUpdateCanvases();
            
            // 重建Content的布局
            if (contentRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            }
            
            // 重建ScrollView的布局
            if (mScrollRect != null)
            {
                var scrollRect = mScrollRect.GetComponent<RectTransform>();
                if (scrollRect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect);
                    Debug.Log($"[ShopPanel] ScrollView尺寸: rect={scrollRect.rect}, sizeDelta={scrollRect.sizeDelta}");
                }
                
                // 重置滚动位置到顶部
                mScrollRect.verticalNormalizedPosition = 1f;
            }
            
            // 输出所有格子的高度信息
            foreach (var slot in mSlots)
            {
                var slotRect = slot.GetComponent<RectTransform>();
                var layoutElem = slot.GetComponent<LayoutElement>();
                Debug.Log($"[ShopPanel] 格子 {slot.ItemName}: rect.height={slotRect?.rect.height}, sizeDelta.y={slotRect?.sizeDelta.y}, LayoutElement.minHeight={layoutElem?.minHeight}, ignoreLayout={layoutElem?.ignoreLayout}");
            }
            
            Debug.Log($"[ShopPanel] 布局已刷新，当前商品数量: {mSlots.Count}, Content高度: {contentRect?.rect.height}, Content子物体数: {mSlotContainer.childCount}");
        }

        /// <summary>
        /// 刷新购买列表
        /// </summary>
        private void RefreshBuyList()
        {
            Debug.Log($"[ShopPanel] 刷新购买列表, ShopType={(int)mData.ShopType}");
            
            var shopItems = ShopManager.Instance.GetShopItems(mData.ShopType);
            
            Debug.Log($"[ShopPanel] 获取到 {shopItems.Count} 个商品");

            foreach (var itemData in shopItems)
            {
                Debug.Log($"[ShopPanel] 创建商品格子: {itemData.ItemName ?? "null"}, 价格:{itemData.BuyPrice}");
                CreateShopSlot(itemData);
            }
            
            if (shopItems.Count == 0)
            {
                Debug.LogWarning($"[ShopPanel] 商店类型 {mData.ShopType} 没有商品！请检查ShopConfig配置");
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
            if (mSlotPrefab == null)
            {
                Debug.LogError("[ShopPanel] mSlotPrefab 为空！");
                return;
            }
            if (mSlotContainer == null)
            {
                Debug.LogError("[ShopPanel] mSlotContainer 为空！");
                return;
            }

            var slotObj = Instantiate(mSlotPrefab, mSlotContainer);
            
            // 确保格子激活并正确设置
            slotObj.SetActive(true);
            
            // 确保RectTransform正确初始化
            var slotRect = slotObj.GetComponent<RectTransform>();
            if (slotRect != null)
            {
                slotRect.localScale = Vector3.one;
                // 设置锚点为水平拉伸，保持固定高度
                slotRect.anchorMin = new Vector2(0, 1);
                slotRect.anchorMax = new Vector2(1, 1);
                slotRect.pivot = new Vector2(0.5f, 1);
                // 强制设置高度
                slotRect.sizeDelta = new Vector2(0, 60f);
            }
            
            var slotController = slotObj.GetComponent<ShopSlotController>();

            if (slotController != null)
            {
                slotController.SetShopItem(itemData);
                slotController.OnSlotClicked += OnSlotSelected;
                mSlots.Add(slotController);
                Debug.Log($"[ShopPanel] 格子创建成功: {itemData.ItemName}, sizeDelta: {slotRect?.sizeDelta}, 父级子物体数: {mSlotContainer.childCount}");
            }
            else
            {
                Debug.LogError("[ShopPanel] ShopSlotController 组件缺失！");
            }
        }

        /// <summary>
        /// 创建背包物品格子（用于出售）
        /// </summary>
        private void CreateInventorySlot(ItemEntity item, int sellPrice)
        {
            if (mSlotPrefab == null || mSlotContainer == null) return;

            var slotObj = Instantiate(mSlotPrefab, mSlotContainer);
            
            // 确保格子激活并正确设置
            slotObj.SetActive(true);
            
            // 确保RectTransform正确初始化
            var slotRect = slotObj.GetComponent<RectTransform>();
            if (slotRect != null)
            {
                slotRect.localScale = Vector3.one;
                // 设置锚点为水平拉伸，保持固定高度
                slotRect.anchorMin = new Vector2(0, 1);
                slotRect.anchorMax = new Vector2(1, 1);
                slotRect.pivot = new Vector2(0.5f, 1);
                // 强制设置高度
                slotRect.sizeDelta = new Vector2(0, 60f);
            }
            
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

            // 显示"请选择商品"提示，而不是隐藏面板
            ShowSelectItemHint();

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

        #region 店主相关

        /// <summary>
        /// 加载店主立绘
        /// </summary>
        private void LoadShopkeeperPortrait()
        {
            if (mShopkeeperPortrait == null) return;

            var sprite = Resources.Load<Sprite>(SHOPKEEPER_ICON_PATH);
            if (sprite != null)
            {
                mShopkeeperPortrait.sprite = sprite;
                mShopkeeperPortrait.enabled = true;
                Debug.Log("[ShopPanel] 店主立绘加载成功");
            }
            else
            {
                Debug.LogWarning($"[ShopPanel] 无法加载店主立绘: {SHOPKEEPER_ICON_PATH}");
                mShopkeeperPortrait.enabled = false;
            }

            // 设置默认对话（可选）
            if (mShopkeeperDialogue != null)
            {
                mShopkeeperDialogue.text = "嗯...欢迎光临...";
            }
        }

        #endregion
    }
}
