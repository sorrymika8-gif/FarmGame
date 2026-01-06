using UnityEngine;

namespace FarmGame.Movement
{
    /// <summary>
    /// 相机跟随组件
    /// 挂载在Main Camera上，实现平滑跟随目标
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        #region 序列化字段

        [Header("跟随设置")]
        [SerializeField]
        [Tooltip("跟随目标")]
        private Transform mTarget;

        [SerializeField]
        [Tooltip("相机偏移量")]
        private Vector3 mOffset = new Vector3(0, 0, -10);

        [SerializeField]
        [Tooltip("平滑跟随速度")]
        [Range(0.1f, 20f)]
        private float mSmoothSpeed = 5f;

        #endregion

        #region 私有字段

        private bool mIsFollowing = true;

        #endregion

        #region 公共属性

        /// <summary>
        /// 跟随目标
        /// </summary>
        public Transform Target
        {
            get => mTarget;
            set => mTarget = value;
        }

        /// <summary>
        /// 相机偏移量
        /// </summary>
        public Vector3 Offset
        {
            get => mOffset;
            set => mOffset = value;
        }

        /// <summary>
        /// 平滑跟随速度
        /// </summary>
        public float SmoothSpeed
        {
            get => mSmoothSpeed;
            set => mSmoothSpeed = Mathf.Max(0.1f, value);
        }

        /// <summary>
        /// 是否启用跟随
        /// </summary>
        public bool IsFollowing
        {
            get => mIsFollowing;
            set => mIsFollowing = value;
        }

        #endregion

        #region 生命周期

        private void LateUpdate()
        {
            if (!mIsFollowing) return;
            if (mTarget == null) return;

            FollowTarget();
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 设置跟随目标
        /// </summary>
        /// <param name="target">目标Transform</param>
        public void SetTarget(Transform target)
        {
            mTarget = target;

            // 立即跳转到目标位置
            if (mTarget != null)
            {
                transform.position = mTarget.position + mOffset;
            }
        }

        /// <summary>
        /// 设置跟随目标（带偏移量）
        /// </summary>
        /// <param name="target">目标Transform</param>
        /// <param name="offset">相机偏移量</param>
        public void SetTarget(Transform target, Vector3 offset)
        {
            mOffset = offset;
            SetTarget(target);
        }

        /// <summary>
        /// 立即跳转到目标位置
        /// </summary>
        public void SnapToTarget()
        {
            if (mTarget == null) return;

            transform.position = mTarget.position + mOffset;
        }

        /// <summary>
        /// 暂停跟随
        /// </summary>
        public void PauseFollow()
        {
            mIsFollowing = false;
        }

        /// <summary>
        /// 恢复跟随
        /// </summary>
        public void ResumeFollow()
        {
            mIsFollowing = true;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 执行跟随逻辑
        /// </summary>
        private void FollowTarget()
        {
            // 计算目标位置
            Vector3 targetPosition = mTarget.position + mOffset;

            // 使用Lerp实现平滑移动
            Vector3 smoothedPosition = Vector3.Lerp(
                transform.position,
                targetPosition,
                mSmoothSpeed * Time.deltaTime
            );

            // 更新相机位置
            transform.position = smoothedPosition;
        }

        #endregion
    }
}
