using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FarmGame.Core.LLMDescription
{
    /// <summary>
    /// 描述上下文
    /// 封装从可描述对象收集的属性数据，用于模板替换
    /// </summary>
    public class DescriptionContext
    {
        /// <summary>
        /// 对象类型标识
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 对象显示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 缓存键
        /// </summary>
        public string CacheKey { get; set; }

        /// <summary>
        /// 收集到的属性字典
        /// Key: 占位符名称, Value: 属性值
        /// </summary>
        public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        /// <summary>
        /// 添加属性
        /// </summary>
        /// <param name="key">占位符名称</param>
        /// <param name="value">属性值</param>
        /// <returns>当前上下文（链式调用）</returns>
        public DescriptionContext AddProperty(string key, object value)
        {
            Properties[key] = value;
            return this;
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">占位符名称</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>属性值或默认值</returns>
        public T GetProperty<T>(string key, T defaultValue = default)
        {
            if (Properties.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// 替换模板中的占位符
        /// 将 {{Key}} 替换为对应的属性值
        /// </summary>
        /// <param name="template">模板字符串</param>
        /// <returns>替换后的字符串</returns>
        public string ReplaceTemplate(string template)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            // 使用正则表达式匹配 {{Key}} 格式的占位符
            var result = Regex.Replace(template, @"\{\{(\w+)\}\}", match =>
            {
                var key = match.Groups[1].Value;
                
                // 首先检查 Properties 字典
                if (Properties.TryGetValue(key, out var value))
                {
                    return FormatValue(value);
                }

                // 特殊处理内置属性
                switch (key)
                {
                    case "Type":
                        return Type ?? "";
                    case "DisplayName":
                        return DisplayName ?? "";
                    case "CacheKey":
                        return CacheKey ?? "";
                }

                // 未找到的占位符返回原样
                return match.Value;
            });

            return result;
        }

        /// <summary>
        /// 格式化属性值为字符串
        /// </summary>
        /// <param name="value">属性值</param>
        /// <returns>格式化后的字符串</returns>
        private string FormatValue(object value)
        {
            if (value == null)
                return "";

            // 布尔值转换为中文
            if (value is bool boolValue)
                return boolValue ? "是" : "否";

            // 浮点数保留两位小数
            if (value is float floatValue)
                return floatValue.ToString("F2");

            if (value is double doubleValue)
                return doubleValue.ToString("F2");

            return value.ToString();
        }

        /// <summary>
        /// 生成调试信息
        /// </summary>
        /// <returns>调试字符串</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[DescriptionContext] Type: {Type}, DisplayName: {DisplayName}");
            sb.AppendLine("Properties:");
            foreach (var kvp in Properties)
            {
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }
            return sb.ToString();
        }
    }
}
