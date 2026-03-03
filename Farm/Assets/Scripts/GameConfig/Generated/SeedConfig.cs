// ==========================================================
// 自动生成，请勿手动修改
// 来源: seed.xlsx
// 描述: 植物种子配置表
// 生成时间: 2026-03-02 23:51:09
// ==========================================================

using System;
using System.Collections.Generic;

namespace FarmGame.GameConfig.Generated
{
    /// <summary>
    /// 植物种子配置表
    /// </summary>
    [Serializable]
    public class SeedConfig
    {
        /// <summary>植物种子id</summary>
        public int class_id;

        /// <summary>植物名称</summary>
        public string name;

        /// <summary>道具类型</summary>
        public int item_type;

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

        /// <summary>售价</summary>
        public int sell_price;

        /// <summary>图标</summary>
        public string icon;
    }
}
