using UnityEngine;
using FarmGame.Movement;

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

        #endregion

        #region 公共属性

        /// <summary>
        /// 是否启用输入
        /// </summary>
        public bool InputEnabled { get; set; } = true;

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

        #region 私有方法

        /// <summary>
        /// 处理点击输入
        /// </summary>
        private void HandleClickInput()
        {
            // 检测鼠标左键点击
            if (Input.GetMouseButtonDown(0))
            {
                // 检查是否点击在UI上
                if (IsPointerOverUI()) return;

                // 获取点击位置的世界坐标
                Vector2 targetPosition = GetMouseWorldPosition();

                // 移动到目标位置
                mMovable.MoveTo(targetPosition);
            }
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
