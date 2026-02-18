using UnityEngine;
using UnityEngine.UI;
using QFramework;
using FarmGame.Item;
using FarmGame.GameConfig;
using System.Collections.Generic;
using System.Linq;

namespace FarmGame.UI
{
    /// <summary>
    /// 背包UI面板数据
    /// </summary>
    public class BackpackPanelData : UIPanelData
    {
        // 可以添加背包特定的数据
        public InventoryComponent Inventory { get; set; }
    }

    /// <summary>
    /// 背包UI面板
    /// 显示玩家物品的网格界面
    /// </summary>
    public partial class BackpackPanel : UIPanel
    {
        #region UI组件 - 主界面

        [Header("主界面组件")]
        [SerializeField]
        private Button mBackgroundMask; // 背景遮罩（点击关闭背包）

        [SerializeField]
        private Transform mSlotContainer; // 物品格子容器

        [SerializeField]
        private GameObject mSlotPrefab; // 物品格子Prefab

        [SerializeField]
        private Text mTitleText; // 标题文本

        [SerializeField]
        private Button mCloseButton; // 关闭按钮

        #endregion

        #region UI组件 - 详情面板

        [Header("详情面板组件")]
        [SerializeField]
        private GameObject mDetailPanel; // 详情面板容器

        [SerializeField]
        private Image mDetailIcon; // 详情图标

        [SerializeField]
        private Text mDetailNameText; // 详情名称

        [SerializeField]
        private Text mDetailDescText; // 详情描述

        [SerializeField]
        private Text mDetailCountText; // 详情数量

        [SerializeField]
        private Text mDetailTypeText; // 详情类型

        #endregion

        #region UI组件 - 操作按钮

        [Header("操作按钮")]
        [SerializeField]
        private Button mUseButton; // 使用按钮

        [SerializeField]
        private Text mUseButtonText; // 使用按钮文本

        [SerializeField]
        private Button mDiscardButton; // 丢弃按钮

        #endregion

        #region 私有字段

        private BackpackPanelData mData;
        private List<InventorySlotController> mSlots = new List<InventorySlotController>();
        private InventorySlotController mSelectedSlot; // 当前选中的格子
        private const int GRID_WIDTH = 8; // 网格宽度
        private const int GRID_HEIGHT = 6; // 网格高度
        private const int TOTAL_SLOTS = GRID_WIDTH * GRID_HEIGHT; // 总格子数

        #endregion

        #region UIPanel生命周期

        /// <summary>
        /// 初始化背包面板
        /// </summary>
        /// <param name="uiData">面板数据</param>
        protected override void OnInit(IUIData uiData = null)
        {
            mData = uiData as BackpackPanelData ?? new BackpackPanelData();
            
            // 如果没有传入背包组件，尝试获取默认的
            if (mData.Inventory == null)
            {
                // 这里可以添加获取默认背包组件的逻辑
                // 例如：从Player组件获取
            }

            // 初始化UI组件
            InitializeUIComponents();

            // 初始化详情面板
            InitializeDetailPanel();

            // 生成物品格子
            GenerateInventorySlots();

            // 刷新背包显示
            RefreshInventory();
        }

        /// <summary>
        /// 打开背包面板
        /// </summary>
        /// <param name="uiData">面板数据</param>
        protected override void OnOpen(IUIData uiData = null)
        {
            // 打开时刷新背包内容
            RefreshInventory();
        }

        /// <summary>
        /// 关闭背包面板
        /// </summary>
        protected override void OnClose()
        {
            // 清除选中状态
            ClearSelection();

            // 清理资源
            ClearSlots();
        }

        /// <summary>
        /// 显示背包面板
        /// </summary>
        protected override void OnShow()
        {
            base.OnShow();
            // 显示时的额外逻辑
        }

