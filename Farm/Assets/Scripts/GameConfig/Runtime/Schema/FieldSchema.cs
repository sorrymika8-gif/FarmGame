// ==========================================================
// 自动生成配置系统 - 字段结构定义
// ==========================================================

using System;

namespace FarmGame.GameConfig
{
    /// <summary>
    /// 配置表字段结构定义
    /// </summary>
    [Serializable]
    public class FieldSchema
    {
        /// <summary>
        /// 字段名（英文，如 task_id）
        /// </summary>
        public string Name;

        /// <summary>
        /// 字段类型（如 int, string, int[]）
        /// </summary>
        public string Type;

        /// <summary>
        /// 字段注释（中文描述，如 任务id）
        /// </summary>
        public string Comment;

        /// <summary>
        /// 列索引（从0开始）
        /// </summary>
        public int ColumnIndex;

        /// <summary>
        /// 是否为主键字段
        /// </summary>
        public bool IsKey;

        /// <summary>
        /// 主键层级（0表示非主键，1-3表示主键层级）
        /// </summary>
        public int KeyLevel;

        /// <summary>
        /// 创建字段结构
        /// </summary>
        /// <param name="name">字段名</param>
        /// <param name="type">字段类型</param>
        /// <param name="comment">字段注释</param>
        /// <param name="columnIndex">列索引</param>
        public FieldSchema(string name, string type, string comment, int columnIndex)
        {
            Name = name;
            Type = type;
            Comment = comment;
            ColumnIndex = columnIndex;
            IsKey = false;
            KeyLevel = 0;
        }

        /// <summary>
        /// 获取 C# 类型名称
        /// </summary>
        public string GetCSharpType()
        {
            return SupportedTypes.GetCSharpType(Type);
        }

        /// <summary>
        /// 获取默认值表达式
        /// </summary>
        public string GetDefaultValue()
        {
            return SupportedTypes.GetDefaultValue(Type);
        }

        /// <summary>
        /// 是否为数组类型
        /// </summary>
        public bool IsArray => Type.EndsWith("[]");

        public override string ToString()
        {
            return $"[{ColumnIndex}] {Name}: {Type} // {Comment}";
        }
    }
}
