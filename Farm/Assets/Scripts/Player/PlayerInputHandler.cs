using UnityEngine;
using FarmGame.Movement;
using FarmGame.Farm;
using FarmGame.Farm.View;
using FarmGame.UI;
using FarmGame.Core;
using FarmGame.Item;

namespace FarmGame.Player
{
    /// <summary>
    /// 玩家输入处理器
    /// 负责处理玩家的输入并转换为移动指令
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        #region 私有字段

        private Camera mMainCamera;
        private Movable mMovable;
        private bool mIsInitialized;
        private FarmTilemapView mFarmView;
        private InventoryComponent mInventory;

        /// <summary>
        /// 与农田交互的最大距离
        /// </summary>
        private const float FARM_INTERACTION_DISTANCE = 2f;

        #endregion

        #region 公共属性

        /// <summary>
        /// 是否启用输入
        /// </summary>
        public bool InputEnabled { get; set; } = true;

        /// <summary>
        /// 玩家背包组件
        /// </summary>
        public InventoryComponent Inventory
        {
            get => mInventory;
            set => mInventory = value;
        }

        #endregion

        #region 生命周期

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (mIsInitialized) return;

            // 获取主相机
            mMainCamera = Camera.main;
            if (mMainCamera == null)
            {
                Debug.LogError("[PlayerInputHandler] Main Camera not found!");
                return;
            }

            // 获取Movable组件
            mMovable = GetComponent<Movable>();
            if (mMovable == null)
            {
                Debug.LogError("[PlayerInputHandler] Movable component not found on this GameObject!");
                return;
            }

            // 初始化背包
            if (mInventory == null)
            {
                mInventory = new InventoryComponent();
            }

            mIsInitialized = true;
            Debug.Log("[PlayerInputHandler] Initialized");
        }

        private void Update()
        {
            if (!mIsInitialized) return;
            if (!InputEnabled) return;

            HandleClickInput();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置农场视图引用
        /// </summary>
        /// <param name="farmView">农场Tilemap视图</param>
        public void SetFarmView(FarmTilemapView farmView)
        {
            mFarmView = farmView;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 处理点击输入（移动或农场交互）
        /// </summary>
        private void HandleClickInput()
        {
            // 检测点击（鼠标左键或触摸）
            if (Input.GetMouseButtonDown(0))
            {
                // 检查是否点击在UI上
                if (IsPointerOverUI()) return;

                // 获取点击位置的世界坐标
                Vector2 worldPos = GetMouseWorldPosition();
                
                Debug.Log($"[PlayerInputHandler] 点击位置: {worldPos}");

                // 尝试查找农场视图
                if (mFarmView == null)
                {
                    mFarmView = FindObjectOfType<FarmTilemapView>();
                }

                // 检查是否点击在农田上
                if (mFarmView != null)
                {
                    SoilEntity soil = mFarmView.GetSoilAtWorldPos(worldPos);
                    if (soil != null)
                    {
                        Debug.Log($"[PlayerInputHandler] 点击了土地: {soil.GridPos}");
                        
                        // 检查玩家与农田的距离
                        Vector3 soilWorldPos = mFarmView.GridToWorld(soil.GridPos);
                        float distance = Vector2.Distance(
                            new Vector2(transform.position.x, transform.position.y),
                            new Vector2(soilWorldPos.x, soilWorldPos.y)
                        );

                        if (distance <= FARM_INTERACTION_DISTANCE)
                        {
                            // 距离够近，根据土地状态打开不同UI
                            if (soil.HasPlant)
                            {
                                // 有作物 → 打开作物详情气泡
                                OpenCropDetailBubble(soil, soilWorldPos);
                            }
                            else
                            {
                                // 无作物 → 打开播种菜单
                                OpenFarmContextMenu(soil, Input.mousePosition);
                            }
                            return;
                        }
                        else
                        {
                            Debug.Log($"[PlayerInputHandler] 距离太远({distance:F2})，先移动过去");
                        }
                    }
                }

                // 不是农田或距离太远，执行移动
                mMovable.MoveTo(worldPos);
            }
        }

        /// <summary>
        /// 打开作物详情气泡
        /// </summary>
        private void OpenCropDetailBubble(SoilEntity soil, Vector3 worldPos)
        {
            Debug.Log($"[PlayerInputHandler] 打开作物详情气泡，土地位置: {soil.GridPos}");
            
            var bubbleData = new CropDetailBubbleData
            {
                Plant = soil.Plant,
                Soil = soil,
                WorldPosition = worldPos,
                Inventory = mInventory
            };

            UIManager.Instance.OpenPanel<CropDetailBubblePanel>(
                "UI/CropBubble/CropDetailBubblePanel",
                bubbleData
            );
        }

        /// <summary>
        /// 打开农场菜单
        /// </summary>
        private void OpenFarmContextMenu(SoilEntity soil, Vector2 screenPos)
        {
            Debug.Log($"[PlayerInputHandler] 打开农场菜单，土地位置: {soil.GridPos}");
            
            var menuData = new FarmContextMenuData
            {
                Soil = soil,
                Inventory = mInventory,
                ScreenPosition = screenPos
            };

            UIManager.Instance.OpenPanel<FarmContextMenuPanel>(
                "UI/Farm/FarmContextMenuPanel",
                menuData
            );
        }

        /// <summary>
        /// 获取鼠标位置的世界坐标（2D）
        /// </summary>
        private Vector2 GetMouseWorldPosition()
        {
            Vector3 mouseScreenPosition = Input.mousePosition;
            mouseScreenPosition.z = Mathf.Abs(mMainCamera.transform.position.z);
            Vector3 worldPosition = mMainCamera.ScreenToWorldPoint(mouseScreenPosition);
            return new Vector2(worldPosition.x, worldPosition.y);
        }

        /// <summary>
        /// 检查指针是否在UI元素上
        /// </summary>
        private bool IsPointerOverUI()
        {
            return UnityEngine.EventSystems.EventSystem.current != null &&
                   UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }

        #endregion
    }
}
