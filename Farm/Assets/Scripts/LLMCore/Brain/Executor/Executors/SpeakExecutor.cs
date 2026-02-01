using System;
using UnityEngine;
using FarmGame.Game.NPC;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 说话指令执行器
    /// 触发对话气泡或UI显示
    /// </summary>
    public class SpeakExecutor : ICommandExecutor
    {
        public string CommandType => CommandTypes.Speak;

        /// <summary>
        /// 说话事件，外部可订阅此事件来显示对话UI
        /// 参数: (说话者GameObject, 说话内容)
        /// </summary>
        public static event Action<GameObject, string> OnSpeak;

        public void Execute(ICommand command, DecisionContext context)
        {
            if (command is not SpeakCommand speakCmd)
            {
                Debug.LogError("[SpeakExecutor] 收到非SpeakCommand类型的指令");
                return;
            }

            if (string.IsNullOrEmpty(speakCmd.Content))
            {
                Debug.LogWarning("[SpeakExecutor] 说话内容为空");
                return;
            }

            // 从上下文获取说话者的GameObject（可选）
            GameObject speaker = null;
            if (context.Extra.TryGetValue("GameObject", out var goObj) && goObj is GameObject go)
            {
                speaker = go;
            }

            // 触发说话事件
            OnSpeak?.Invoke(speaker, speakCmd.Content);

            // 获取 NPC 名字并记录行为到短期记忆
            string speakerName = "Unknown";
            if (context.Extra.TryGetValue("NPCEntity", out var entityObj) && entityObj is NPCEntity npc)
            {
                speakerName = npc.Name;
                npc.RecordMemory($"我说：{speakCmd.Content}");
            }

            Debug.Log($"[{speakerName}] {speakCmd.Content}");
        }
    }
}
