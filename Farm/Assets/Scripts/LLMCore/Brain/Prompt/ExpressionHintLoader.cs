using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 表情提示词片段加载器
    /// 负责从 Assets/Prompts/Expressions/ 目录加载各种表情的 Skill 描述
    /// 采用 AI Agent Skills 模式：每个表情一个 .md 文件，构建 Prompt 时动态加载
    /// </summary>
    public static class ExpressionHintLoader
    {
        /// <summary>表情片段目录路径 (相对于 Assets 目录)</summary>
        private const string EXPRESSIONS_DIR_REL_PATH = "Prompts/Expressions";

        /// <summary>缓存：表情ID -> 片段内容</summary>
        private static Dictionary<string, string> mExpressionHintCache = new Dictionary<string, string>();

        /// <summary>所有表情ID列表（按加载顺序）</summary>
        private static List<string> mExpressionIds = new List<string>();

        /// <summary>是否已加载</summary>
        private static bool mIsLoaded = false;

        /// <summary>
        /// 加载所有表情片段（如果尚未加载）
        /// </summary>
        public static void EnsureLoaded()
        {
            if (mIsLoaded) return;

            LoadAllExpressionHints();
            mIsLoaded = true;
        }

        /// <summary>
        /// 强制重新加载所有表情片段
        /// </summary>
        public static void Reload()
        {
            mExpressionHintCache.Clear();
            mExpressionIds.Clear();
            mIsLoaded = false;
            EnsureLoaded();
        }

        /// <summary>
        /// 获取指定表情的提示词片段
        /// </summary>
        /// <param name="expressionId">表情ID（如 "happy", "sad"）</param>
        /// <returns>提示词片段内容，如果不存在返回空字符串</returns>
        public static string GetExpressionHint(string expressionId)
        {
            EnsureLoaded();

            if (mExpressionHintCache.TryGetValue(expressionId, out string hint))
            {
                return hint;
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取所有表情的提示词片段，拼接成一个字符串
        /// </summary>
        /// <returns>所有表情片段拼接后的字符串</returns>
        public static string GetAllExpressionHints()
        {
            EnsureLoaded();

            var sb = new StringBuilder();

            foreach (var expressionId in mExpressionIds)
            {
                if (mExpressionHintCache.TryGetValue(expressionId, out string hint) && !string.IsNullOrEmpty(hint))
                {
                    sb.AppendLine($"- {hint}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// 获取指定表情列表的提示词片段，拼接成一个字符串
        /// </summary>
        /// <param name="expressionIds">要包含的表情ID列表</param>
        /// <returns>指定表情片段拼接后的字符串</returns>
        public static string GetExpressionHints(IEnumerable<string> expressionIds)
        {
            EnsureLoaded();

            var sb = new StringBuilder();

            foreach (var expressionId in expressionIds)
            {
                if (mExpressionHintCache.TryGetValue(expressionId, out string hint) && !string.IsNullOrEmpty(hint))
                {
                    sb.AppendLine($"- {hint}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// 获取所有已加载的表情ID
        /// </summary>
        public static IEnumerable<string> GetLoadedExpressionIds()
        {
            EnsureLoaded();
            return mExpressionIds.AsReadOnly();
        }

        /// <summary>
        /// 检查指定表情是否已加载
        /// </summary>
        /// <param name="expressionId">表情ID</param>
        /// <returns>是否存在该表情的 Skill 描述</returns>
        public static bool HasExpression(string expressionId)
        {
            EnsureLoaded();
            return mExpressionHintCache.ContainsKey(expressionId);
        }

        /// <summary>
        /// 从目录加载所有表情片段
        /// </summary>
        private static void LoadAllExpressionHints()
        {
            string expressionsDir = Path.Combine(Application.dataPath, EXPRESSIONS_DIR_REL_PATH);

            if (!Directory.Exists(expressionsDir))
            {
                Debug.LogWarning($"[ExpressionHintLoader] 表情片段目录不存在: {expressionsDir}");
                return;
            }

            // 扫描目录中的所有 .md 文件
            string[] files = Directory.GetFiles(expressionsDir, "*.md");

            foreach (string filePath in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                // 文件名即为表情ID（严格匹配）
                string expressionId = fileName;

                try
                {
                    string content = File.ReadAllText(filePath).Trim();
                    mExpressionHintCache[expressionId] = content;
                    mExpressionIds.Add(expressionId);
                    Debug.Log($"[ExpressionHintLoader] 已加载表情片段: {expressionId}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ExpressionHintLoader] 加载表情片段失败 {fileName}: {ex.Message}");
                }
            }

            Debug.Log($"[ExpressionHintLoader] 共加载 {mExpressionHintCache.Count} 个表情片段");
        }
    }
}
