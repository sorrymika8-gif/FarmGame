using UnityEngine;
using FarmGame.Movement;

namespace FarmGame.Player
{
    /// <summary>
    /// 玩家控制器
    /// 挂载在玩家GameObject上，作为玩家实体的核心组件
    /// 移动功能由Movable组件提供
    /// </summary>
    [RequireComponent(typeof(Movable))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerController : MonoBehaviour
    {
        #region 私有字段

        private PlayerData mData;
        private bool mIsInitialized;
        private Movable mMovable;

        #endregion

        #region 公共属性

        /// <summary>
        /// 玩家数据
        /// </summary>
        public PlayerData Data => mData;

        /// <summary>
        /// 移动组件
        /// </summary>
        public Movable Movable => mMovable;

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化玩家控制器
        /// </summary>
        public void Initialize()
        {
            if (mIsInitialized) return;

            // 获取Movable组件
            mMovable = GetComponent<Movable>();

            // 创建玩家数据
            mData = new PlayerData();

            // 同步移动速度
            if (mMovable != null)
            {
                mMovable.MoveSpeed = mData.MoveSpeed;
            }

            // 同步初始位置
            transform.position = mData.Position;

            // 订阅移动事件（预留给动画系统）
            SubscribeMovementEvents();

            mIsInitialized = true;
            Debug.Log("[PlayerController] Initialized");
        }

        private void OnDestroy()
        {
            UnsubscribeMovementEvents();
        }

        #endregion

        #region 移动事件处理（预留给动画系统）

        private void SubscribeMovementEvents()
        {
            if (mMovable == null) return;

            mMovable.OnMoveStart += HandleMoveStart;
            mMovable.OnMoveStop += HandleMoveStop;
            mMovable.OnDirectionChanged += HandleDirectionChanged;
        }

        private void UnsubscribeMovementEvents()
        {
            if (mMovable == null) return;

            mMovable.OnMoveStart -= HandleMoveStart;
            mMovable.OnMoveStop -= HandleMoveStop;
            mMovable.OnDirectionChanged -= HandleDirectionChanged;
        }

        /// <summary>
        /// 开始移动时调用（预留给动画系统）
        /// </summary>
        protected virtual void HandleMoveStart()
        {
            // TODO: 触发行走动画
            // 子类可重写此方法来处理动画逻辑
        }

        /// <summary>
        /// 停止移动时调用（预留给动画系统）
        /// </summary>
        protected virtual void HandleMoveStop()
        {
            // TODO: 切换到待机动画
            // 子类可重写此方法来处理动画逻辑

            // 同步最终位置到数据
            mData.Position = transform.position;
        }

        /// <summary>
        /// 移动方向改变时调用（预留给动画系统）
        /// </summary>
        /// <param name="direction">新的移动方向</param>
        protected virtual void HandleDirectionChanged(Vector2 direction)
        {
            // TODO: 根据方向切换动画
            // 子类可重写此方法来处理动画逻辑

            // 同步朝向到数据
            mData.FacingDirection = new Vector3(direction.x, direction.y, 0);
        }

        #endregion
    }
}
