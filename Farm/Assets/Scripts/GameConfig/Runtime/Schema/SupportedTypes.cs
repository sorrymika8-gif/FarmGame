// ==========================================================
// 自动生成配置系统 - 支持的类型定义
// ==========================================================

using System;
using System.Collections.Generic;

namespace FarmGame.GameConfig
{
    /// <summary>
    /// 配置格式类型
    /// </summary>
    public enum ConfigFormat
    {
        /// <summary>纯列表格式</summary>
        List,
        /// <summary>字典格式（按 key 索引）</summary>
        Map,
        /// <summary>分组字典格式（值为 List）</summary>
        GroupMap
    }

    /// <summary>
    /// 支持的类型定义与映射
    /// </summary>
    public static class SupportedTypes
    {
        /// <summary>
        /// Excel 类型 → C# 类型映射
        /// </summary>
        private static readonly Dictionary<string, string> TypeMapping = new Dictionary<string, string>
        {
            { "int", "int" },
            { "long", "long" },
            { "float", "float" },
            { "double", "double" },
            { "bool", "bool" },
            { "string", "string" },
            { "int[]", "int[]" },
            { "long[]", "long[]" },
            { "float[]", "float[]" },
            { "double[]", "double[]" },
            { "bool[]", "bool[]" },
            { "string[]", "string[]" },
            { "json", "Dictionary<string, object>" },
        };

        /// <summary>
        /// 类型默认值映射
        /// </summary>
        private static readonly Dictionary<string, string> DefaultValues = new Dictionary<string, string>
        {
            { "int", "0" },
            { "long", "0L" },
            { "float", "0f" },
            { "double", "0d" },
            { "bool", "false" },
            { "string", "\"\"" },
            { "int[]", "Array.Empty<int>()" },
            { "long[]", "Array.Empty<long>()" },
            { "float[]", "Array.Empty<float>()" },
            { "double[]", "Array.Empty<double>()" },
            { "bool[]", "Array.Empty<bool>()" },
            { "string[]", "Array.Empty<string>()" },
            { "json", "new Dictionary<string, object>()" },
        };

        /// <summary>
        /// 检查类型是否支持
        /// </summary>
        /// <param name="excelType">Excel 中的类型字符串</param>
        /// <returns>是否支持</returns>
        public static bool IsSupported(string excelType)
        {
            var normalizedType = NormalizeType(excelType);
            return TypeMapping.ContainsKey(normalizedType);
        }

        /// <summary>
        /// 获取 C# 类型名称
        /// </summary>
        /// <param name="excelType">Excel 中的类型字符串</param>
        /// <returns>C# 类型名称</returns>
        public static string GetCSharpType(string excelType)
        {
            var normalizedType = NormalizeType(excelType);
            if (TypeMapping.TryGetValue(normalizedType, out var csharpType))
            {
                return csharpType;
            }
            throw new NotSupportedException($"不支持的类型: {excelType}");
        }

        /// <summary>
        /// 获取类型的默认值表达式
        /// </summary>
        /// <param name="excelType">Excel 中的类型字符串</param>
        /// <returns>默认值表达式</returns>
        public static string GetDefaultValue(string excelType)
        {
            var normalizedType = NormalizeType(excelType);
            if (DefaultValues.TryGetValue(normalizedType, out var defaultValue))
            {
                return defaultValue;
            }
            return "default";
        }

        /// <summary>
        /// 获取类型的 System.Type
        /// </summary>
        /// <param name="excelType">Excel 中的类型字符串</param>
        /// <returns>System.Type 对象</returns>
        public static Type GetSystemType(string excelType)
        {
            var normalizedType = NormalizeType(excelType);
            return normalizedType switch
            {
                "int" => typeof(int),
                "long" => typeof(long),
                "float" => typeof(float),
                "double" => typeof(double),
                "bool" => typeof(bool),
                "string" => typeof(string),
                "int[]" => typeof(int[]),
                "long[]" => typeof(long[]),
                "float[]" => typeof(float[]),
                "double[]" => typeof(double[]),
                "bool[]" => typeof(bool[]),
                "string[]" => typeof(string[]),
                "json" => typeof(Dictionary<string, object>),
                _ => throw new NotSupportedException($"不支持的类型: {excelType}")
            };
        }

        /// <summary>
        /// 标准化类型字符串（处理 # 后缀等）
        /// </summary>
        /// <param name="excelType">原始类型字符串</param>
        /// <returns>标准化后的类型</returns>
        public static string NormalizeType(string excelType)
        {
            if (string.IsNullOrWhiteSpace(excelType))
            {
                return "string";
            }

            // 处理 # 后缀（如 string#ref_graph=task）
            var hashIndex = excelType.IndexOf('#');
            if (hashIndex > 0)
            {
                excelType = excelType.Substring(0, hashIndex);
            }

            return excelType.Trim().ToLower();
        }

        /// <summary>
        /// 是否为数组类型
        /// </summary>
        public static bool IsArrayType(string excelType)
        {
            return NormalizeType(excelType).EndsWith("[]");
        }

        /// <summary>
        /// 获取数组元素类型
        /// </summary>
        public static string GetArrayElementType(string excelType)
        {
            var normalizedType = NormalizeType(excelType);
            if (normalizedType.EndsWith("[]"))
            {
                return normalizedType.Substring(0, normalizedType.Length - 2);
            }
            return normalizedType;
        }

        /// <summary>
        /// 获取所有支持的类型列表
        /// </summary>
        public static IEnumerable<string> GetAllSupportedTypes()
        {
            return TypeMapping.Keys;
        }
    }
}
