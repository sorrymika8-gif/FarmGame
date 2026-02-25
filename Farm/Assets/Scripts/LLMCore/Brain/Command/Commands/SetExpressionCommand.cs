using System;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 设置表情指令
    /// 用于改变NPC的面部表情/立绘显示
    /// </summary>
    [Serializable]
    public class SetExpressionCommand : ICommand
    {
        public string CommandType => CommandTypes.SetExpression;

        /// <summary>表情ID（如 "happy", "sad", "angry" 等）</summary>
        public string Expression { get; set; }
    }
}
