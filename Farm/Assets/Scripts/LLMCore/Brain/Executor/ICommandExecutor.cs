using Cysharp.Threading.Tasks;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 指令执行器接口
    /// </summary>
    public interface ICommandExecutor
    {
        /// <summary>该执行器处理的指令类型</summary>
        string CommandType { get; }

        /// <summary>执行指令（发起执行，不等待完成）</summary>
        void Execute(ICommand command, DecisionContext context);
    }
}
