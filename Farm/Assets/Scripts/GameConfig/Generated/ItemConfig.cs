// ==========================================================
// 自动生成，请勿手动修改
// 来源: item.xlsx
// 描述: 道具配置表
// 生成时间: 2026-02-23 02:47:02
// ==========================================================

using System;
using System.Collections.Generic;

namespace FarmGame.GameConfig.Generated
{
    /// <summary>
    /// 道具配置表
    /// </summary>
    [Serializable]
    public class ItemConfig
    {
        /// <summary>道具class_id</summary>
        public int class_id;

        /// <summary>道具名字</summary>
        public string name;

        /// <summary>道具类型</summary>
        public int item_type;

        /// <summary>道具图标</summary>
        public string icon;

        /// <summary>描述</summary>
        public string description;

        /// <summary>使用方式</summary>
        public string use;

        /// <summary>使用参数</summary>
        public Dictionary<string, object> use_arg;
    }
}
