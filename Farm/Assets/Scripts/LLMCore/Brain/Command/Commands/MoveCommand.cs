using System;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>移动指令</summary>
    [Serializable]
    public class MoveCommand : ICommand
    {
        public string CommandType => CommandTypes.Move;

        /// <summary>目标X坐标</summary>
        public float TargetX { get; set; }

        /// <summary>目标Y坐标</summary>
        public float TargetY { get; set; }
    }
}
