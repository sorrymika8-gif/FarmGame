using System;
using UnityEngine;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 攻击指令执行器
    /// 触发攻击逻辑
    /// </summary>
    public class AttackExecutor : ICommandExecutor
    {
        public string CommandType => CommandTypes.Attack;

        /// <summary>
        /// 攻击事件，外部可订阅此事件来处理攻击逻辑
        /// 参数: (攻击者GameObject, 目标ID)
        /// </summary>
        public static event Action<GameObject, string> OnAttack;

        public void Execute(ICommand command, DecisionContext context)
        {
            if (command is not AttackCommand attackCmd)
            {
                Debug.LogError("[AttackExecutor] 收到非AttackCommand类型的指令");
                return;
            }

            if (string.IsNullOrEmpty(attackCmd.TargetId))
            {
                Debug.LogWarning("[AttackExecutor] 攻击目标ID为空");
                return;
            }

            // 从上下文获取攻击者的GameObject（可选）
            GameObject attacker = null;
            if (context.Extra.TryGetValue("GameObject", out var goObj) && goObj is GameObject go)
            {
                attacker = go;
            }

            // 触发攻击事件
            OnAttack?.Invoke(attacker, attackCmd.TargetId);

            string attackerName = attacker != null ? attacker.name : "Unknown";
            Debug.Log($"[AttackExecutor] {attackerName} 攻击目标: {attackCmd.TargetId}");
        }
    }
}
