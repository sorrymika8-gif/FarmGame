using UnityEngine;
using FarmGame.Movement;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 移动指令执行器
    /// 调用 Movable 组件执行移动
    /// </summary>
    public class MoveExecutor : ICommandExecutor
    {
        public string CommandType => CommandTypes.Move;

        public void Execute(ICommand command, DecisionContext context)
        {
            if (command is not MoveCommand moveCmd)
            {
                Debug.LogError("[MoveExecutor] 收到非MoveCommand类型的指令");
                return;
            }

            // 从上下文中获取 Movable 组件
            if (!context.Extra.TryGetValue("Movable", out var movableObj) || movableObj is not Movable movable)
            {
                Debug.LogError("[MoveExecutor] 上下文中未找到Movable组件，请在Extra中设置 'Movable' 键");
                return;
            }

            // 执行移动
            Vector2 targetPos = new Vector2(moveCmd.TargetX, moveCmd.TargetY);
            movable.MoveTo(targetPos);

            Debug.Log($"[MoveExecutor] 开始移动到 ({moveCmd.TargetX}, {moveCmd.TargetY})");
        }
    }
}
