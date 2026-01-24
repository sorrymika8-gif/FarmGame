using System;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>说话指令</summary>
    [Serializable]
    public class SpeakCommand : ICommand
    {
        public string CommandType => CommandTypes.Speak;

        /// <summary>说话内容</summary>
        public string Content { get; set; }
    }
}
