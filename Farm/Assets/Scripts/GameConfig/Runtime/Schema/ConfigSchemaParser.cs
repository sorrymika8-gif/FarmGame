// ==========================================================
// 自动生成配置系统 - Excel 元数据解析器
// ==========================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FarmGame.GameConfig
{
    /// <summary>
    /// Excel 配置表元数据解析器
    /// 解析 Excel 前 8 行，提取表结构信息
    /// </summary>
    public class ConfigSchemaParser
    {
        /// <summary>
        /// 表名标识行（第1行）
        /// </summary>
        private const int ROW_TABLE_NAME = 0;

        /// <summary>
        /// 注释说明行（第2行）
        /// </summary>
        private const int ROW_COMMENT = 1;

        /// <summary>
        /// JSON 元数据行（第4行）
        /// </summary>
        private const int ROW_METADATA = 3;

        /// <summary>
        /// 中文字段描述行（第6行）
        /// </summary>
        private const int ROW_FIELD_COMMENT = 5;

        /// <summary>
        /// 字段类型行（第7行）
        /// </summary>
        private const int ROW_FIELD_TYPE = 6;

        /// <summary>
        /// 字段名行（第8行）
        /// </summary>
        private const int ROW_FIELD_NAME = 7;

        /// <summary>
        /// 数据起始行（第9行）
        /// </summary>
        private const int ROW_DATA_START = 8;

        /// <summary>
        /// 从 Excel 读取器解析 Schema
        /// </summary>
        /// <param name="reader">Excel 读取器</param>
        /// <param name="filePath">文件路径（用于错误提示）</param>
        /// <returns>配置表结构</returns>
        public ConfigSchema Parse(IExcelReader reader, string filePath = null)
        {
            var schema = new ConfigSchema
            {
                SourceFilePath = filePath,
                DataStartRow = ROW_DATA_START
            };

            // 1. 解析表名（第1行）
            ParseTableName(reader, schema);

            // 2. 解析注释（第2行）
            ParseComment(reader, schema);

            // 3. 解析元数据（第4行）
            ParseMetadata(reader, schema);

            // 4. 解析字段（第6-8行）
            ParseFields(reader, schema);

            // 5. 标记主键字段
            MarkKeyFields(schema);

            return schema;
        }

        /// <summary>
        /// 解析表名（第1行）
        /// 改为读取文件名
        /// </summary>
        private void ParseTableName(IExcelReader reader, ConfigSchema schema)
        {
            // var value = reader.GetCellValue(ROW_TABLE_NAME, 0)?.Trim() ?? "";

            // // 移除 # 前缀
            // if (value.StartsWith("#"))
            // {
            //     value = value.Substring(1);
            // }

            // 改为读取文件名
            var value = System.IO.Path.GetFileNameWithoutExtension(schema.SourceFilePath);

            schema.TableName = value;
            schema.ClassName = ConfigSchema.TableNameToClassName(value);
        }

        /// <summary>
        /// 解析注释（第2行）
        /// 格式：#任务配置表
        /// </summary>
        private void ParseComment(IExcelReader reader, ConfigSchema schema)
        {
            var value = reader.GetCellValue(ROW_COMMENT, 0)?.Trim() ?? "";

            // 移除 # 前缀
            if (value.StartsWith("#"))
            {
                value = value.Substring(1);
            }

            schema.Comment = value;
        }

        /// <summary>
        /// 解析元数据（第4行）
        /// 格式：{"format":"map","key":["task_id","task_step"]}
        /// </summary>
        private void ParseMetadata(IExcelReader reader, ConfigSchema schema)
        {
            var value = reader.GetCellValue(ROW_METADATA, 0)?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(value))
            {
                // 默认为 list 格式
                schema.Format = ConfigFormat.List;
                schema.Keys = new List<string>();
                return;
            }

            try
            {
                var json = JObject.Parse(value);

                // 解析 format
                var formatStr = json["format"]?.ToString()?.ToLower() ?? "list";
                schema.Format = formatStr switch
                {
                    "list" => ConfigFormat.List,
                    "map" => ConfigFormat.Map,
                    "group_map" => ConfigFormat.GroupMap,
                    "groupmap" => ConfigFormat.GroupMap,
                    _ => ConfigFormat.List
                };

                // 解析 key
                var keyArray = json["key"] as JArray;
                schema.Keys = new List<string>();
                if (keyArray != null)
                {
                    foreach (var key in keyArray)
                    {
                        var keyStr = key?.ToString();
                        if (!string.IsNullOrWhiteSpace(keyStr))
                        {
                            schema.Keys.Add(keyStr);
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                throw new FormatException($"解析元数据失败（第{ROW_METADATA + 1}行）: {ex.Message}\n原始值: {value}");
            }
        }

        /// <summary>
        /// 解析字段（第6-8行）
        /// </summary>
        private void ParseFields(IExcelReader reader, ConfigSchema schema)
        {
            schema.Fields = new List<FieldSchema>();

            // 获取列数（通过字段名行判断）
            var colIndex = 0;
            while (true)
            {
                var fieldName = reader.GetCellValue(ROW_FIELD_NAME, colIndex)?.Trim();
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    break;
                }

                var fieldType = reader.GetCellValue(ROW_FIELD_TYPE, colIndex)?.Trim() ?? "string";
                var fieldComment = reader.GetCellValue(ROW_FIELD_COMMENT, colIndex)?.Trim() ?? "";

                // 验证类型是否支持
                if (!SupportedTypes.IsSupported(fieldType))
                {
                    throw new NotSupportedException(
                        $"字段 '{fieldName}'（列{colIndex + 1}）使用了不支持的类型: {fieldType}");
                }

                var field = new FieldSchema(fieldName, fieldType, fieldComment, colIndex);
                schema.Fields.Add(field);

                colIndex++;

                // 防止无限循环
                if (colIndex > 1000)
                {
                    throw new InvalidOperationException("字段数量超过限制（1000）");
                }
            }

            if (schema.Fields.Count == 0)
            {
                throw new InvalidOperationException("未找到任何字段定义");
            }
        }

        /// <summary>
        /// 标记主键字段
        /// </summary>
        private void MarkKeyFields(ConfigSchema schema)
        {
            if (schema.Keys == null || schema.Keys.Count == 0)
            {
                return;
            }

            for (int i = 0; i < schema.Keys.Count; i++)
            {
                var keyName = schema.Keys[i];
                var field = schema.GetField(keyName);
                if (field != null)
                {
                    field.IsKey = true;
                    field.KeyLevel = i + 1;
                }
            }
        }

        /// <summary>
        /// 从字符串数组解析 Schema（用于测试或 CSV）
        /// </summary>
        /// <param name="rows">行数据（每行是列数组）</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>配置表结构</returns>
        public ConfigSchema ParseFromArray(string[][] rows, string filePath = null)
        {
            var mockReader = new ArrayExcelReader(rows);
            return Parse(mockReader, filePath);
        }

        /// <summary>
        /// 用于测试的数组读取器
        /// </summary>
        private class ArrayExcelReader : IExcelReader
        {
            private readonly string[][] mRows;

            public ArrayExcelReader(string[][] rows)
            {
                mRows = rows;
            }

            public void Open(string filePath) { }
            public void Close() { }
            public void Dispose() { }

            public string GetCellValue(int row, int col)
            {
                if (row < 0 || row >= mRows.Length)
                {
                    return null;
                }
                var rowData = mRows[row];
                if (col < 0 || col >= rowData.Length)
                {
                    return null;
                }
                return rowData[col];
            }

            public int GetRowCount() => mRows.Length;
            public int GetColumnCount() => mRows.Length > 0 ? mRows[0].Length : 0;
            public bool HasSheet(string sheetName) => true;
            public void SetActiveSheet(string sheetName) { }
            public void SetActiveSheet(int sheetIndex) { }
            public IEnumerable<string> GetSheetNames() { yield return "Sheet1"; }
        }
    }
}
