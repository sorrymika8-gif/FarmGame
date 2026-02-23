using System.Collections.Generic;
using FarmGame.Player;
using UnityEngine;

namespace FarmGame.Item.Handlers
{
    /// <summary>
    /// 治疗/恢复效果处理器
    /// 配置表 use = "heal"
    /// 支持的 use_arg 参数:
    ///   - hp: int, 恢复的生命值/体力值
    ///   - hunger: int, 恢复的饥饿度
    /// </summary>
    public class HealHandler : IItemUseHandler
    {
        public string Name => "heal";

        public bool Execute(PlayerData player, ItemEntity item, Dictionary<string, object> args)
        {
            // 解析参数
            int hp = GetIntArg(args, "hp", 0);
            int hunger = GetIntArg(args, "hunger", 0);

            Debug.Log($"[HealHandler] 使用道具: {item.ConfigInfo.Name}, 恢复体力: {hp}, 恢复饥饿: {hunger}");

            // TODO: 实现具体的恢复逻辑
            // 当 PlayerData 扩展了 Stamina/Hunger 属性后，在此处实现：
            // if (hp > 0) player.Stamina = Mathf.Min(player.Stamina + hp, player.MaxStamina);
            // if (hunger > 0) player.Hunger = Mathf.Min(player.Hunger + hunger, player.MaxHunger);

            // 框架阶段：直接返回成功，表示道具可以被消耗
            return true;
        }

        /// <summary>
        /// 从参数字典获取整数值
        /// </summary>
        private int GetIntArg(Dictionary<string, object> args, string key, int defaultValue)
        {
            if (args == null || !args.TryGetValue(key, out var value))
            {
                return defaultValue;
            }

            // MiniJSON 解析数字可能是 int、long 或 double
            if (value is int intVal) return intVal;
            if (value is long longVal) return (int)longVal;
            if (value is double doubleVal) return (int)doubleVal;
            if (value is string strVal && int.TryParse(strVal, out var parsed)) return parsed;

            return defaultValue;
        }
    }
}
