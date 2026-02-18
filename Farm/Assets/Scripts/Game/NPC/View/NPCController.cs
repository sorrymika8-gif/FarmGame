using UnityEngine;
using FarmGame.Movement;
using FarmGame.Core;
using FarmGame.UI;
using FarmGame.Player;

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
        private Movable mMovable;
        private NPCEntity mEntity;

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
                UIManager.Instance.OpenPanel<DialogueUIPanel>("UI/DialogUI/DialogueUIPanel", new DialogueUIData(mEntity));
            }
        }

        private void OnMouseDown()
        {
            // 检查与玩家的距离
            if (!IsPlayerInInteractionRange())
            {
                return;
            }
            
            Interact();
        }
        
        /// <summary>
        /// 检查玩家是否在交互距离内
        /// </summary>
        private bool IsPlayerInInteractionRange()
        {
            if (mEntity == null) return false;
            
            var player = PlayerManager.Instance?.Player;
            if (player == null)
            {
                return true; // 如果获取不到玩家，默认允许交互
            }
            
            // 使用2D距离计算 (忽略Z轴)
            Vector2 npcPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 playerPos = new Vector2(player.transform.position.x, player.transform.position.y);
            float distance = Vector2.Distance(npcPos, playerPos);
            
            return distance <= mEntity.InteractionDistance;
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

        #region 行为接口

        /// <summary>移动到指定位置</summary>
        public void MoveTo(Vector3 position)
        {
            if (mMovable)
            {
                mMovable.MoveTo(position);
            }
        }

        /// <summary>说话</summary>
        public void Speak(string content)
        {
            Debug.Log($"[NPC {mEntity?.Name}] 说: {content}");
        }

        #endregion
    }
}
