// ==========================================================
// 自动生成配置系统 - 配置表结构定义
// ==========================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmGame.GameConfig
{
    /// <summary>
    /// 配置表结构定义
    /// </summary>
    [Serializable]
    public class ConfigSchema
    {
        /// <summary>
        /// 表名（如 task）
        /// </summary>
        public string TableName;

        /// <summary>
        /// 生成的类名（如 TaskConfig）
        /// </summary>
        public string ClassName;

        /// <summary>
        /// 表注释/描述
        /// </summary>
        public string Comment;

        /// <summary>
        /// 配置格式（List/Map/GroupMap）
        /// </summary>
        public ConfigFormat Format;

        /// <summary>
        /// 主键字段名列表
        /// </summary>
        public List<string> Keys;

        /// <summary>
        /// 字段列表
        /// </summary>
        public List<FieldSchema> Fields;

        /// <summary>
        /// 数据起始行（从0开始）
        /// </summary>
        public int DataStartRow;

        /// <summary>
        /// 源文件路径
        /// </summary>
        public string SourceFilePath;

        public ConfigSchema()
        {
            Keys = new List<string>();
            Fields = new List<FieldSchema>();
            Format = ConfigFormat.List;
            DataStartRow = 8; // 默认第9行开始（0-indexed）
        }

        /// <summary>
        /// 主键层级数量
        /// </summary>
        public int KeyDepth => Keys?.Count ?? 0;

        /// <summary>
        /// 获取主键字段列表
        /// </summary>
        public List<FieldSchema> GetKeyFields()
        {
            if (Keys == null || Keys.Count == 0)
            {
                return new List<FieldSchema>();
            }

            var keyFields = new List<FieldSchema>();
            foreach (var keyName in Keys)
            {
                var field = Fields?.FirstOrDefault(f => f.Name == keyName);
                if (field != null)
                {
                    keyFields.Add(field);
                }
            }
            return keyFields;
        }

        /// <summary>
        /// 获取非主键字段列表
        /// </summary>
        public List<FieldSchema> GetNonKeyFields()
        {
            if (Keys == null || Keys.Count == 0)
            {
                return Fields ?? new List<FieldSchema>();
            }

            return Fields?.Where(f => !Keys.Contains(f.Name)).ToList() ?? new List<FieldSchema>();
        }

        /// <summary>
        /// 根据字段名获取字段
        /// </summary>
        public FieldSchema GetField(string fieldName)
        {
            return Fields?.FirstOrDefault(f => f.Name == fieldName);
        }

        /// <summary>
        /// 验证 Schema 是否有效
        /// </summary>
        /// <param name="errors">错误信息列表</param>
        /// <returns>是否有效</returns>
        public bool Validate(out List<string> errors)
        {
            errors = new List<string>();

            if (string.IsNullOrWhiteSpace(TableName))
            {
                errors.Add("表名不能为空");
            }

            if (string.IsNullOrWhiteSpace(ClassName))
            {
                errors.Add("类名不能为空");
            }

            if (Fields == null || Fields.Count == 0)
            {
                errors.Add("字段列表不能为空");
            }

            // 验证主键字段存在
            if (Keys != null)
            {
                foreach (var key in Keys)
                {
                    if (Fields?.All(f => f.Name != key) ?? true)
                    {
                        errors.Add($"主键字段 '{key}' 在字段列表中不存在");
                    }
                }
            }

            // 验证字段类型
            if (Fields != null)
            {
                foreach (var field in Fields)
                {
                    if (!SupportedTypes.IsSupported(field.Type))
                    {
                        errors.Add($"字段 '{field.Name}' 使用了不支持的类型: {field.Type}");
                    }
                }
            }

            // 验证 format 与 key 的组合
            if (Format != ConfigFormat.List && (Keys == null || Keys.Count == 0))
            {
                errors.Add($"格式 '{Format}' 需要至少一个主键");
            }

            return errors.Count == 0;
        }

        /// <summary>
        /// 将表名转换为类名（下划线转帕斯卡命名 + Config 后缀）
        /// </summary>
        public static string TableNameToClassName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return "UnnamedConfig";
            }

            // 移除 .csv 等后缀
            var dotIndex = tableName.IndexOf('.');
            if (dotIndex > 0)
            {
                tableName = tableName.Substring(0, dotIndex);
            }

            // 下划线分隔转帕斯卡命名
            var parts = tableName.Split('_');
            var className = string.Join("", parts.Select(p =>
                string.IsNullOrEmpty(p) ? "" :
                char.ToUpper(p[0]) + (p.Length > 1 ? p.Substring(1).ToLower() : "")
            ));

            // 添加 Config 后缀
            if (!className.EndsWith("Config"))
            {
                className += "Config";
            }

            return className;
        }

        public override string ToString()
        {
            return $"[{ClassName}] Format={Format}, Keys=[{string.Join(",", Keys ?? new List<string>())}], Fields={Fields?.Count ?? 0}";
        }
    }
}