        /// <summary>
        /// 隐藏背包面板
        /// </summary>
        protected override void OnHide()
        {
            base.OnHide();
            // 隐藏时的额外逻辑
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 初始化UI组件
        /// </summary>
        private void InitializeUIComponents()
        {
            // 设置标题
            if (mTitleText != null)
            {
                mTitleText.text = "背包";
            }

            // 绑定背景遮罩点击事件（点击背包外部区域关闭）
            if (mBackgroundMask != null)
            {
                mBackgroundMask.onClick.RemoveAllListeners();
                mBackgroundMask.onClick.AddListener(() =>
                {
                    CloseSelf();
                });
            }

            // 绑定关闭按钮事件
            if (mCloseButton != null)
            {
                mCloseButton.onClick.RemoveAllListeners();
                mCloseButton.onClick.AddListener(() =>
                {
                    CloseSelf();
                });
            }

            // 绑定使用按钮事件
            if (mUseButton != null)
            {
                mUseButton.onClick.RemoveAllListeners();
                mUseButton.onClick.AddListener(OnUseButtonClick);
            }

            // 绑定丢弃按钮事件
            if (mDiscardButton != null)
            {
                mDiscardButton.onClick.RemoveAllListeners();
                mDiscardButton.onClick.AddListener(OnDiscardButtonClick);
            }
        }

        /// <summary>
        /// 初始化详情面板
        /// </summary>
        private void InitializeDetailPanel()
        {
            // 初始隐藏详情面板
            if (mDetailPanel != null)
            {
                mDetailPanel.SetActive(false);
            }

            // 初始禁用操作按钮
            SetOperationButtonsEnabled(false);
        }

        /// <summary>
        /// 生成物品格子
        /// </summary>
        private void GenerateInventorySlots()
        {
            if (mSlotContainer == null || mSlotPrefab == null)
            {
                Debug.LogError("[BackpackPanel] Slot container or prefab is not assigned!");
                return;
            }

            // 清空现有格子
            ClearSlots();

            // 生成网格格子
            for (int i = 0; i < TOTAL_SLOTS; i++)
            {
                var slotObj = Instantiate(mSlotPrefab, mSlotContainer);
                var slotController = slotObj.GetComponent<InventorySlotController>();
                
                if (slotController != null)
                {
                    mSlots.Add(slotController);
                    slotController.Initialize(i); // 初始化格子索引

                    // 注册格子点击事件
                    slotController.OnSlotClicked += OnSlotSelected;
                }
                else
                {
                    Debug.LogWarning($"[BackpackPanel] Slot prefab at index {i} doesn't have InventorySlotController component!");
                }
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
                    // 取消事件订阅
                    slot.OnSlotClicked -= OnSlotSelected;

                    if (slot.gameObject != null)
                    {
                        Destroy(slot.gameObject);
                    }
                }
            }
            mSlots.Clear();
        }

        /// <summary>
        /// 刷新背包显示
        /// </summary>
        private void RefreshInventory()
        {
            if (mData.Inventory == null)
            {
                Debug.LogWarning("[BackpackPanel] No inventory data available to refresh!");
                return;
            }

            // 清空所有格子
            ClearAllSlots();

            // 获取所有物品
            var items = mData.Inventory.GetAllItems();
            int slotIndex = 0;

            // 更新格子显示
            foreach (var item in items)
            {
                if (item == null || item.Count <= 0)
                    continue;

                // 确保不超过格子数量
                if (slotIndex >= mSlots.Count)
                {
                    Debug.LogWarning($"[BackpackPanel] Not enough slots for all items! Items: {items.Count()}, Slots: {mSlots.Count}");
                    break;
                }

                // 更新格子
                var slot = mSlots[slotIndex];
                if (slot != null)
                {
                    slot.UpdateSlot(item);
                }

                slotIndex++;
            }

            Debug.Log($"[BackpackPanel] Refreshed inventory with {slotIndex} items");

            // 刷新后更新选中格子的显示
            if (mSelectedSlot != null)
            {
                var item = mSelectedSlot.GetCurrentItem();
                if (item == null || item.Count <= 0)
                {
                    // 选中的物品已经没了，清除选中状态
                    ClearSelection();
                }
                else
                {
                    // 更新详情面板显示
                    UpdateDetailPanel(item);
                }
            }
        }

        /// <summary>
        /// 清空所有格子显示
        /// </summary>
        private void ClearAllSlots()
        {
            foreach (var slot in mSlots)
            {
                if (slot != null)
                {
                    slot.ClearSlot();
                }
            }
        }

