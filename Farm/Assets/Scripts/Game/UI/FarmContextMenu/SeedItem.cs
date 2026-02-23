using System;
using UnityEngine;
using UnityEngine.UI;
using FarmGame.GameConfig;
using FarmGame.GameConfig.Generated;
using FarmGame.Item;

namespace FarmGame.UI
{
    /// <summary>
    /// 种子选择条目控制器
    /// 挂载在 SeedItem 预制体上
    /// </summary>
    public class SeedItem : MonoBehaviour
    {
        #region 序列化字段

        [SerializeField] private Button mButton;
        [SerializeField] private Image mIcon;
        [SerializeField] private Text mNameText;
        [SerializeField] private Text mCountText;

        #endregion

        #region 私有字段

        private int mItemId;
        private int mCount;
        private Action<int> mOnClick;

        #endregion

        #region 公共属性

        /// <summary>
        /// 种子物品ID
        /// </summary>
        public int ItemId => mItemId;

        /// <summary>
        /// 当前数量
        /// </summary>
        public int Count => mCount;

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
        /// 设置种子数据
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <param name="itemName">物品名称</param>
        /// <param name="count">数量</param>
        /// <param name="icon">图标（可选）</param>
        /// <param name="onClick">点击回调</param>
        public void Setup(int itemId, string itemName, int count, Sprite icon = null, Action<int> onClick = null)
        {
            mItemId = itemId;
            mCount = count;
            mOnClick = onClick;

            if (mNameText != null)
                mNameText.text = itemName;

            if (mCountText != null)
                mCountText.text = $"x{count}";

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
        }

        /// <summary>
        /// 使用物品配置信息设置数据
        /// </summary>
        /// <param name="configInfo">物品配置信息</param>
        /// <param name="count">数量</param>
        /// <param name="onClick">点击回调</param>
        public void Setup(ItemConfigInfo configInfo, int count, Action<int> onClick = null)
        {
            if (!configInfo.IsValid)
            {
                Debug.LogWarning("[SeedItem] configInfo 无效");
                return;
            }

            // 尝试加载图标（尝试多种路径格式）
            Sprite icon = null;
            if (!string.IsNullOrEmpty(configInfo.Icon))
            {
                string[] pathFormats = new string[]
                {
                    "prefabs/" + configInfo.Icon,           // prefabs/plants/pasture_seed
                    configInfo.Icon,                         // plants/pasture_seed
                    "Sprites/Farm/" + configInfo.Icon,      // Sprites/Farm/plants/pasture_seed
                    "Sprites/" + configInfo.Icon,           // Sprites/plants/pasture_seed
                };

                foreach (var path in pathFormats)
                {
                    icon = Core.ResourceManager.Instance?.Load<Sprite>(path);
                    if (icon != null)
                    {
                        Debug.Log($"[SeedItem] 成功加载图标: '{path}'");
                        break;
                    }
                }
                
                if (icon == null)
                {
                    Debug.LogWarning($"[SeedItem] 无法加载图标: '{configInfo.Icon}'");
                }
            }

            Setup(configInfo.ClassId, configInfo.Name, count, icon, onClick);
        }

        /// <summary>
        /// 更新数量显示
        /// </summary>
        /// <param name="newCount">新数量</param>
        public void UpdateCount(int newCount)
        {
            mCount = newCount;
            if (mCountText != null)
                mCountText.text = $"x{newCount}";
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
            mOnClick?.Invoke(mItemId);
        }

        #endregion
    }
}
