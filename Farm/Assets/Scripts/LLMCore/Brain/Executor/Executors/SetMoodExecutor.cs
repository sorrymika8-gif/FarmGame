using System;
using UnityEngine;
using FarmGame.Game.NPC;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 设置心情指令执行器
    /// 设置NPC的当前心情emoji，用于气泡对话显示
    /// </summary>
    public class SetMoodExecutor : ICommandExecutor
    {
        public string CommandType => CommandTypes.SetMood;

        /// <summary>
        /// 心情变更事件，外部可订阅此事件来处理心情变化
        /// 参数: (NPCEntity, 新emoji)
        /// </summary>
        public static event Action<NPCEntity, string> OnMoodChanged;

        public void Execute(ICommand command, DecisionContext context)
        {
            if (command is not SetMoodCommand moodCmd)
            {
                Debug.LogError("[SetMoodExecutor] 收到非SetMoodCommand类型的指令");
                return;
            }

            if (string.IsNullOrEmpty(moodCmd.Emoji))
            {
                Debug.LogWarning("[SetMoodExecutor] Emoji为空");
                return;
            }

            // 从上下文获取NPCEntity
            NPCEntity entity = null;
            if (context.Extra.TryGetValue("NPCEntity", out var entityObj) && entityObj is NPCEntity npc)
            {
                entity = npc;
            }

            if (entity == null)
            {
                Debug.LogWarning("[SetMoodExecutor] 无法从上下文获取NPCEntity");
                return;
            }

            // 更新NPCEntity的心情状态
            string oldMood = entity.CurrentMood;
            entity.SetMood(moodCmd.Emoji);

            // 触发心情变更事件
            OnMoodChanged?.Invoke(entity, moodCmd.Emoji);

            Debug.Log($"[SetMoodExecutor] {entity.Name} 心情变更: {oldMood} -> {moodCmd.Emoji}");
        }
    }
}
