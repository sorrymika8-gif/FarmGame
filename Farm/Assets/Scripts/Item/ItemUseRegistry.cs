using System.Collections.Generic;
using FarmGame.Player;
using FarmGame.GameConfig.Generated;
using UnityEngine;

namespace FarmGame.Item
{
    /// <summary>
    /// 道具使用处理器注册表
    /// 管理所有道具使用处理器的注册与调用
    /// </summary>
    public static class ItemUseRegistry
    {
        private static readonly Dictionary<string, IItemUseHandler> mHandlers = new Dictionary<string, IItemUseHandler>();

        /// <summary>
        /// 注册道具使用处理器
        /// </summary>
        /// <param name="handler">处理器实例</param>
        public static void Register(IItemUseHandler handler)
        {
            if (handler == null)
            {
                Debug.LogWarning("[ItemUseRegistry] 尝试注册空处理器");
                return;
            }

            if (string.IsNullOrEmpty(handler.Name))
            {
                Debug.LogWarning("[ItemUseRegistry] 处理器名称为空，跳过注册");
                return;
            }

            if (mHandlers.ContainsKey(handler.Name))
            {
                Debug.LogWarning($"[ItemUseRegistry] 处理器 '{handler.Name}' 已存在，将被覆盖");
            }

            mHandlers[handler.Name] = handler;
            Debug.Log($"[ItemUseRegistry] 注册处理器: {handler.Name}");
        }

        /// <summary>
        /// 注销道具使用处理器
        /// </summary>
        /// <param name="name">处理器名称</param>
        public static void Unregister(string name)
        {
            if (mHandlers.Remove(name))
            {
                Debug.Log($"[ItemUseRegistry] 注销处理器: {name}");
            }
        }

        /// <summary>
        /// 清空所有处理器
        /// </summary>
        public static void Clear()
        {
            mHandlers.Clear();
            Debug.Log("[ItemUseRegistry] 已清空所有处理器");
        }

        /// <summary>
        /// 尝试使用道具
        /// </summary>
        /// <param name="player">玩家数据</param>
        /// <param name="item">道具实体</param>
        /// <returns>是否使用成功</returns>
        public static bool TryUse(PlayerData player, ItemEntity item)
        {
            if (player == null || item == null)
            {
                Debug.LogWarning("[ItemUseRegistry] 玩家或道具为空");
                return false;
            }

            var config = item.Config;
            if (config == null)
            {
                Debug.LogWarning($"[ItemUseRegistry] 道具配置不存在: ConfigId={item.ConfigId}");
                return false;
            }

            // 检查 use 字段是否配置
            if (string.IsNullOrEmpty(config.use))
            {
                Debug.Log($"[ItemUseRegistry] 道具 '{config.name}' 未配置使用处理器");
                return false;
            }

            // 查找处理器
            if (!mHandlers.TryGetValue(config.use, out var handler))
            {
                Debug.LogWarning($"[ItemUseRegistry] 未找到处理器: '{config.use}'");
                return false;
            }

            // 获取参数
            var args = config.use_arg ?? new Dictionary<string, object>();

            Debug.Log($"[ItemUseRegistry] 执行处理器: {config.use}, 道具: {config.name}, 参数: {args.Count}个");

            // 执行处理器
            return handler.Execute(player, item, args);
        }

        /// <summary>
        /// 检查道具是否可使用
        /// </summary>
        /// <param name="item">道具实体</param>
        /// <returns>是否可使用</returns>
        public static bool CanUse(ItemEntity item)
        {
            if (item == null) return false;

            var config = item.Config;
            if (config == null) return false;

            return !string.IsNullOrEmpty(config.use) && mHandlers.ContainsKey(config.use);
        }

        /// <summary>
        /// 获取已注册的处理器数量
        /// </summary>
        public static int HandlerCount => mHandlers.Count;
    }
}
