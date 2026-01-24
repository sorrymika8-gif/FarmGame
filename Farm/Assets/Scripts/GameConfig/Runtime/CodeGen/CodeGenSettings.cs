// ==========================================================
// 自动生成配置系统 - 代码生成配置
// ==========================================================

using System;

namespace FarmGame.GameConfig
{
    /// <summary>
    /// 字段命名风格
    /// </summary>
    public enum FieldNamingStyle
    {
        /// <summary>保持原样（task_id）</summary>
        Original,
        /// <summary>帕斯卡命名（TaskId）</summary>
        PascalCase,
        /// <summary>驼峰命名（taskId）</summary>
        CamelCase
    }

    /// <summary>
    /// 代码生成配置
    /// </summary>
    [Serializable]
    public class CodeGenSettings
    {
        /// <summary>
        /// 生成代码的命名空间
        /// </summary>
        public string Namespace = "FarmGame.GameConfig.Generated";

        /// <summary>
        /// 生成代码的输出路径
        /// </summary>
        public string OutputPath = "Assets/Scripts/GameConfig/Generated";

        /// <summary>
        /// 是否使用属性而非字段
        /// </summary>
        public bool UseProperties = false;

        /// <summary>
        /// 字段命名风格
        /// </summary>
        public FieldNamingStyle FieldNaming = FieldNamingStyle.Original;

        /// <summary>
        /// 是否生成字段注释
        /// </summary>
        public bool GenerateFieldComments = true;

        /// <summary>
        /// 是否生成类注释
        /// </summary>
        public bool GenerateClassComments = true;

        /// <summary>
        /// 是否生成文件头注释
        /// </summary>
        public bool GenerateFileHeader = true;

        /// <summary>
        /// 是否添加 [Serializable] 特性
        /// </summary>
        public bool AddSerializableAttribute = true;

        /// <summary>
        /// 缩进字符串（默认4个空格）
        /// </summary>
        public string IndentString = "    ";

        /// <summary>
        /// 换行符
        /// </summary>
        public string NewLine = "\n";

        /// <summary>
        /// 默认配置
        /// </summary>
        public static CodeGenSettings Default => new CodeGenSettings();

        /// <summary>
        /// 将字段名转换为指定命名风格
        /// </summary>
        /// <param name="fieldName">原始字段名</param>
        /// <returns>转换后的字段名</returns>
        public string ConvertFieldName(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                return fieldName;
            }

            return FieldNaming switch
            {
                FieldNamingStyle.Original => fieldName,
                FieldNamingStyle.PascalCase => ToPascalCase(fieldName),
                FieldNamingStyle.CamelCase => ToCamelCase(fieldName),
                _ => fieldName
            };
        }

        /// <summary>
        /// 转换为帕斯卡命名（TaskId）
        /// </summary>
        private static string ToPascalCase(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            var parts = name.Split('_');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                {
                    parts[i] = char.ToUpper(parts[i][0]) +
                        (parts[i].Length > 1 ? parts[i].Substring(1).ToLower() : "");
                }
            }
            return string.Join("", parts);
        }

        /// <summary>
        /// 转换为驼峰命名（taskId）
        /// </summary>
        private static string ToCamelCase(string name)
        {
            var pascal = ToPascalCase(name);
            if (string.IsNullOrEmpty(pascal))
            {
                return pascal;
            }
            return char.ToLower(pascal[0]) + (pascal.Length > 1 ? pascal.Substring(1) : "");
        }
    }
}
