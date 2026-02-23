using System;

namespace FarmGame.Core.LLMDescription
{
    /// <summary>
    /// 标记属性为可描述属性
    /// 被标记的属性会被 DescriptionContextBuilder 自动收集并用于提示词生成
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DescribableAttribute : Attribute
    {
        /// <summary>
        /// 占位符名称（用于模板中的 {{Key}}）
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// 中文描述（可选，用于调试或日志）
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 是否在描述中包含此属性
        /// </summary>
        public bool IncludeInDescription { get; set; } = true;

        /// <summary>
        /// 创建可描述属性特性
        /// </summary>
        /// <param name="key">占位符名称，用于模板替换 {{key}}</param>
        /// <param name="description">中文描述（可选）</param>
        public DescribableAttribute(string key, string description = null)
        {
            Key = key;
            Description = description;
        }
    }

    /// <summary>
    /// 标记嵌套对象中需要提取的属性
    /// 用于从关联对象（如配置数据）中提取属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class DescribableNestedAttribute : Attribute
    {
        /// <summary>
        /// 嵌套属性路径（如 "PlantData.name"）
        /// </summary>
        public string PropertyPath { get; }

        /// <summary>
        /// 占位符名称
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// 中文描述
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 创建嵌套属性特性
        /// </summary>
        /// <param name="propertyPath">属性路径</param>
        /// <param name="key">占位符名称</param>
        /// <param name="description">中文描述（可选）</param>
        public DescribableNestedAttribute(string propertyPath, string key, string description = null)
        {
            PropertyPath = propertyPath;
            Key = key;
            Description = description;
        }
    }
}
