using UnityEngine;
using FarmGame.LLMCore.Brain;

namespace FarmGame.Game.NPC
{
    /// <summary>
    /// 气泡事件桥接器
    /// 作为全局监听器，处理 SpeakExecutor.OnSpeak 事件
    /// 当 speaker 为 null 或无法直接匹配 NPCController 时提供兜底处理
    /// </summary>
    public class BubbleEventBridge : MonoBehaviour
    {
        #region 单例

        private static BubbleEventBridge mInstance;

        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static BubbleEventBridge Instance
        {
            get
            {
                if (mInstance == null)
                {
                    var go = new GameObject("[BubbleEventBridge]");
                    mInstance = go.AddComponent<BubbleEventBridge>();
                    DontDestroyOnLoad(go);
                }
                return mInstance;
            }
        }

        #endregion

        #region 配置

        [Tooltip("当speaker为null时，是否尝试通过NPCManager查找当前激活的NPC")]
        [SerializeField] private bool mFallbackToActiveNpc = true;

        #endregion

        #region 生命周期

        private void Awake()
        {
            if (mInstance != null && mInstance != this)
            {
                Destroy(gameObject);
                return;
            }

            mInstance = this;
            DontDestroyOnLoad(gameObject);

            // 订阅事件（使用 LLMCore 中的 SpeakExecutor）
            LLMCore.Brain.SpeakExecutor.OnSpeak += OnSpeakEvent;
        }

        private void OnDestroy()
        {
            LLMCore.Brain.SpeakExecutor.OnSpeak -= OnSpeakEvent;

            if (mInstance == this)
            {
                mInstance = null;
            }
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 处理说话事件
        /// 主要用于 speaker 为 null 的情况
        /// </summary>
        private void OnSpeakEvent(GameObject speaker, string content)
        {
            // 如果 speaker 不为空，由 NPCController 自己处理
            if (speaker != null) return;

            if (!mFallbackToActiveNpc) return;

            // 尝试通过 NPCManager 找到可能的说话者
            // 这里可以根据具体需求实现逻辑
            // 例如：找到最近与玩家交互的NPC
            Debug.LogWarning($"[BubbleEventBridge] 收到说话事件但speaker为null，内容: {content}");
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 手动初始化桥接器（可选）
        /// 通常由 BootManager 在启动时调用
        /// </summary>
        public void Initialize()
        {
            Debug.Log("[BubbleEventBridge] 初始化完成");
        }

        /// <summary>
        /// 强制让指定NPC显示气泡
        /// </summary>
        /// <param name="npcId">NPC ID</param>
        /// <param name="content">气泡内容</param>
        /// <param name="duration">显示时长，-1使用默认值</param>
        public void ShowBubbleForNpc(string npcId, string content, float duration = -1f)
        {
            var controller = NPCManager.Instance?.GetController(npcId);
            if (controller != null)
            {
                controller.ShowBubble(content, duration);
            }
            else
            {
                Debug.LogWarning($"[BubbleEventBridge] 未找到NPC: {npcId}");
            }
        }

        /// <summary>
        /// 隐藏指定NPC的气泡
        /// </summary>
        /// <param name="npcId">NPC ID</param>
        public void HideBubbleForNpc(string npcId)
        {
            var controller = NPCManager.Instance?.GetController(npcId);
            if (controller != null)
            {
                controller.HideBubble();
            }
        }

        #endregion
    }
}
