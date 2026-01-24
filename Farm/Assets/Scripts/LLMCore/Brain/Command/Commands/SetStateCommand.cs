using System;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>设置状态指令</summary>
    [Serializable]
    public class SetStateCommand : ICommand
    {
        public string CommandType => CommandTypes.SetState;

        /// <summary>状态Key</summary>
        public string Key { get; set; }

        /// <summary>状态Value</summary>
        public string Value { get; set; }
    }
}
