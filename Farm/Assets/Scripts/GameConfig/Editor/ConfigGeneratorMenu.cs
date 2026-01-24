// ==========================================================
// 自动生成配置系统 - Unity 编辑器工具
// ==========================================================

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FarmGame.GameConfig.Editor
{
    /// <summary>
    /// 配置代码生成器菜单
    /// </summary>
    public static class ConfigGeneratorMenu
    {
        private const string ExcelFolder = "Assets/Configs";
        private const string OutputFolder = "Assets/Scripts/GameConfig/Generated";
        private const string Namespace = "FarmGame.GameConfig.Generated";
        private const FieldNamingStyle FieldNaming = FieldNamingStyle.Original;
        private const bool UseProperties = false;
        private const bool GenerateComments = true;

        [MenuItem("Tools/FarmGame/Generate All Configs", false, 100)]
        public static void GenerateAll()
        {
            // 查找所有配置文件
            var files = Directory.GetFiles(ExcelFolder, "*.csv", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(ExcelFolder, "*.xlsx", SearchOption.AllDirectories))
                .Where(f => !Path.GetFileName(f).StartsWith("~$") && !f.EndsWith(".meta")) // 过滤临时文件
                .ToList();

            if (files.Count == 0)
            {
                Debug.LogWarning("[ConfigGenerator] 未找到任何配置文件 (.csv/.xlsx)");
                return;
            }

            GenerateFiles(files);
        }

        /// <summary>
        /// 生成指定的文件
        /// </summary>
        private static void GenerateFiles(List<string> files)
        {
            var settings = new CodeGenSettings
            {
                Namespace = Namespace,
                OutputPath = OutputFolder,
                FieldNaming = FieldNaming,
                UseProperties = UseProperties,
                GenerateFieldComments = GenerateComments,
                GenerateClassComments = GenerateComments
            };

            var generator = new ConfigCodeGenerator(settings);
            var parser = new ConfigSchemaParser();

            int successCount = 0;
            int failCount = 0;
            var errors = new List<string>();
            var generatedFilePaths = new List<string>();

            try
            {
                EditorUtility.DisplayProgressBar("生成配置代码", "准备中...", 0);

                for (int i = 0; i < files.Count; i++)
                {
                    var filePath = files[i];
                    var fileName = Path.GetFileName(filePath);
                    
                    // ... (progress bar update)

                    try
                    {
                        // 使用 CSV 解析（暂时不依赖 Excel 库）
                        var schema = ParseCsvFile(filePath, parser);

                        // 尝试 XLSX 解析
                        if (schema == null && Path.GetExtension(filePath).ToLower() == ".xlsx")
                        {
                            var rows = XlsxReader.Read(filePath);
                            if (rows != null && rows.Length > 0)
                            {
                                schema = parser.ParseFromArray(rows, filePath);
                            }
                        }

                        if (schema != null)
                        {
                            var outputPath = generator.GenerateToFile(schema);
                            generatedFilePaths.Add(Path.GetFullPath(outputPath)); // store full path for comparison
                            Debug.Log($"[ConfigGenerator] 已生成: {outputPath}");
                            successCount++;
                        }
                        else
                        {
                            errors.Add($"{fileName}: 解析失败或文件格式不支持");
                            failCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{fileName}: {ex.Message}");
                        failCount++;
                        Debug.LogError($"[ConfigGenerator] 生成失败: {fileName}\n{ex}");
                    }
                }

                // 清理旧文件
                CleanupUnusedFiles(generatedFilePaths);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            // 刷新资源
            AssetDatabase.Refresh();

            // 显示结果
            if (failCount == 0)
            {
                Debug.Log($"[ConfigGenerator] 生成完成！成功: {successCount} 个文件");
            }
            else
            {
                Debug.LogError($"[ConfigGenerator] 生成完成。成功: {successCount}，失败: {failCount}\n{string.Join("\n", errors)}");
            }
        }

        /// <summary>
        /// 清理未生成的文件
        /// </summary>
        private static void CleanupUnusedFiles(List<string> generatedFiles)
        {
            if (!Directory.Exists(OutputFolder)) return;
            
            // Normalize generated paths
            var normalizedGeneratedFiles = new HashSet<string>(
                generatedFiles.Select(p => Path.GetFullPath(p).Replace('\\', '/').ToLower())
            );

            var existingFiles = Directory.GetFiles(OutputFolder, "*.cs");
            foreach (var file in existingFiles)
            {
                var fullPath = Path.GetFullPath(file).Replace('\\', '/').ToLower();
                
                // 如果文件不在生成的列表中，则删除
                if (!normalizedGeneratedFiles.Contains(fullPath))
                {
                    try
                    {
                        File.Delete(file);
                        // Try delete meta file
                        var metaPath = file + ".meta";
                        if (File.Exists(metaPath)) File.Delete(metaPath);
                        
                        Debug.Log($"[ConfigGenerator] 已清理过期文件: {file}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ConfigGenerator] 清理文件失败: {file}\n{ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 解析 CSV 文件
        /// </summary>
        private static ConfigSchema ParseCsvFile(string filePath, ConfigSchemaParser parser)
        {
            var extension = Path.GetExtension(filePath).ToLower();

            if (extension == ".csv")
            {
                // CSV 文件直接解析
                var lines = File.ReadAllLines(filePath);
                var rows = lines.Select(line => ParseCsvLine(line)).ToArray();
                return parser.ParseFromArray(rows, filePath);
            }
            
            return null;
        }

        /// <summary>
        /// 解析 CSV 行
        /// </summary>
        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = "";
            var inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }

            result.Add(current);
            return result.ToArray();
        }
    }
}
#endif
