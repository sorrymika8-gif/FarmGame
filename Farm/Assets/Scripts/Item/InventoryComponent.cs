using System;
using System.Collections.Generic;
using UnityEngine;

namespace FarmGame.Item
{
    /// <summary>
    /// 背包组件
    /// 管理玩家持有的物品
    /// </summary>
    public class InventoryComponent
    {
        // ItemId -> ItemEntity (Simplified: Assuming non-stackable instanced items or stackable simple items)
        // Actually, for stackable items like seeds, we usually just track Count.
        // But since we defined ItemEntity to have a Count, we can store a list of ItemEntities.
        // For simplicity in this farm game, let's map ConfigId -> ItemEntity.
        // If we get more of the same item, we just increase the count of the existing entity.
        private Dictionary<int, ItemEntity> mItems = new Dictionary<int, ItemEntity>();

        /// <summary>
        /// 物品变化事件（添加/移除时触发）
        /// 参数：configId, 变化后的数量（0表示物品被完全移除）
        /// </summary>
        public event Action<int, int> OnItemChanged;

        /// <summary>
        /// 添加物品
        /// </summary>
        /// <param name="configId">物品配置ID</param>
        /// <param name="count">数量</param>
        public void AddItem(int configId, int count = 1)
        {
            if (mItems.TryGetValue(configId, out var item))
            {
                item.Count += count;
            }
            else
            {
                mItems[configId] = new ItemEntity(configId, count);
            }
            Debug.Log($"[Inventory] Added item {configId} x{count}. Current total: {mItems[configId].Count}");
            
            // 触发物品变化事件
            OnItemChanged?.Invoke(configId, mItems[configId].Count);
        }

        /// <summary>
        /// 移除物品
        /// </summary>
        /// <param name="configId">物品配置ID</param>
        /// <param name="count">数量</param>
        /// <returns>是否移除成功</returns>
        public bool RemoveItem(int configId, int count = 1)
        {
            if (mItems.TryGetValue(configId, out var item))
            {
                if (item.Count >= count)
                {
                    item.Count -= count;
                    int remainingCount = item.Count;
                    if (item.Count <= 0)
                    {
                        mItems.Remove(configId);
                    }
                    Debug.Log($"[Inventory] Removed item {configId} x{count}.");
                    
                    // 触发物品变化事件
                    OnItemChanged?.Invoke(configId, remainingCount);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查是否拥有指定数量的物品
        /// </summary>
        /// <param name="configId">物品配置ID</param>
        /// <param name="count">数量</param>
        /// <returns>是否拥有</returns>
        public bool HasItem(int configId, int count = 1)
        {
            return mItems.TryGetValue(configId, out var item) && item.Count >= count;
        }

        /// <summary>
        /// 获取指定物品
        /// </summary>
        /// <param name="configId">物品配置ID</param>
        /// <returns>物品实体，不存在则返回null</returns>
        public ItemEntity GetItem(int configId)
        {
            if (mItems.TryGetValue(configId, out var item)) return item;
            return null;
        }

        /// <summary>
        /// 获取指定物品的数量
        /// </summary>
        /// <param name="configId">物品配置ID</param>
        /// <returns>物品数量，不存在则返回0</returns>
        public int GetItemCount(int configId)
        {
            if (mItems.TryGetValue(configId, out var item))
            {
                return item.Count;
            }
            return 0;
        }
        
        public IEnumerable<ItemEntity> GetAllItems() => mItems.Values;
    }
}
