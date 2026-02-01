using FarmGame.LLMCore.Brain;

namespace FarmGame.Game.NPC
{
    /// <summary>
    /// 对话指令
    /// </summary>
    public class SpeakCommand : ICommand
    {
        public string CommandType => "Speak";

        public string Content { get; set; }
        public string TargetId { get; set; }

        public SpeakCommand(string content, string targetId = null)
        {
            Content = content;
            TargetId = targetId;
        }
    }
}
