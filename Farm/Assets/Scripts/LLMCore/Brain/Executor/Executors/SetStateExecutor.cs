using System;
using UnityEngine;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 设置状态指令执行器
    /// 修改角色的状态属性
    /// </summary>
    public class SetStateExecutor : ICommandExecutor
    {
        public string CommandType => CommandTypes.SetState;

        /// <summary>
        /// 状态变更事件，外部可订阅此事件来处理状态变更
        /// 参数: (角色GameObject, 状态Key, 状态Value)
        /// </summary>
        public static event Action<GameObject, string, string> OnStateChanged;

        public void Execute(ICommand command, DecisionContext context)
        {
            if (command is not SetStateCommand stateCmd)
            {
                Debug.LogError("[SetStateExecutor] 收到非SetStateCommand类型的指令");
                return;
            }

            if (string.IsNullOrEmpty(stateCmd.Key))
            {
                Debug.LogWarning("[SetStateExecutor] 状态Key为空");
                return;
            }

            // 直接更新上下文中的状态（如果需要持久化，外部需要订阅事件处理）
            if (context.CurrentState != null)
            {
                context.CurrentState[stateCmd.Key] = stateCmd.Value;
            }

            // 从上下文获取角色的GameObject（可选）
            GameObject character = null;
            if (context.Extra.TryGetValue("GameObject", out var goObj) && goObj is GameObject go)
            {
                character = go;
            }

            // 触发状态变更事件
            OnStateChanged?.Invoke(character, stateCmd.Key, stateCmd.Value);

            string charName = character != null ? character.name : "Unknown";
            Debug.Log($"[SetStateExecutor] {charName} 状态变更: {stateCmd.Key} = {stateCmd.Value}");
        }
    }
}
