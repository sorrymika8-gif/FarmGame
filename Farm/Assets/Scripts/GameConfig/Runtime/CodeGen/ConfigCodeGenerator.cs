// ==========================================================
// 自动生成配置系统 - C# 代码生成器
// ==========================================================

using System;
using System.IO;
using System.Text;

namespace FarmGame.GameConfig
{
    /// <summary>
    /// C# 配置类代码生成器
    /// </summary>
    public class ConfigCodeGenerator
    {
        private readonly CodeGenSettings mSettings;
        private StringBuilder mBuilder;

        public ConfigCodeGenerator() : this(CodeGenSettings.Default)
        {
        }

        public ConfigCodeGenerator(CodeGenSettings settings)
        {
            mSettings = settings ?? CodeGenSettings.Default;
        }

        /// <summary>
        /// 根据 Schema 生成 C# 代码
        /// </summary>
        /// <param name="schema">配置表结构</param>
        /// <returns>生成的 C# 代码</returns>
        public string Generate(ConfigSchema schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            mBuilder = new StringBuilder();

            // 生成文件头注释
            if (mSettings.GenerateFileHeader)
            {
                GenerateFileHeader(schema);
            }

            // 生成 using 语句
            GenerateUsings();

            // 生成命名空间开始
            AppendLine($"namespace {mSettings.Namespace}");
            AppendLine("{");

            // 生成类注释
            if (mSettings.GenerateClassComments && !string.IsNullOrWhiteSpace(schema.Comment))
            {
                AppendLine($"{mSettings.IndentString}/// <summary>");
                AppendLine($"{mSettings.IndentString}/// {schema.Comment}");
                AppendLine($"{mSettings.IndentString}/// </summary>");
            }

            // 生成 Serializable 特性
            if (mSettings.AddSerializableAttribute)
            {
                AppendLine($"{mSettings.IndentString}[Serializable]");
            }

            // 生成类定义
            AppendLine($"{mSettings.IndentString}public class {schema.ClassName}");
            AppendLine($"{mSettings.IndentString}{{");

            // 生成字段/属性
            GenerateFields(schema);

            // 类结束
            AppendLine($"{mSettings.IndentString}}}");

            // 命名空间结束
            AppendLine("}");

            return mBuilder.ToString();
        }

        /// <summary>
        /// 生成代码并保存到文件
        /// </summary>
        /// <param name="schema">配置表结构</param>
        /// <returns>生成的文件路径</returns>
        public string GenerateToFile(ConfigSchema schema)
        {
            var code = Generate(schema);
            var filePath = GetOutputFilePath(schema);

            // 确保目录存在
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 写入文件
            File.WriteAllText(filePath, code, Encoding.UTF8);

            return filePath;
        }

        /// <summary>
        /// 获取输出文件路径
        /// </summary>
        public string GetOutputFilePath(ConfigSchema schema)
        {
            return Path.Combine(mSettings.OutputPath, $"{schema.ClassName}.cs");
        }

        /// <summary>
        /// 生成文件头注释
        /// </summary>
        private void GenerateFileHeader(ConfigSchema schema)
        {
            AppendLine("// ==========================================================");
            AppendLine("// 自动生成，请勿手动修改");
            if (!string.IsNullOrWhiteSpace(schema.SourceFilePath))
            {
                AppendLine($"// 来源: {Path.GetFileName(schema.SourceFilePath)}");
            }
            if (!string.IsNullOrWhiteSpace(schema.Comment))
            {
                AppendLine($"// 描述: {schema.Comment}");
            }
            AppendLine($"// 生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            AppendLine("// ==========================================================");
            AppendLine();
        }

        /// <summary>
        /// 生成 using 语句
        /// </summary>
        private void GenerateUsings()
        {
            AppendLine("using System;");
            AppendLine("using System.Collections.Generic;");
            AppendLine();
        }

        /// <summary>
        /// 生成字段/属性
        /// </summary>
        private void GenerateFields(ConfigSchema schema)
        {
            var indent = mSettings.IndentString + mSettings.IndentString;
            var isFirst = true;

            foreach (var field in schema.Fields)
            {
                // 字段之间添加空行
                if (!isFirst)
                {
                    AppendLine();
                }
                isFirst = false;

                // 生成字段注释
                if (mSettings.GenerateFieldComments && !string.IsNullOrWhiteSpace(field.Comment))
                {
                    AppendLine($"{indent}/// <summary>{field.Comment}</summary>");
                }

                // 获取转换后的字段名和类型
                var fieldName = mSettings.ConvertFieldName(field.Name);
                var csharpType = field.GetCSharpType();

                if (mSettings.UseProperties)
                {
                    // 生成属性
                    AppendLine($"{indent}public {csharpType} {fieldName} {{ get; set; }}");
                }
                else
                {
                    // 生成字段
                    AppendLine($"{indent}public {csharpType} {fieldName};");
                }
            }
        }

        /// <summary>
        /// 添加一行代码
        /// </summary>
        private void AppendLine(string line = "")
        {
            mBuilder.Append(line);
            mBuilder.Append(mSettings.NewLine);
        }
    }
}
