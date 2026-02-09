using FarmGame.GameConfig;

namespace FarmGame.Item
{
    /// <summary>
    /// Runtime data for an item.
    /// Can be stored in inventory or dropped in the world.
    /// </summary>
    public class ItemEntity
    {
        public int ConfigId { get; protected set; }
        public int Count { get; set; }
        public string InstanceId { get; private set; }

        public ItemConfig Config => ConfigManager.Instance.GetConfig<ItemConfig>(ConfigId);

        public ItemEntity(int configId, int count)
        {
            ConfigId = configId;
            Count = count;
            InstanceId = System.Guid.NewGuid().ToString();
        }
        
        // For serialization or empty creation
        public ItemEntity() { }
    }
}
