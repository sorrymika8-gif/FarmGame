using System;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>攻击指令</summary>
    [Serializable]
    public class AttackCommand : ICommand
    {
        public string CommandType => CommandTypes.Attack;

        /// <summary>攻击目标ID</summary>
        public string TargetId { get; set; }
    }
}
