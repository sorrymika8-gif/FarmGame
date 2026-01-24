using System.Collections.Generic;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 决策结果
    /// </summary>
    public class DecisionResult
    {
        /// <summary>是否成功</summary>
        public bool Success { get; set; }

        /// <summary>错误信息</summary>
        public string ErrorMessage { get; set; }

        /// <summary>解析出的指令列表</summary>
        public List<ICommand> Commands { get; set; } = new();

        /// <summary>LLM 原始输出</summary>
        public string RawOutput { get; set; }

        /// <summary>处理时间（秒）</summary>
        public float ProcessingTime { get; set; }
    }
}
