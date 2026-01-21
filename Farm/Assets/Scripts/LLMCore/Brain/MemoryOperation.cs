using System;

namespace GameLLM.Brain
{
    /// <summary>
    /// 记忆操作
    /// LLM 返回的操作指令
    /// </summary>
    [Serializable]
    public class MemoryOperation
    {
        public string type;           // Delete, Update, Transfer, Add
        
        // Common
        public int index;
        
        // Delete, Update, Add
        public string partition;
        
        // Update
        public string newContent;
        
        // Add
        public string content;
        
        // Transfer
        public string fromPartition;
        public string toPartition;
    }
}