        /// <summary>
        /// 格子被选中的回调
        /// </summary>
        /// <param name="slot">被选中的格子</param>
        private void OnSlotSelected(InventorySlotController slot)
        {
            if (slot == null) return;

            // 取消之前的选中状态
            if (mSelectedSlot != null && mSelectedSlot != slot)
            {
                mSelectedSlot.SetSelected(false);
            }

            // 设置新的选中状态
            mSelectedSlot = slot;
            mSelectedSlot.SetSelected(true);

            // 获取当前物品
            var item = slot.GetCurrentItem();

            if (item != null && item.Count > 0)
            {
                // 显示详情面板
                UpdateDetailPanel(item);
                SetOperationButtonsEnabled(true);

                Debug.Log($"[BackpackPanel] Selected item: {item.Config?.name ?? "Unknown"} x{item.Count}");
            }
            else
            {
                // 空格子，隐藏详情
                ClearDetailPanel();
                SetOperationButtonsEnabled(false);

                Debug.Log($"[BackpackPanel] Selected empty slot at index {slot.GetSlotIndex()}");
            }
        }

        /// <summary>
        /// 更新详情面板显示
        /// </summary>
        /// <param name="item">物品实体</param>
        private void UpdateDetailPanel(ItemEntity item)
        {
            if (item == null || mDetailPanel == null) return;

            var config = item.Config;
            if (config == null) return;

            // 显示详情面板
            mDetailPanel.SetActive(true);

            // 更新图标
            if (mDetailIcon != null)
            {
                if (!string.IsNullOrEmpty(config.iconPath))
                {
                    var sprite = Resources.Load<Sprite>(config.iconPath);
                    if (sprite != null)
                    {
                        mDetailIcon.sprite = sprite;
                        mDetailIcon.color = Color.white;
                    }
                    else
                    {
                        SetDetailPlaceholderIcon(config);
                    }
                }
                else
                {
                    SetDetailPlaceholderIcon(config);
                }
            }

            // 更新名称
            if (mDetailNameText != null)
            {
                mDetailNameText.text = config.name;
            }

            // 更新描述
            if (mDetailDescText != null)
            {
                mDetailDescText.text = string.IsNullOrEmpty(config.description) 
                    ? "暂无描述" 
                    : config.description;
            }

            // 更新数量
            if (mDetailCountText != null)
            {
                mDetailCountText.text = $"数量: {item.Count}";
            }

            // 更新类型
            if (mDetailTypeText != null)
            {
                string typeStr = config.ItemType switch
                {
                    ItemType.Seed => "种子",
                    ItemType.Product => "农产品",
                    ItemType.Tool => "工具",
                    _ => "未知"
                };
                mDetailTypeText.text = $"类型: {typeStr}";
            }

            // 更新使用按钮文本
            if (mUseButtonText != null)
            {
                string useText = config.ItemType switch
                {
                    ItemType.Seed => "种植",
                    ItemType.Product => "使用",
                    ItemType.Tool => "装备",
                    _ => "使用"
                };
                mUseButtonText.text = useText;
            }
        }

        /// <summary>
        /// 设置详情面板的占位图标
        /// </summary>
        private void SetDetailPlaceholderIcon(ItemConfig config)
        {
            if (mDetailIcon == null) return;

            Color typeColor = config.ItemType switch
            {
                ItemType.Seed => new Color(0.4f, 0.8f, 0.4f, 1f),
                ItemType.Product => new Color(1f, 0.6f, 0.2f, 1f),
                ItemType.Tool => new Color(0.5f, 0.5f, 0.8f, 1f),
                _ => new Color(0.7f, 0.7f, 0.7f, 1f)
            };

            mDetailIcon.sprite = null;
            mDetailIcon.color = typeColor;
        }

        /// <summary>
        /// 清空详情面板
        /// </summary>
        private void ClearDetailPanel()
        {
            if (mDetailPanel != null)
            {
                mDetailPanel.SetActive(false);
            }
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

            ClearDetailPanel();
            SetOperationButtonsEnabled(false);
        }

