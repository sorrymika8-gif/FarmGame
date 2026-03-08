using UnityEngine;

namespace FarmGame.Core
{
    /// <summary>
    /// 基于Y坐标的精灵排序组件
    /// Y坐标越小（屏幕越靠下）的物体渲染在前面
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteSortByY : MonoBehaviour
    {
        #region 私有字段

        private string mSortingLayerName = SortingLayerConfig.Characters;
        private int mBaseSortingOrder = 100;
        private int mPrecision = 100;
        private UpdateMode mUpdateMode = UpdateMode.EveryFrame;
        private float mYOffset = 0f;
        private SpriteRenderer mSpriteRenderer;
        private float mLastY;

        #endregion

        #region 公共属性

        /// <summary>
        /// 排序层级名称
        /// </summary>
        public string SortingLayerName
        {
            get => mSortingLayerName;
            set
            {
                mSortingLayerName = value;
                UpdateSortingOrder();
            }
        }

        /// <summary>
        /// 基础排序值
        /// </summary>
        public int BaseSortingOrder
        {
            get => mBaseSortingOrder;
            set
            {
                mBaseSortingOrder = value;
                UpdateSortingOrder();
            }
        }

        /// <summary>
        /// 精度
        /// </summary>
        public int Precision
        {
            get => mPrecision;
            set
            {
                mPrecision = value;
                UpdateSortingOrder();
            }
        }

        #endregion

        #region 枚举

        public enum UpdateMode
        {
            /// <summary>每帧更新（适合移动物体）</summary>
            EveryFrame,
            /// <summary>仅在Start时设置一次（适合静态物体）</summary>
            OnceOnStart,
            /// <summary>手动调用更新</summary>
            Manual
        }

        #endregion

        #region 生命周期

        private void Awake()
        {
            mSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            mLastY = transform.position.y;
            UpdateSortingOrder();
        }

        private void LateUpdate()
        {
            if (mUpdateMode != UpdateMode.EveryFrame) return;

            // 仅当Y坐标变化时更新，避免不必要的计算
            float currentY = transform.position.y;
            if (!Mathf.Approximately(currentY, mLastY))
            {
                mLastY = currentY;
                UpdateSortingOrder();
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 手动更新排序顺序
        /// </summary>
        public void UpdateSortingOrder()
        {
            if (mSpriteRenderer == null) return;

            // 设置排序层级
            if (!string.IsNullOrEmpty(mSortingLayerName))
            {
                mSpriteRenderer.sortingLayerName = mSortingLayerName;
            }

            // 计算排序值：基础值 - Y坐标 * 精度
            // Y越小（屏幕越下方）-> 排序值越大 -> 渲染在前面
            float effectiveY = transform.position.y + mYOffset;
            int calculatedOrder = mBaseSortingOrder - Mathf.RoundToInt(effectiveY * mPrecision);
            mSpriteRenderer.sortingOrder = calculatedOrder;
        }

        /// <summary>
        /// 设置更新模式
        /// </summary>
        public void SetUpdateMode(UpdateMode mode)
        {
            mUpdateMode = mode;
        }

        #endregion

        #region 编辑器辅助

#if UNITY_EDITOR
        private void OnValidate()
        {
            // 编辑器中实时预览排序效果
            if (mSpriteRenderer == null)
                mSpriteRenderer = GetComponent<SpriteRenderer>();
            
            if (mSpriteRenderer != null)
                UpdateSortingOrder();
        }
#endif

        #endregion
    }
}
