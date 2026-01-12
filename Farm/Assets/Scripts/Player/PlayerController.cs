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
        private Animator mAnimator;
        private Transform mVisualRoot;

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
            mAnimator = GetComponentInChildren<Animator>();
            
            // 查找视觉根节点（通常是第一个不包含Shadow的子节点，或者是Animator所在的节点）
            if (mAnimator != null && mAnimator.transform != transform)
            {
                // 如果Animator在子节点上，那它就是视觉根节点
                mVisualRoot = mAnimator.transform;
            }
            else
            {
                // 否则遍历查找第一个非阴影的子节点
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);
                    if (!child.name.Contains("Shadow") && !child.name.Contains("shadow"))
                    {
                        mVisualRoot = child;
                        break;
                    }
                }
            }

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
            if (mAnimator != null)
            {
                mAnimator.SetBool("isRun", true);
            }
        }

        /// <summary>
        /// 停止移动时调用（预留给动画系统）
        /// </summary>
        protected virtual void HandleMoveStop()
        {
            if (mAnimator != null)
            {
                mAnimator.SetBool("isRun", false);
            }

            // 同步最终位置到数据
            mData.Position = transform.position;
        }

        /// <summary>
        /// 移动方向改变时调用（预留给动画系统）
        /// </summary>
        /// <param name="direction">新的移动方向</param>
        protected virtual void HandleDirectionChanged(Vector2 direction)
        {
            // 通过缩放翻转角色朝向
            if (mVisualRoot != null && direction.x != 0)
            {
                Vector3 scale = mVisualRoot.localScale;
                // 确保使用绝对值作为基准，避免多次乘以-1导致的错误
                float absScaleX = Mathf.Abs(scale.x); 
                scale.x = direction.x < 0 ? -absScaleX : absScaleX;
                mVisualRoot.localScale = scale;
            }

            // 同步朝向到数据
            mData.FacingDirection = new Vector3(direction.x, direction.y, 0);
        }

        #endregion
    }
}
