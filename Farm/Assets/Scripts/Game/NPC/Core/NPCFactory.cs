using System;
using UnityEngine;
using FarmGame.LLMCore.Brain;

namespace FarmGame.Game.NPC
{
    /// <summary>
    /// NPC 配置数据 (对应配置表)
    /// </summary>
    [Serializable]
    public class NPCConfig
    {
        public string Id;
        public string Name;
        public string Gender;
        public string Personality;
        public string Background;
        public string Appearance;
        public string InitialRoomId;
        public Vector3 InitialPosition;
        public string[] InitialMemories;
        // 分区配置可以后续扩展，暂时使用默认值
        public PartitionConfig[] PartitionConfigs;
    }

    /// <summary>
    /// NPC 工厂
    /// </summary>
    public static class NPCFactory
    {
        public static NPCEntity Create(NPCConfig config, Brain brain)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (brain == null) throw new ArgumentNullException(nameof(brain));

            var entity = new NPCEntity(
                id: config.Id,
                name: config.Name,
                executorRegistry: brain.ExecutorRegistry
            );

            // 设置属性
            entity.Gender = config.Gender;
            entity.Personality = config.Personality;
            entity.Background = config.Background;
            entity.Appearance = config.Appearance;
            entity.RoomId = config.InitialRoomId;
            entity.Position = config.InitialPosition;
            
            // 初始化记忆
            // 注意: NPCEntity 内部现在使用默认的 MemoryStore 创建方式
            // 如果需要特定 PartitionConfig，需要扩展 NPCEntity 的初始化能力或者手动配置
            if (config.InitialMemories != null)
            {
                entity.InitializeMemories(config.InitialMemories);
            }

            return entity;
        }
    }
}
