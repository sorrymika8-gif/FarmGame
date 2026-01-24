using System;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>记忆操作指令</summary>
    [Serializable]
    public class MemoryOperationCommand : ICommand
    {
        public string CommandType => CommandTypes.MemoryOperation;

        /// <summary>操作类型 (Add/Remove)</summary>
        public string Operation { get; set; }

        /// <summary>目标分区</summary>
        public string Partition { get; set; }

        /// <summary>记忆内容</summary>
        public string Content { get; set; }
    }
}
