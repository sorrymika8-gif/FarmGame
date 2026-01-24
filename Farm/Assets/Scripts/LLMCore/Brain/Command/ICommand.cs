namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 指令接口
    /// 所有指令的基类
    /// </summary>
    public interface ICommand
    {
        /// <summary>指令类型标识</summary>
        string CommandType { get; }
    }
}
