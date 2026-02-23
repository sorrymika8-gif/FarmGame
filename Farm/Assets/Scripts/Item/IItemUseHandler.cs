using System.Collections.Generic;
using FarmGame.Player;

namespace FarmGame.Item
{
    /// <summary>
    /// 道具使用处理器接口
    /// 实现此接口以定义不同类型道具的使用行为
    /// </summary>
    public interface IItemUseHandler
    {
        /// <summary>
        /// 处理器名称，对应配置表中的 use 字段
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 执行道具使用逻辑
        /// </summary>
        /// <param name="player">玩家数据</param>
        /// <param name="item">道具实体</param>
        /// <param name="args">配置表中的 use_arg 参数</param>
        /// <returns>是否使用成功（成功时应消耗道具）</returns>
        bool Execute(PlayerData player, ItemEntity item, Dictionary<string, object> args);
    }
}
