using System;
using UnityEngine;
using FarmGame.Game.NPC;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 说话模式
    /// </summary>
    public enum SpeakMode
    {
        /// <summary>气泡对话（日常自言自语）</summary>
        Bubble,
        /// <summary>对话框（正式对话）</summary>
        Dialogue
    }

    /// <summary>
    /// 说话指令执行器
    /// 根据上下文区分气泡对话和正式对话框显示
    /// </summary>
    public class SpeakExecutor : ICommandExecutor
    {
        public string CommandType => CommandTypes.Speak;

        /// <summary>
        /// 气泡说话事件，用于日常自言自语
        /// 参数: (说话者GameObject, 说话内容, 心情emoji)
        /// </summary>
        public static event Action<GameObject, string, string> OnBubbleSpeak;

        /// <summary>
        /// 对话框说话事件，用于正式对话
        /// 参数: (NPCEntity, 说话内容)
        /// </summary>
        public static event Action<NPCEntity, string> OnDialogueSpeak;

        /// <summary>
        /// 通用说话事件（兼容旧代码）
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

            // 获取NPCEntity
            NPCEntity npcEntity = null;
            if (context.Extra.TryGetValue("NPCEntity", out var entityObj) && entityObj is NPCEntity npc)
            {
                npcEntity = npc;
            }

            // 从上下文获取说话者的GameObject
            GameObject speaker = null;
            if (context.Extra.TryGetValue("GameObject", out var goObj) && goObj is GameObject go)
            {
                speaker = go;
            }
            else if (npcEntity != null)
            {
                var controller = NPCManager.Instance?.GetController(npcEntity.Id);
                if (controller != null)
                {
                    speaker = controller.gameObject;
                }
            }

            // 根据TriggerEvent判断说话模式
            SpeakMode mode = DetermineSpeakMode(context);

            if (mode == SpeakMode.Dialogue && npcEntity != null)
            {
                // 正式对话模式：触发对话框事件
                OnDialogueSpeak?.Invoke(npcEntity, speakCmd.Content);
            }
            else
            {
                // 气泡模式：触发气泡事件，附带心情emoji
                string mood = npcEntity?.CurrentMood ?? "";
                OnBubbleSpeak?.Invoke(speaker, speakCmd.Content, mood);
                
                // 气泡显示后清除心情（一次性使用）
                npcEntity?.ClearMood();
            }

            // 触发通用事件（兼容）
            OnSpeak?.Invoke(speaker, speakCmd.Content);

            // 记录行为到短期记忆
            string speakerName = npcEntity?.Name ?? "Unknown";
            npcEntity?.RecordMemory($"我说：{speakCmd.Content}");

            Debug.Log($"[{speakerName}] ({mode}) {speakCmd.Content}");
        }

        /// <summary>
        /// 根据上下文判断说话模式
        /// </summary>
        private SpeakMode DetermineSpeakMode(DecisionContext context)
        {
            // 根据TriggerEvent判断
            string triggerEvent = context.TriggerEvent ?? "";
            
            // 玩家主动对话相关的触发事件 -> 对话框模式
            if (triggerEvent.Contains("Chat") || 
                triggerEvent.Contains("Dialogue") ||
                triggerEvent.Contains("Talk") ||
                triggerEvent == "ReceiveChat" ||
                triggerEvent == "PlayerInteract")
            {
                return SpeakMode.Dialogue;
            }

            // 其他情况（Idle、Behavior、自主行为等）-> 气泡模式
            return SpeakMode.Bubble;
        }
    }
}
