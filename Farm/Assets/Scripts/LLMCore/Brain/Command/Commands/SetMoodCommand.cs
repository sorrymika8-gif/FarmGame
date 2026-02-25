using System;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 设置心情指令
    /// 用于在日常气泡对话中显示emoji表情
    /// </summary>
    [Serializable]
    public class SetMoodCommand : ICommand
    {
        public string CommandType => CommandTypes.SetMood;

        /// <summary>心情emoji（如 "😊", "😢", "(´・ω・`)" 等）</summary>
        public string Emoji { get; set; }
    }
}
