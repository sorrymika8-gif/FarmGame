// ==========================================================
// 自动生成，请勿手动修改
// 来源: npc.xlsx
// 描述: npc配置
// 生成时间: 2026-02-25 22:23:12
// ==========================================================

using System;
using System.Collections.Generic;

namespace FarmGame.GameConfig.Generated
{
    /// <summary>
    /// npc配置
    /// </summary>
    [Serializable]
    public class NpcConfig
    {
        /// <summary>npcid</summary>
        public int class_id;

        /// <summary>npc名字</summary>
        public string name;

        /// <summary>npc性别</summary>
        public string gender;

        /// <summary>npc描述</summary>
        public string desc;

        /// <summary>npc性格</summary>
        public string character;

        /// <summary>npc背景</summary>
        public string background;

        /// <summary>npc补充提示词</summary>
        public string extra_prompt;

        /// <summary>模型名</summary>
        public string model_name;

        /// <summary>初始位置[x,y]</summary>
        public int[] init_pos;

        /// <summary>交互距离</summary>
        public int interaction_dis;
    }
}
