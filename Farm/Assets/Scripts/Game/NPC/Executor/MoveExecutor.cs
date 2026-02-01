using UnityEngine;
using FarmGame.LLMCore.Brain;

namespace FarmGame.Game.NPC
{
    /// <summary>
    /// 移动指令执行器
    /// </summary>
    public class MoveExecutor : ICommandExecutor
    {
        public string CommandType => "Move";

        public void Execute(ICommand command, DecisionContext context)
        {
            if (command is not MoveCommand moveCmd) return;

            // 从 Context 中获取 NPC ID
            if (!context.CharacterProfile.TryGetValue("ID", out string npcId))
            {
                Debug.LogWarning("[MoveExecutor] Context 中缺少 ID，无法执行移动");
                return;
            }

            // 获取 NPC 控制器
            var controller = NPCManager.Instance.GetController(npcId);
            if (controller != null)
            {
                controller.MoveTo(moveCmd.TargetPosition);
            }
            else
            {
                Debug.LogWarning($"[MoveExecutor] 找不到 NPC 控制器: {npcId}");
            }
        }
    }
}
