using System;
using UnityEngine;
using FarmGame.LLMCore.Brain;
using FarmGame.GameConfig.Generated;
using FarmGame.Map;

namespace FarmGame.Game.NPC
{
    /// <summary>
    /// NPC 工厂
    /// 使用 NpcConfig (从配置表生成) 创建 NPC 实体
    /// </summary>
    public static class NPCFactory
    {
        /// <summary>
        /// 从配置表创建 NPC 实体
        /// </summary>
        /// <param name="config">NPC配置 (来自npc.xlsx)</param>
        /// <param name="brain">Brain实例</param>
        /// <returns>创建的NPC实体</returns>
        public static NPCEntity Create(NpcConfig config, Brain brain)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (brain == null) throw new ArgumentNullException(nameof(brain));

            var entity = new NPCEntity(
                id: config.class_id.ToString(),
                name: config.name,
                executorRegistry: brain.ExecutorRegistry
            );

            // 设置属性
            entity.Gender = config.gender ?? "未知";
            
            // 设置初始位置
            if (config.init_pos != null && config.init_pos.Length >= 2)
            {
                entity.Position = MapManager.Instance.GridToWorld(config.init_pos[0], config.init_pos[1]);
            }
            
            // 设置交互距离
            entity.InteractionDistance = config.interaction_dis > 0 ? config.interaction_dis : 2f;
            
            // 设置提示词文件路径
            entity.PromptFilePath = config.prompt;

            return entity;
        }
    }
}
