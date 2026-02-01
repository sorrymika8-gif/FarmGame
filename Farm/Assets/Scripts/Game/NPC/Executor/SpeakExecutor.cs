using UnityEngine;
using FarmGame.LLMCore.Brain;

namespace FarmGame.Game.NPC
{
    /// <summary>
    /// 对话指令执行器
    /// </summary>
    public class SpeakExecutor : ICommandExecutor
    {
        public string CommandType => "Speak";

        public void Execute(ICommand command, DecisionContext context)
        {
            if (command is not SpeakCommand speakCmd) return;

            if (!context.CharacterProfile.TryGetValue("ID", out string npcId))
            {
                Debug.LogWarning("[SpeakExecutor] Context 中缺少 ID");
                return;
            }

            var controller = NPCManager.Instance.GetController(npcId);
            if (controller != null)
            {
                controller.Speak(speakCmd.Content);
            }
        }
    }
}
