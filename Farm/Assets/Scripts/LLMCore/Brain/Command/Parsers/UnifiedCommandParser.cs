using System;
using System.Collections.Generic;
using UnityEngine;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 统一指令解析器
    /// 将 LLM 返回的 JSON 解析为指令列表
    /// 支持所有类型的指令（Speak, Move, Attack, SetState, MemoryOperation）
    /// </summary>
    public class UnifiedCommandParser : ICommandParser
    {
        public string DecisionType => DecisionTypes.Unified;

        public IEnumerable<ICommand> Parse(string llmOutput)
        {
            var commands = new List<ICommand>();

            if (string.IsNullOrWhiteSpace(llmOutput))
            {
                Debug.LogWarning("[UnifiedCommandParser] LLM输出为空");
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
                Debug.LogError($"[UnifiedCommandParser] 解析失败: {ex.Message}\n原始输出: {llmOutput}");
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
        private List<CommandData> ParseJsonArray(string jsonArray)
        {
            var results = new List<CommandData>();

            // 找到数组的起始和结束
            int startIndex = jsonArray.IndexOf('[');
            int endIndex = jsonArray.LastIndexOf(']');

            if (startIndex < 0 || endIndex < 0 || endIndex <= startIndex)
            {
                Debug.LogWarning("[UnifiedCommandParser] 无效的JSON数组格式");
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
                Debug.LogWarning("[UnifiedCommandParser] 指令缺少type字段");
                return null;
            }

            switch (data.type)
            {
                case CommandTypes.Speak:
                    return new SpeakCommand
                    {
                        Content = data.content
                    };

                case CommandTypes.Move:
                    return new MoveCommand
                    {
                        TargetX = data.x,
                        TargetY = data.y
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

                case CommandTypes.Till:
                    return new TillCommand
                    {
                        X = Mathf.RoundToInt(data.x),
                        Y = Mathf.RoundToInt(data.y)
                    };

                case CommandTypes.Plant:
                    return new PlantCommand
                    {
                        X = Mathf.RoundToInt(data.x),
                        Y = Mathf.RoundToInt(data.y),
                        ItemId = data.itemId
                    };

                case CommandTypes.Harvest:
                    return new HarvestCommand
                    {
                        X = Mathf.RoundToInt(data.x),
                        Y = Mathf.RoundToInt(data.y)
                    };

                case CommandTypes.SetExpression:
                    return new SetExpressionCommand
                    {
                        Expression = data.expression
                    };

                case CommandTypes.SetMood:
                    return new SetMoodCommand
                    {
                        Emoji = data.emoji
                    };

                default:
                    Debug.LogWarning($"[UnifiedCommandParser] 未知的指令类型: {data.type}");
                    return null;
            }
        }

        /// <summary>
        /// 用于JSON反序列化的中间数据结构
        /// 包含所有可能的指令字段
        /// </summary>
        [Serializable]
        private class CommandData
        {
            public string type;

            // Speak / MemoryOperation
            public string content;

            // Move
            public float x;
            public float y;

            // Attack
            public string targetId;

            // SetState
            public string key;
            public string value;

            // MemoryOperation
            public string operation;
            public string partition;

            // Plant
             public int itemId;

            // SetExpression
            public string expression;

            // SetMood
            public string emoji;
        }
    }
}
