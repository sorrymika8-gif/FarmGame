// ==========================================================
// 自动生成，请勿手动修改
// 来源: plant.xlsx
// 描述: 植物配置表
// 生成时间: 2026-02-09 21:57:37
// ==========================================================

using System;
using System.Collections.Generic;

namespace FarmGame.GameConfig.Generated
{
    /// <summary>
    /// 植物配置表
    /// </summary>
    [Serializable]
    public class PlantConfig
    {
        /// <summary>植物id</summary>
        public int class_id;

        /// <summary>植物名称</summary>
        public string name;

        /// <summary>植物成熟度</summary>
        public float need_maturity;

        /// <summary>植物生长速度</summary>
        public float maturity_speed;

        /// <summary>到达各个生长阶段所需的成熟度</summary>
        public int[] maturity_stage;

        /// <summary>成熟后收获的作物</summary>
        public int[] bonus_item;

        /// <summary>收获数量</summary>
        public int[] bonus_amount;
    }
}
