using UnityEngine;
using FarmGame.LLMCore.Brain;

namespace FarmGame.Game.NPC
{
    /// <summary>
    /// 移动指令
    /// </summary>
    public class MoveCommand : ICommand
    {
        public string CommandType => "Move"; // 可以在常量类中定义

        public Vector3 TargetPosition { get; set; }

        public MoveCommand(Vector3 targetPosition)
        {
            TargetPosition = targetPosition;
        }
    }
}
