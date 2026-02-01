using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 行为提示词片段加载器
    /// 负责从 Assets/Prompts/Actions/ 目录加载各种行为的提示词片段
    /// </summary>
    public static class ActionHintLoader
    {
        // 行为片段目录路径 (相对于 Assets 目录)
        private const string ACTIONS_DIR_REL_PATH = "Prompts/Actions";

        // 缓存：行为类型 -> 片段内容
        private static Dictionary<string, string> mActionHintCache = new Dictionary<string, string>();

        // 是否已加载
        private static bool mIsLoaded = false;

        /// <summary>
        /// 所有支持的行为类型及其对应的文件名
        /// </summary>
        private static readonly Dictionary<string, string> ActionFileNames = new Dictionary<string, string>
        {
            { CommandTypes.Speak, "Speak.md" },
            { CommandTypes.Move, "Move.md" },
            { CommandTypes.Attack, "Attack.md" },
            { CommandTypes.SetState, "SetState.md" },
            { CommandTypes.MemoryOperation, "MemoryOperation.md" }
        };

        /// <summary>
        /// 加载所有行为片段（如果尚未加载）
        /// </summary>
        public static void EnsureLoaded()
        {
            if (mIsLoaded) return;

            LoadAllActionHints();
            mIsLoaded = true;
        }

        /// <summary>
        /// 强制重新加载所有行为片段
        /// </summary>
        public static void Reload()
        {
            mActionHintCache.Clear();
            mIsLoaded = false;
            EnsureLoaded();
        }

        /// <summary>
        /// 获取指定行为的提示词片段
        /// </summary>
        /// <param name="actionType">行为类型（使用 CommandTypes 常量）</param>
        /// <returns>提示词片段内容，如果不存在返回空字符串</returns>
        public static string GetActionHint(string actionType)
        {
            EnsureLoaded();

            if (mActionHintCache.TryGetValue(actionType, out string hint))
            {
                return hint;
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取所有行为的提示词片段，拼接成一个字符串
        /// </summary>
        /// <returns>所有行为片段拼接后的字符串</returns>
        public static string GetAllActionHints()
        {
            EnsureLoaded();

            var sb = new StringBuilder();
            sb.AppendLine("你可以在回复中执行以下一种或多种行为：");
            sb.AppendLine();

            foreach (var actionType in ActionFileNames.Keys)
            {
                if (mActionHintCache.TryGetValue(actionType, out string hint) && !string.IsNullOrEmpty(hint))
                {
                    sb.AppendLine(hint);
                    sb.AppendLine();
                }
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// 获取指定行为列表的提示词片段，拼接成一个字符串
        /// </summary>
        /// <param name="actionTypes">要包含的行为类型列表</param>
        /// <returns>指定行为片段拼接后的字符串</returns>
        public static string GetActionHints(IEnumerable<string> actionTypes)
        {
            EnsureLoaded();

            var sb = new StringBuilder();
            sb.AppendLine("你可以在回复中执行以下一种或多种行为：");
            sb.AppendLine();

            foreach (var actionType in actionTypes)
            {
                if (mActionHintCache.TryGetValue(actionType, out string hint) && !string.IsNullOrEmpty(hint))
                {
                    sb.AppendLine(hint);
                    sb.AppendLine();
                }
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// 获取所有已加载的行为类型
        /// </summary>
        public static IEnumerable<string> GetLoadedActionTypes()
        {
            EnsureLoaded();
            return mActionHintCache.Keys;
        }

        /// <summary>
        /// 从目录加载所有行为片段
        /// </summary>
        private static void LoadAllActionHints()
        {
            string actionsDir = Path.Combine(Application.dataPath, ACTIONS_DIR_REL_PATH);

            if (!Directory.Exists(actionsDir))
            {
                Debug.LogWarning($"[ActionHintLoader] 行为片段目录不存在: {actionsDir}");
                return;
            }

            foreach (var kv in ActionFileNames)
            {
                string actionType = kv.Key;
                string fileName = kv.Value;
                string filePath = Path.Combine(actionsDir, fileName);

                if (File.Exists(filePath))
                {
                    try
                    {
                        string content = File.ReadAllText(filePath);
                        mActionHintCache[actionType] = content;
                        Debug.Log($"[ActionHintLoader] 已加载行为片段: {actionType}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ActionHintLoader] 加载行为片段失败 {fileName}: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[ActionHintLoader] 行为片段文件不存在: {filePath}");
                }
            }

            Debug.Log($"[ActionHintLoader] 共加载 {mActionHintCache.Count} 个行为片段");
        }
    }
}
