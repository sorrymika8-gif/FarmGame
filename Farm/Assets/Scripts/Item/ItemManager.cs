using QFramework;
using UnityEngine;
using FarmGame.Item.Handlers;
using FarmGame.Player;

namespace FarmGame.Item
{
    public class ItemManager : MonoSingleton<ItemManager>
    {
        public void Initialize()
        {
            // 注册道具使用处理器
            RegisterItemHandlers();
            
            Debug.Log("[ItemManager] Initialized");
        }

        /// <summary>
        /// 注册所有道具使用处理器
        /// </summary>
        private void RegisterItemHandlers()
        {
            ItemUseRegistry.Clear();
            
            // 注册内置处理器
            ItemUseRegistry.Register(new HealHandler());
            ItemUseRegistry.Register(new EquipHandler());
            
            Debug.Log($"[ItemManager] 已注册 {ItemUseRegistry.HandlerCount} 个处理器");
        }
        
        /// <summary>
        /// 使用道具
        /// </summary>
        /// <param name="player">玩家数据</param>
        /// <param name="item">道具实体</param>
        /// <returns>是否使用成功</returns>
        public bool UseItem(PlayerData player, ItemEntity item)
        {
            return ItemUseRegistry.TryUse(player, item);
        }

        /// <summary>
        /// 检查道具是否可使用
        /// </summary>
        /// <param name="item">道具实体</param>
        /// <returns>是否可使用</returns>
        public bool CanUseItem(ItemEntity item)
        {
            return ItemUseRegistry.CanUse(item);
        }
        
        /// <summary>
        /// 创建道具实体
        /// </summary>
        public ItemEntity CreateItem(int configId, int count)
        {
            return new ItemEntity(configId, count);
        }
    }
}
