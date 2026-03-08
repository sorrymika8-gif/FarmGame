using UnityEngine;
using FarmGame.Core;
using FarmGame.Shop;
using FarmGame.Player;

namespace FarmGame.Game.Interactable
{
    /// <summary>
    /// 商店可交互物体控制器
    /// 放置在场景中的商店物体上，玩家点击后打开商店UI
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class ShopInteractable : MonoBehaviour
    {
        #region 配置字段

        [Header("商店配置")]
        [SerializeField]
        [Tooltip("商店类型")]
        private ShopType mShopType = ShopType.SeedShop;

        [SerializeField]
        [Tooltip("交互距离")]
        private float mInteractionDistance = 2f;

        [SerializeField]
        [Tooltip("商店名称（用于气泡提示）")]
        private string mShopName = "商店";

        #endregion

        #region 可选配置

        [Header("可选配置")]
        [SerializeField]
        [Tooltip("交互提示文本")]
        private string mInteractionHint = "点击购物";

        [SerializeField]
        [Tooltip("超出距离时的提示")]
        private string mTooFarHint = "太远了，请靠近一点";

        [SerializeField]
        [Tooltip("是否显示交互提示")]
        private bool mShowHint = true;

        #endregion

        #region 组件引用

        private BoxCollider2D mCollider;
        private SpriteRenderer mSpriteRenderer;

        #endregion

        #region 私有字段

        private bool mIsPlayerNearby;

        #endregion

        #region 公共属性

        /// <summary>商店类型</summary>
        public ShopType ShopType => mShopType;

        /// <summary>商店名称</summary>
        public string ShopName => mShopName;

        /// <summary>交互距离</summary>
        public float InteractionDistance => mInteractionDistance;

        #endregion

        #region 生命周期

        private void Awake()
        {
            // 获取或添加 Collider
            mCollider = GetComponent<BoxCollider2D>();
            if (mCollider == null)
            {
                mCollider = gameObject.AddComponent<BoxCollider2D>();
            }

            // 确保 Collider 不是触发器（用于点击检测）
            mCollider.isTrigger = false;

            // 获取 SpriteRenderer（可选）
            mSpriteRenderer = GetComponent<SpriteRenderer>();

            // 添加基于Y坐标的排序组件（建筑物使用静态模式）
            var sortByY = GetComponent<SpriteSortByY>();
            if (sortByY == null && mSpriteRenderer != null)
            {
                sortByY = gameObject.AddComponent<SpriteSortByY>();
                sortByY.SetUpdateMode(SpriteSortByY.UpdateMode.OnceOnStart);
            }
            if (sortByY != null)
            {
                sortByY.SortingLayerName = SortingLayerConfig.MapObjects;
            }
        }

        private void Update()
        {
            // 更新玩家距离状态
            mIsPlayerNearby = IsPlayerInInteractionRange();
        }

        #endregion

        #region 交互检测

        /// <summary>
        /// 鼠标点击检测（Unity 2D）
        /// </summary>
        private void OnMouseDown()
        {
            TryInteract();
        }

        /// <summary>
        /// 鼠标悬停进入
        /// </summary>
        private void OnMouseEnter()
        {
            if (mShowHint && mIsPlayerNearby)
            {
                ShowInteractionHint();
            }
        }

        /// <summary>
        /// 鼠标悬停离开
        /// </summary>
        private void OnMouseExit()
        {
            HideInteractionHint();
        }

        /// <summary>
        /// 尝试交互
        /// </summary>
        public void TryInteract()
        {
            if (!IsPlayerInInteractionRange())
            {
                if (mShowHint)
                {
                    ShowMessage(mTooFarHint);
                }
                return;
            }

            Interact();
        }

        /// <summary>
        /// 执行交互（打开商店）
        /// </summary>
        public void Interact()
        {
            // 检查是否已有商店面板打开
            if (UIManager.Instance.IsShopPanelOpen())
            {
                Debug.Log("[ShopInteractable] Shop panel already open");
                return;
            }

            // 获取玩家背包
            var inventory = PlayerManager.Instance?.Player?.Inventory;

            // 打开商店面板
            UIManager.Instance.OpenShopPanel(mShopType, inventory);

            Debug.Log($"[ShopInteractable] Opened {mShopType} shop");
        }

        #endregion

        #region 距离检测

        /// <summary>
        /// 检查玩家是否在交互范围内
        /// </summary>
        /// <returns>是否在范围内</returns>
        private bool IsPlayerInInteractionRange()
        {
            var player = PlayerManager.Instance?.Player;
            if (player == null) return false;

            // 2D 距离计算
            Vector2 shopPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 playerPos = new Vector2(player.transform.position.x, player.transform.position.y);
            float distance = Vector2.Distance(shopPos, playerPos);

            return distance <= mInteractionDistance;
        }

        /// <summary>
        /// 获取玩家到商店的距离
        /// </summary>
        /// <returns>距离</returns>
        public float GetDistanceToPlayer()
        {
            var player = PlayerManager.Instance?.Player;
            if (player == null) return float.MaxValue;

            Vector2 shopPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 playerPos = new Vector2(player.transform.position.x, player.transform.position.y);
            return Vector2.Distance(shopPos, playerPos);
        }

        #endregion

        #region UI提示

        /// <summary>
        /// 显示交互提示
        /// </summary>
        private void ShowInteractionHint()
        {
            // TODO: 可以扩展为显示UI提示气泡
            // 例如：BubbleManager.Instance.ShowBubble(transform, mInteractionHint);
            Debug.Log($"[ShopInteractable] {mInteractionHint}");
        }

        /// <summary>
        /// 隐藏交互提示
        /// </summary>
        private void HideInteractionHint()
        {
            // TODO: 隐藏UI提示气泡
            // 例如：BubbleManager.Instance.HideBubble(transform);
        }

        /// <summary>
        /// 显示消息
        /// </summary>
        private void ShowMessage(string message)
        {
            Debug.Log($"[ShopInteractable] {message}");
            // TODO: 可以扩展为显示Toast提示
        }

        #endregion

        #region 编辑器辅助

#if UNITY_EDITOR
        /// <summary>
        /// 在场景视图中绘制交互范围
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, mInteractionDistance);
        }
#endif

        #endregion
    }
}