        /// <summary>
        /// 设置操作按钮启用状态
        /// </summary>
        /// <param name="enabled">是否启用</param>
        private void SetOperationButtonsEnabled(bool enabled)
        {
            if (mUseButton != null)
            {
                mUseButton.interactable = enabled;
            }

            if (mDiscardButton != null)
            {
                mDiscardButton.interactable = enabled;
            }
        }

        /// <summary>
        /// 使用按钮点击处理
        /// </summary>
        private void OnUseButtonClick()
        {
            if (mSelectedSlot == null || mSelectedSlot.IsEmpty())
            {
                Debug.LogWarning("[BackpackPanel] No item selected to use!");
                return;
            }

            var item = mSelectedSlot.GetCurrentItem();
            if (item == null) return;

            var config = item.Config;
            if (config == null) return;

            Debug.Log($"[BackpackPanel] Using item: {config.name}");

            // 根据物品类型执行不同操作
            switch (config.ItemType)
            {
                case ItemType.Seed:
                    // 种子类型 - 可以触发种植模式
                    OnUseSeed(item, config);
                    break;

                case ItemType.Product:
                    // 农产品类型 - 可以食用或出售
                    OnUseProduct(item, config);
                    break;

                case ItemType.Tool:
                    // 工具类型 - 装备工具
                    OnUseTool(item, config);
                    break;

                default:
                    Debug.Log($"[BackpackPanel] Unknown item type: {config.type}");
                    break;
            }
        }

        /// <summary>
        /// 丢弃按钮点击处理
        /// </summary>
        private void OnDiscardButtonClick()
        {
            if (mSelectedSlot == null || mSelectedSlot.IsEmpty())
            {
                Debug.LogWarning("[BackpackPanel] No item selected to discard!");
                return;
            }

            var item = mSelectedSlot.GetCurrentItem();
            if (item == null || mData.Inventory == null) return;

            Debug.Log($"[BackpackPanel] Discarding item: {item.Config?.name ?? "Unknown"}");

            // 移除一个物品
            bool success = mData.Inventory.RemoveItem(item.ConfigId, 1);

            if (success)
            {
                // 刷新背包显示
                RefreshInventory();
            }
        }

        /// <summary>
        /// 使用种子
        /// </summary>
        private void OnUseSeed(ItemEntity item, ItemConfig config)
        {
            Debug.Log($"[BackpackPanel] Planting seed: {config.name}, PlantConfigId: {config.function_args}");

            // TODO: 触发种植模式
            // 可以发送事件或调用GameManager进入种植模式
            // 示例: GameManager.Instance.EnterPlantMode(config.function_args);

            // 关闭背包界面
            CloseSelf();
        }

        /// <summary>
        /// 使用农产品
        /// </summary>
        private void OnUseProduct(ItemEntity item, ItemConfig config)
        {
            Debug.Log($"[BackpackPanel] Using product: {config.name}");

            // TODO: 实现农产品使用逻辑
            // 例如恢复体力、出售等

            // 消耗一个物品
            if (mData.Inventory != null)
            {
                mData.Inventory.RemoveItem(item.ConfigId, 1);
                RefreshInventory();
            }
        }

        /// <summary>
        /// 使用工具
        /// </summary>
        private void OnUseTool(ItemEntity item, ItemConfig config)
        {
            Debug.Log($"[BackpackPanel] Equipping tool: {config.name}");

            // TODO: 实现工具装备逻辑
            // 例如设置当前装备的工具

            // 关闭背包界面
            CloseSelf();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置背包数据
        /// </summary>
        /// <param name="inventory">背包组件</param>
        public void SetInventory(InventoryComponent inventory)
        {
            mData.Inventory = inventory;
            ClearSelection();
            RefreshInventory();
        }

        /// <summary>
        /// 获取当前选中的物品
        /// </summary>
        /// <returns>当前选中的物品实体，如果没有选中则返回null</returns>
        public ItemEntity GetSelectedItem()
        {
            return mSelectedSlot?.GetCurrentItem();
        }

        /// <summary>
        /// 关闭背包面板
        /// </summary>
        public void CloseSelf()
        {
            UIKit.ClosePanel(this);
        }

        #endregion
    }
}