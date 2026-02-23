using UnityEngine;
using FarmGame.Movement;
using FarmGame.Core;
using FarmGame.UI;
using FarmGame.Player;
using FarmGame.LLMCore.Brain;

namespace FarmGame.Game.NPC
{
    /// <summary>
    /// NPC 控制器 (View/Controller层)
    /// 负责处理 NPC 的可视化表现、移动和交互
    /// 挂载在 NPC GameObject 上
    /// </summary>
    [RequireComponent(typeof(Movable))]
    public class NPCController : MonoBehaviour
    {
        #region 序列化字段

        [Header("气泡配置")]
        [Tooltip("气泡预制体")]
        [SerializeField] private NPCBubble mBubblePrefab;

        [Tooltip("气泡显示锚点（头顶位置）")]
        [SerializeField] private Transform mBubbleAnchor;

        [Tooltip("气泡相对锚点的偏移")]
        [SerializeField] private Vector3 mBubbleOffset = new Vector3(0, 0.5f, 0);

        #endregion

        #region 私有字段

        private Movable mMovable;
        private NPCEntity mEntity;
        private NPCBubble mBubbleInstance;

        #endregion

        #region 公共属性

        public string NpcId => mEntity?.Id;

        private void Awake()
        {
            mMovable = GetComponent<Movable>();
            
            // 确保有 Collider 用于射线检测 (点击交互)
            if (GetComponent<Collider2D>() == null)
            {
                var col = gameObject.AddComponent<BoxCollider2D>();
                col.isTrigger = true; 
                col.size = new Vector2(1, 1);
            }
        }

        private void Start()
        {
            // 如果是在场景中手动放置的 NPC，需要在 Start 时注册
            // 如果是动态生成的，由 Factory 负责初始化

            // 订阅说话事件（使用 LLMCore 中的 SpeakExecutor）
            LLMCore.Brain.SpeakExecutor.OnSpeak += OnNPCSpeak;

            // 初始化气泡
            InitializeBubble();
        }
        
        /// <summary>
        /// 交互接口
        /// </summary>
        public void Interact()
        {
            // 优先使用 DialogueSystem
            var dialogueSystem = GetComponent<DialogueSystem>();
            if (dialogueSystem != null)
            {
                dialogueSystem.StartDialogue();
            }
            else
            {
                // Fallback (应该不再需要，但保留兼容性)
                UIManager.Instance.OpenPanel<DialogueUIPanel>("UI/DialogUI/DiaLogUiPab", new DialogueUIData(mEntity));
            }
        }

        private void OnMouseDown()
        {
            Debug.Log($"[NPCController] OnMouseDown triggered on {gameObject.name}, mEntity={(mEntity != null ? mEntity.Name : "NULL")}");
            
            // 检查与玩家的距离
            if (!IsPlayerInInteractionRange())
            {
                Debug.Log($"[NPCController] Player not in interaction range, skipping interaction");
                return;
            }
            
            Debug.Log($"[NPCController] Starting interaction with {mEntity?.Name}");
            Interact();
        }
        
        /// <summary>
        /// 检查玩家是否在交互距离内
        /// </summary>
        private bool IsPlayerInInteractionRange()
        {
            if (mEntity == null)
            {
                Debug.LogWarning($"[NPCController] mEntity is NULL on {gameObject.name}! NPC not properly bound.");
                return false;
            }
            
            var player = PlayerManager.Instance?.Player;
            if (player == null)
            {
                Debug.LogWarning($"[NPCController] Player is NULL, allowing interaction by default");
                return true; // 如果获取不到玩家，默认允许交互
            }
            
            // 使用2D距离计算 (忽略Z轴)
            Vector2 npcPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 playerPos = new Vector2(player.transform.position.x, player.transform.position.y);
            float distance = Vector2.Distance(npcPos, playerPos);
            
            bool inRange = distance <= mEntity.InteractionDistance;
            Debug.Log($"[NPCController] Distance check: {distance:F2} / {mEntity.InteractionDistance:F2}, inRange={inRange}");
            
            return inRange;
        }

        /// <summary>
        /// 初始化 (绑定数据实体)
        /// </summary>
        public void Bind(NPCEntity entity)
        {
            mEntity = entity;
            if (mEntity == null) return;

            // 初始化位置
            transform.position = mEntity.Position;
            
            // 注册到 Manager (View 层注册)
            NPCManager.Instance.RegisterController(this);
        }

        private void OnDestroy()
        {
            // 取消订阅说话事件
            LLMCore.Brain.SpeakExecutor.OnSpeak -= OnNPCSpeak;

            // 销毁气泡实例
            if (mBubbleInstance != null)
            {
                Destroy(mBubbleInstance.gameObject);
                mBubbleInstance = null;
            }

            if (NPCManager.Instance && mEntity != null)
            {
                NPCManager.Instance.UnregisterController(mEntity.Id);
            }
        }

        private void Update()
        {
            if (mEntity == null) return;

            // 同步物理位置回 Entity
            mEntity.Position = transform.position;
        }

        #endregion

        #region 气泡相关

        /// <summary>
        /// 初始化气泡
        /// </summary>
        private void InitializeBubble()
        {
            if (mBubblePrefab == null)
            {
                // 尝试从Resources加载默认预制体
                mBubblePrefab = Resources.Load<NPCBubble>("UI/BubbleUI/NPCBubble");
                if (mBubblePrefab == null)
                {
                    Debug.LogWarning($"[NPCController] {gameObject.name} 未配置气泡预制体，且无法加载默认预制体");
                    return;
                }
            }

            // 确定锚点位置
            Transform anchor = mBubbleAnchor != null ? mBubbleAnchor : transform;

            // 实例化气泡
            mBubbleInstance = Instantiate(mBubblePrefab, anchor);
            mBubbleInstance.transform.localPosition = mBubbleOffset;
            mBubbleInstance.gameObject.SetActive(false);
        }

        /// <summary>
        /// 处理说话事件
        /// </summary>
        private void OnNPCSpeak(GameObject speaker, string content)
        {
            // 检查是否是自己
            if (speaker != gameObject) return;

            // 显示气泡
            ShowBubble(content);
        }

        /// <summary>
        /// 显示气泡
        /// </summary>
        /// <param name="content">显示内容</param>
        /// <param name="duration">显示时长，-1使用默认值</param>
        public void ShowBubble(string content, float duration = -1f)
        {
            if (mBubbleInstance == null)
            {
                Debug.LogWarning($"[NPCController] {gameObject.name} 气泡实例未初始化");
                return;
            }

            mBubbleInstance.Show(content, duration);
        }

        /// <summary>
        /// 隐藏气泡
        /// </summary>
        public void HideBubble()
        {
            if (mBubbleInstance != null)
            {
                mBubbleInstance.HideWithFade();
            }
        }

        #endregion

        #region 行为接口

        /// <summary>移动到指定位置</summary>
        public void MoveTo(Vector3 position)
        {
            if (mMovable)
            {
                mMovable.MoveTo(position);
            }
        }

        /// <summary>说话（会显示气泡）</summary>
        public void Speak(string content)
        {
            Debug.Log($"[NPC {mEntity?.Name}] 说: {content}");
            ShowBubble(content);
        }

        #endregion
    }
}
