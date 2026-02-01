using System;
using System.Collections.Generic;
using UnityEngine;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 聊天决策指令解析器
    /// 将 LLM 返回的 JSON 解析为 Speak 指令
    /// </summary>
    public class ChatCommandParser : ICommandParser
    {
        public string DecisionType => DecisionTypes.Chat;

        public IEnumerable<ICommand> Parse(string llmOutput)
        {
            var commands = new List<ICommand>();

            if (string.IsNullOrWhiteSpace(llmOutput))
            {
                return commands;
            }

            try
            {
                string jsonText = CleanJsonOutput(llmOutput);
                var commandDataList = ParseJsonArray(jsonText);

                foreach (var data in commandDataList)
                {
                    var command = CreateCommand(data);
                    if (command != null)
                    {
                        commands.Add(command);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChatCommandParser] 解析失败: {ex.Message}");
            }

            return commands;
        }

        /// <summary>
        /// 清理JSON输出，移除markdown代码块等
        /// </summary>
        private string CleanJsonOutput(string output)
        {
            string result = output.Trim();

            // 移除markdown代码块标记
            if (result.StartsWith("```json"))
            {
                result = result.Substring(7);
            }
            else if (result.StartsWith("```"))
            {
                result = result.Substring(3);
            }

            if (result.EndsWith("```"))
            {
                result = result.Substring(0, result.Length - 3);
            }

            return result.Trim();
        }

        /// <summary>
        /// 简易JSON数组解析器
        /// </summary>
        private List<ChatCommandData> ParseJsonArray(string jsonArray)
        {
            var results = new List<ChatCommandData>();

            // 找到数组的起始和结束
            int startIndex = jsonArray.IndexOf('[');
            int endIndex = jsonArray.LastIndexOf(']');

            if (startIndex < 0 || endIndex < 0 || endIndex <= startIndex)
            {
                Debug.LogWarning("[ChatCommandParser] 无效的JSON数组格式");
                return results;
            }

            string arrayContent = jsonArray.Substring(startIndex + 1, endIndex - startIndex - 1);

            // 简单的对象分割
            int depth = 0;
            int objectStart = -1;

            for (int i = 0; i < arrayContent.Length; i++)
            {
                char c = arrayContent[i];

                if (c == '{')
                {
                    if (depth == 0)
                    {
                        objectStart = i;
                    }
                    depth++;
                }
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0 && objectStart >= 0)
                    {
                        string objectJson = arrayContent.Substring(objectStart, i - objectStart + 1);
                        var data = JsonUtility.FromJson<ChatCommandData>(objectJson);
                        if (data != null)
                        {
                            results.Add(data);
                        }
                        objectStart = -1;
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// 根据解析的数据创建具体的指令对象
        /// </summary>
        private ICommand CreateCommand(ChatCommandData data)
        {
            if (string.IsNullOrEmpty(data.type))
            {
                Debug.LogWarning("[ChatCommandParser] 指令缺少type字段");
                return null;
            }

            // Chat 解析器主要处理 Speak 指令
            if (data.type == CommandTypes.Speak)
            {
                return new SpeakCommand
                {
                    Content = data.content
                };
            }

            Debug.LogWarning($"[ChatCommandParser] 聊天场景下不支持的指令类型: {data.type}");
            return null;
        }

        /// <summary>
        /// 用于JSON反序列化的中间数据结构
        /// </summary>
        [Serializable]
        private class ChatCommandData
        {
            public string type;
            public string content;
        }
    }
}
