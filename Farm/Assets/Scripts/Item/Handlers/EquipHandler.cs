using System.Collections.Generic;
using FarmGame.Player;
using UnityEngine;

namespace FarmGame.Item.Handlers
{
    /// <summary>
    /// 装备处理器
    /// 配置表 use = "equip"
    /// 支持的 use_arg 参数:
    ///   - slot: string, 装备槽位 ("tool", "accessory" 等)
    /// </summary>
    public class EquipHandler : IItemUseHandler
    {
        public string Name => "equip";

        public bool Execute(PlayerData player, ItemEntity item, Dictionary<string, object> args)
        {
            // 解析参数
            string slot = GetStringArg(args, "slot", "tool");

            Debug.Log($"[EquipHandler] 装备道具: {item.ConfigInfo.Name}, 槽位: {slot}");

            // TODO: 实现具体的装备逻辑
            // 当实现装备系统后，在此处处理：
            // 1. 检查槽位是否可用
            // 2. 如果槽位已有物品，卸下并放回背包
            // 3. 将当前物品装备到指定槽位
            // player.Equipment.Equip(slot, item);

            // 框架阶段：不消耗道具，仅切换状态
            // 装备类道具通常不会从背包消失，所以返回 false
            return false;
        }

        /// <summary>
        /// 从参数字典获取字符串值
        /// </summary>
        private string GetStringArg(Dictionary<string, object> args, string key, string defaultValue)
        {
            if (args == null || !args.TryGetValue(key, out var value))
            {
                return defaultValue;
            }

            return value?.ToString() ?? defaultValue;
        }
    }
}
