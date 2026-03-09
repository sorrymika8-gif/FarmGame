// ==========================================================
// 自动生成，请勿手动修改
// 来源: llm_settings.xlsx
// 描述: LLM服务配置表
// 生成时间: 2026-03-02 23:51:09
// ==========================================================

using System;
using System.Collections.Generic;

namespace FarmGame.GameConfig.Generated
{
    /// <summary>
    /// LLM服务配置表
    /// </summary>
    [Serializable]
    public class LlmSettingsConfig
    {
        /// <summary>配置id</summary>
        public int setting_id;

        /// <summary>提供商类型</summary>
        public string provider_type;

        /// <summary>API密钥</summary>
        public string api_key;

        /// <summary>基础URL</summary>
        public string base_url;

        /// <summary>默认模型</summary>
        public string default_model;

        /// <summary>是否启用</summary>
        public bool enabled;

        /// <summary>描述</summary>
        public string description;
    }
}
