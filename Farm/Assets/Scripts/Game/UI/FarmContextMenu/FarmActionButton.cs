using System;
using UnityEngine;
using UnityEngine.UI;

namespace FarmGame.UI
{
    /// <summary>
    /// 农场操作类型
    /// </summary>
    public enum FarmActionType
    {
        None,
        Till,       // 耕地
        Plant,      // 种植
        Harvest     // 收获
    }

    /// <summary>
    /// 农场操作按钮控制器
    /// 挂载在 ActionButton 预制体上
    /// </summary>
    public class FarmActionButton : MonoBehaviour
    {
        #region 序列化字段

        [SerializeField] private Button mButton;
        [SerializeField] private Image mIcon;
        [SerializeField] private Text mText;
        [SerializeField] private GameObject mArrow; // 有子菜单时显示的箭头

        #endregion

        #region 私有字段

        private FarmActionType mActionType;
        private Action<FarmActionType> mOnClick;

        #endregion

        #region 公共属性

        /// <summary>
        /// 操作类型
        /// </summary>
        public FarmActionType ActionType => mActionType;

        #endregion

        #region 生命周期

        private void Awake()
        {
            if (mButton == null)
                mButton = GetComponent<Button>();

            if (mButton != null)
                mButton.onClick.AddListener(OnButtonClick);
        }

        private void OnDestroy()
        {
            if (mButton != null)
                mButton.onClick.RemoveListener(OnButtonClick);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置按钮数据
        /// </summary>
        /// <param name="actionType">操作类型</param>
        /// <param name="text">按钮文本</param>
        /// <param name="icon">图标（可选）</param>
        /// <param name="showArrow">是否显示子菜单箭头</param>
        /// <param name="onClick">点击回调</param>
        public void Setup(FarmActionType actionType, string text, Sprite icon = null, bool showArrow = false, Action<FarmActionType> onClick = null)
        {
            mActionType = actionType;
            mOnClick = onClick;

            if (mText != null)
                mText.text = text;

            if (mIcon != null)
            {
                if (icon != null)
                {
                    mIcon.sprite = icon;
                    mIcon.gameObject.SetActive(true);
                }
                else
                {
                    mIcon.gameObject.SetActive(false);
                }
            }

            if (mArrow != null)
                mArrow.SetActive(showArrow);
        }

        /// <summary>
        /// 设置按钮交互状态
        /// </summary>
        /// <param name="interactable">是否可交互</param>
        public void SetInteractable(bool interactable)
        {
            if (mButton != null)
                mButton.interactable = interactable;
        }

        #endregion

        #region 私有方法

        private void OnButtonClick()
        {
            mOnClick?.Invoke(mActionType);
        }

        #endregion
    }
}
