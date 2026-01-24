using System;
using System.Collections.Generic;
using FarmGame.LLMCore.Memory;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 决策上下文
    /// 包含大脑做决策时需要的所有数据
    /// </summary>
    [Serializable]
    public class DecisionContext
    {
        /// <summary>决策类型</summary>
        public string DecisionType { get; set; }

        /// <summary>角色设定（名字、性格、背景等）</summary>
        public Dictionary<string, string> CharacterProfile { get; set; } = new();

        /// <summary>当前属性（血量、攻击力、情绪等）</summary>
        public Dictionary<string, object> CurrentState { get; set; } = new();

        /// <summary>环境感知（看到什么、听到什么）</summary>
        public Dictionary<string, object> Perception { get; set; } = new();

        /// <summary>记忆存储引用</summary>
        public MemoryStore MemoryStore { get; set; }

        /// <summary>触发事件（是什么导致了这次决策）</summary>
        public string TriggerEvent { get; set; }

        /// <summary>额外数据（特定决策类型可能需要的数据）</summary>
        public Dictionary<string, object> Extra { get; set; } = new();
    }
}
