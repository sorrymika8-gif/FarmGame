using System;
using System.Collections.Generic;
using UnityEngine;
using FarmGame.LLMCore.Memory;
using FarmGame.LLMCore.Brain;
using Cysharp.Threading.Tasks;
using FarmGame.Item;

namespace FarmGame.Game.NPC
{
    /// <summary>
    /// NPC 实体
    /// 包含 NPC 的身份、状态、记忆和指令队列
    /// </summary>
    [Serializable]
    public class NPCEntity
    {
        #region 身份信息
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Gender { get; set; }
        #endregion

        #region 初始设定
        public string Personality { get; set; }
        public string Background { get; set; }
        public string Appearance { get; set; }
        #endregion

        #region 动态状态
        public string RoomId { get; set; }
        public Vector3 Position { get; set; }
        /// <summary>交互距离 (单位: Unity世界单位)</summary>
        public float InteractionDistance { get; set; } = 2f;
        public float Health { get; set; } = 100f;
        public float Hunger { get; set; } = 0f;
        public float Fatigue { get; set; } = 0f;
        public string Emotion { get; set; } = "neutral";
        public string CurrentActivity { get; set; } = "idle";
        #endregion

        #region 物品
        /// <summary>背包组件</summary>
        public InventoryComponent Inventory { get; private set; } = new InventoryComponent();
        #endregion

        #region 核心组件
        /// <summary>记忆存储 (每个 NPC 独有)</summary>
        public MemoryStore MemoryStore { get; }

        /// <summary>指令队列 (每个 NPC 独有)</summary>
        public CommandQueue CommandQueue { get; }
        #endregion

        public NPCEntity(string id, string name, CommandExecutorRegistry executorRegistry = null)
        {
            Id = id;
            Name = name;
            
            // 默认值
            Gender = "未知";
            Personality = "";
            Background = "";
            Appearance = "";

            MemoryStore = new MemoryStore();
            InitializeMemoryPartitions();

            // 如果没有传入，尝试获取全局共享的
            executorRegistry ??= NPCManager.Instance.SharedBrain?.ExecutorRegistry;
            
            if (executorRegistry != null)
            {
                CommandQueue = new CommandQueue(executorRegistry);
            }
            else
            {
                Debug.LogWarning($"[NPCEntity] ExecutorRegistry is null for {name}");
            }
        }

        private void InitializeMemoryPartitions()
        {
            // 默认分区（统一的记忆层级）
            MemoryStore.CreatePartition("short_term"); // "最近发生的事"（包括对话、行为等）
            MemoryStore.CreatePartition("long_term"); // "有些印象的事"
            MemoryStore.CreatePartition("permanent"); // "刻骨铭心的记忆"
        }

        public void InitializeMemories(string[] initialMemories)
        {
            if (initialMemories == null || initialMemories.Length == 0) return;

            var permanent = MemoryStore.GetPartition("permanent");
            if (permanent != null)
            {
                foreach (var memory in initialMemories) permanent.Add(new Memory(memory));
            }
        }

        /// <summary>
        /// 接收聊天信息并触发思考
        /// </summary>
        public async UniTask ReceiveChatAsync(string content)
        {
            // 打印玩家说的话
            Debug.Log($"[玩家] {content}");

            // 1. 将玩家消息存入短期记忆
            RecordMemory($"玩家对我说：{content}");

            // 2. 构建决策上下文
            var context = new DecisionContext
            {
                DecisionType = "Chat",
                MemoryStore = MemoryStore,
                TriggerEvent = "ReceiveChat"
            };
            
            // 填充上下文数据 (使用 Helper 构建)
            context.CharacterProfile = BuildCharacterProfile();
            context.CurrentState = BuildCurrentState();
            
            // 传入 NPCEntity 引用，供执行器记录行为
            context.Extra["NPCEntity"] = this;

            // 3. 触发大脑思考
            var brain = NPCManager.Instance.SharedBrain;
            if (brain != null)
            {
               var result = await brain.DecideAsync(context);
               
               if (result.Success && result.Commands != null && result.Commands.Count > 0)
               {
                   foreach (var cmd in result.Commands)
                   {
                        var executor = brain.ExecutorRegistry.Get(cmd.CommandType);
                        executor?.Execute(cmd, context);
                   }
               }
            }
        }

        /// <summary>
        /// 记录行为到短期记忆
        /// </summary>
        public void RecordMemory(string content)
        {
            var shortTerm = MemoryStore.GetPartition("short_term");
            shortTerm?.Add(new Memory(content));
        }

        /// <summary>构建角色设定字典</summary>
        public Dictionary<string, string> BuildCharacterProfile()
        {
            return new Dictionary<string, string>
            {
                ["ID"] = Id,
                ["Name"] = Name,
                ["Gender"] = Gender,
                ["Personality"] = Personality,
                ["Background"] = Background,
                ["Appearance"] = Appearance
            };
        }

        /// <summary>构建当前状态字典</summary>
        public Dictionary<string, object> BuildCurrentState()
        {
            return new Dictionary<string, object>
            {
                ["Position"] = Position, // Vector3 toString
                ["Health"] = Health,
                ["Hunger"] = Hunger,
                ["Fatigue"] = Fatigue,
                ["Emotion"] = Emotion,
                ["CurrentActivity"] = CurrentActivity
            };
        }
    }
}
