using System.Collections.Generic;
using UnityEngine;

namespace FarmGame.Item
{
    public class InventoryComponent
    {
        // ItemId -> ItemEntity (Simplified: Assuming non-stackable instanced items or stackable simple items)
        // Actually, for stackable items like seeds, we usually just track Count.
        // But since we defined ItemEntity to have a Count, we can store a list of ItemEntities.
        // For simplicity in this farm game, let's map ConfigId -> ItemEntity.
        // If we get more of the same item, we just increase the count of the existing entity.
        private Dictionary<int, ItemEntity> mItems = new Dictionary<int, ItemEntity>();

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
        }

        public bool RemoveItem(int configId, int count = 1)
        {
            if (mItems.TryGetValue(configId, out var item))
            {
                if (item.Count >= count)
                {
                    item.Count -= count;
                    if (item.Count <= 0)
                    {
                        mItems.Remove(configId);
                    }
                    Debug.Log($"[Inventory] Removed item {configId} x{count}.");
                    return true;
                }
            }
            return false;
        }

        public bool HasItem(int configId, int count = 1)
        {
            return mItems.TryGetValue(configId, out var item) && item.Count >= count;
        }

        public ItemEntity GetItem(int configId)
        {
            if (mItems.TryGetValue(configId, out var item)) return item;
            return null;
        }
        
        public IEnumerable<ItemEntity> GetAllItems() => mItems.Values;
    }
}
