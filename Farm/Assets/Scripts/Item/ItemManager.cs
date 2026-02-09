using QFramework;
using UnityEngine;

namespace FarmGame.Item
{
    public class ItemManager : MonoSingleton<ItemManager>
    {
        public void Initialize()
        {
            Debug.Log("ItemManager Initialized");
        }
        
        // Factory method (optional for now)
        public ItemEntity CreateItem(int configId, int count)
        {
            return new ItemEntity(configId, count);
        }
    }
}
