using System;
using System.Collections.Generic;
using UnityEngine;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 行为决策指令解析器
    /// 将 LLM 返回的 JSON 解析为指令对象列表
    /// </summary>
    public class BehaviorCommandParser : ICommandParser
    {
        public string DecisionType => DecisionTypes.Behavior;

        public IEnumerable<ICommand> Parse(string llmOutput)
        {
            var commands = new List<ICommand>();

            if (string.IsNullOrWhiteSpace(llmOutput))
            {
                Debug.LogWarning("[BehaviorCommandParser] LLM输出为空");
                return commands;
            }

            try
            {
                // 清理输出（移除可能的markdown代码块标记）
                string jsonText = CleanJsonOutput(llmOutput);

                // 使用Unity内置的JsonUtility需要包装数组
                // 这里我们手动解析简单的JSON数组
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
                Debug.LogError($"[BehaviorCommandParser] 解析失败: {ex.Message}\n原始输出: {llmOutput}");
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
        /// 使用Unity的JsonUtility解析每个对象
        /// </summary>
        private List<CommandData> ParseJsonArray(string jsonArray)
        {
            var results = new List<CommandData>();

            // 找到数组的起始和结束
            int startIndex = jsonArray.IndexOf('[');
            int endIndex = jsonArray.LastIndexOf(']');

            if (startIndex < 0 || endIndex < 0 || endIndex <= startIndex)
            {
                Debug.LogWarning("[BehaviorCommandParser] 无效的JSON数组格式");
                return results;
            }

            string arrayContent = jsonArray.Substring(startIndex + 1, endIndex - startIndex - 1);

            // 简单的对象分割（假设对象内没有嵌套的大括号）
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
                        var data = JsonUtility.FromJson<CommandData>(objectJson);
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
        private ICommand CreateCommand(CommandData data)
        {
            if (string.IsNullOrEmpty(data.type))
            {
                Debug.LogWarning("[BehaviorCommandParser] 指令缺少type字段");
                return null;
            }

            switch (data.type)
            {
                case CommandTypes.Move:
                    return new MoveCommand
                    {
                        TargetX = data.x,
                        TargetY = data.y
                    };

                case CommandTypes.Speak:
                    return new SpeakCommand
                    {
                        Content = data.content
                    };

                case CommandTypes.Attack:
                    return new AttackCommand
                    {
                        TargetId = data.targetId
                    };

                case CommandTypes.SetState:
                    return new SetStateCommand
                    {
                        Key = data.key,
                        Value = data.value
                    };

                case CommandTypes.MemoryOperation:
                    return new MemoryOperationCommand
                    {
                        Operation = data.operation,
                        Partition = data.partition,
                        Content = data.content
                    };

                default:
                    Debug.LogWarning($"[BehaviorCommandParser] 未知的指令类型: {data.type}");
                    return null;
            }
        }

        /// <summary>
        /// 用于JSON反序列化的中间数据结构
        /// </summary>
        [Serializable]
        private class CommandData
        {
            public string type;

            // Move
            public float x;
            public float y;

            // Speak / MemoryOperation
            public string content;

            // Attack
            public string targetId;

            // SetState
            public string key;
            public string value;

            // MemoryOperation
            public string operation;
            public string partition;
        }
    }
}
