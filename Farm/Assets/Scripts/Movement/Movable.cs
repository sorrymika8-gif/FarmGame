using UnityEngine;
using System;

namespace FarmGame.Movement
{
    /// <summary>
    /// 可移动组件
    /// 挂载在任何需要移动能力的GameObject上（玩家、怪物、NPC等）
    /// </summary>
    public class Movable : MonoBehaviour
    {
        #region 私有字段

        private float mMoveSpeed = 5f;
        private float mStoppingDistance = 0.05f;
        private bool mIsMoving;
        private Vector2 mTargetPosition;
        private Vector2 mMoveDirection;
        private Vector2 mFacingDirection;

        #endregion

        #region 公共属性

        /// <summary>
        /// 移动速度
        /// </summary>
        public float MoveSpeed
        {
            get => mMoveSpeed;
            set => mMoveSpeed = Mathf.Max(0f, value);
        }

        /// <summary>
        /// 是否正在移动
        /// </summary>
        public bool IsMoving => mIsMoving;

        /// <summary>
        /// 当前移动方向
        /// </summary>
        public Vector2 MoveDirection => mMoveDirection;

        /// <summary>
        /// 当前朝向
        /// </summary>
        public Vector2 FacingDirection => mFacingDirection;

        /// <summary>
        /// 目标位置
        /// </summary>
        public Vector2 TargetPosition => mTargetPosition;

        #endregion

        #region 事件（供动画系统订阅）

        /// <summary>
        /// 开始移动事件
        /// </summary>
        public event Action OnMoveStart;

        /// <summary>
        /// 停止移动事件
        /// </summary>
        public event Action OnMoveStop;

        /// <summary>
        /// 方向改变事件
        /// </summary>
        public event Action<Vector2> OnDirectionChanged;

        #endregion

        #region 生命周期

        private void Awake()
        {
            mFacingDirection = Vector2.down; // 默认朝下
        }

        private void Update()
        {
            if (mIsMoving)
            {
                UpdateMovement();
            }
        }

        private void OnEnable()
        {
            // 注册到MovementManager
            MovementManager.Instance?.Register(this);
        }

        private void OnDisable()
        {
            // 从MovementManager注销
            MovementManager.Instance?.Unregister(this);
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 移动到目标位置
        /// </summary>
        /// <param name="targetPosition">目标位置</param>
        public void MoveTo(Vector2 targetPosition)
        {
            mTargetPosition = targetPosition;

            // 计算移动方向
            Vector2 currentPosition = transform.position;
            Vector2 newDirection = (mTargetPosition - currentPosition).normalized;

            // 更新朝向
            if (newDirection != Vector2.zero)
            {
                mFacingDirection = newDirection;
            }

            // 检查方向是否改变
            bool directionChanged = mMoveDirection != newDirection;
            mMoveDirection = newDirection;

            // 开始移动
            if (!mIsMoving)
            {
                mIsMoving = true;
                OnMoveStart?.Invoke();
            }

            if (directionChanged)
            {
                OnDirectionChanged?.Invoke(mMoveDirection);
            }
        }

        /// <summary>
        /// 停止移动
        /// </summary>
        public void StopMovement()
        {
            if (mIsMoving)
            {
                mIsMoving = false;
                mMoveDirection = Vector2.zero;
                OnMoveStop?.Invoke();
            }
        }

        /// <summary>
        /// 立即传送到指定位置
        /// </summary>
        /// <param name="position">目标位置</param>
        public void TeleportTo(Vector2 position)
        {
            StopMovement();
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }

        /// <summary>
        /// 设置朝向（不移动）
        /// </summary>
        /// <param name="direction">朝向方向</param>
        public void SetFacingDirection(Vector2 direction)
        {
            if (direction != Vector2.zero)
            {
                mFacingDirection = direction.normalized;
                OnDirectionChanged?.Invoke(mFacingDirection);
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 更新移动逻辑
        /// </summary>
        private void UpdateMovement()
        {
            Vector2 currentPosition = transform.position;
            float distance = Vector2.Distance(currentPosition, mTargetPosition);

            // 检查是否到达目标点
            if (distance < mStoppingDistance)
            {
                transform.position = new Vector3(mTargetPosition.x, mTargetPosition.y, transform.position.z);
                StopMovement();
                return;
            }

            // 平滑移动
            Vector2 newPosition = Vector2.MoveTowards(
                currentPosition,
                mTargetPosition,
                mMoveSpeed * Time.deltaTime
            );

            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
        }

        #endregion
    }
}
