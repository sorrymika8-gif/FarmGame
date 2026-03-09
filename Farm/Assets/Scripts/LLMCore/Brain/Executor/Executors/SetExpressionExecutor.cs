using System;
using UnityEngine;
using FarmGame.Game.NPC;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 设置表情指令执行器
    /// 修改NPC的表情/立绘显示
    /// </summary>
    public class SetExpressionExecutor : ICommandExecutor
    {
        public string CommandType => CommandTypes.SetExpression;

        /// <summary>
        /// 表情变更事件，外部可订阅此事件来处理表情切换
        /// 参数: (NPCEntity, 新表情ID)
        /// </summary>
        public static event Action<NPCEntity, string> OnExpressionChanged;

        public void Execute(ICommand command, DecisionContext context)
        {
            if (command is not SetExpressionCommand expressionCmd)
            {
                Debug.LogError("[SetExpressionExecutor] 收到非SetExpressionCommand类型的指令");
                return;
            }

            if (string.IsNullOrEmpty(expressionCmd.Expression))
            {
                Debug.LogWarning("[SetExpressionExecutor] 表情ID为空");
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
                Debug.LogWarning("[SetExpressionExecutor] 无法从上下文获取NPCEntity");
                return;
            }

            // 检查表情是否可用
            string npcModelName = entity.Id; // 或者使用其他标识符
            if (!PortraitService.Instance.HasExpression(npcModelName, expressionCmd.Expression))
            {
                Debug.LogWarning($"[SetExpressionExecutor] NPC '{entity.Name}' 不支持表情 '{expressionCmd.Expression}'，将使用当前表情");
                return;
            }

            // 更新NPCEntity的表情状态
            string oldExpression = entity.CurrentExpression;
            entity.SetExpression(expressionCmd.Expression);

            // 触发表情变更事件
            OnExpressionChanged?.Invoke(entity, expressionCmd.Expression);

            Debug.Log($"[SetExpressionExecutor] {entity.Name} 表情变更: {oldExpression} -> {expressionCmd.Expression}");
        }
    }
}
