using System;
using System.Text;

namespace GameLLM.Brain
{
    /// <summary>
    /// 角色设定
    /// 用于描述角色的基本信息，作为系统 Prompt的一部分传给 LLM
    /// </summary>
    [Serializable]
    public class CharacterSetting
    {
        public string Name;        // 姓名
        public string Identity;    // 身份/职业
        public string Personality; // 性格特征
        public string Goal;        // 核心目标
        public string Background;  // 背景故事/额外描述

        public CharacterSetting() { }

        public CharacterSetting(string name, string identity, string personality, string goal = null, string background = null)
        {
            Name = name;
            Identity = identity;
            Personality = personality;
            Goal = goal;
            Background = background;
        }

        /// <summary>
        /// 生成用于 Prompt 的描述文本
        /// </summary>
        public string ToPrompt()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(Name)) sb.AppendLine($"- 姓名: {Name}");
            if (!string.IsNullOrEmpty(Identity)) sb.AppendLine($"- 身份: {Identity}");
            if (!string.IsNullOrEmpty(Personality)) sb.AppendLine($"- 性格: {Personality}");
            if (!string.IsNullOrEmpty(Goal)) sb.AppendLine($"- 目标: {Goal}");
            if (!string.IsNullOrEmpty(Background)) sb.AppendLine($"- 背景: {Background}");
            return sb.ToString();
        }

        public override string ToString() => ToPrompt();
    }
}
