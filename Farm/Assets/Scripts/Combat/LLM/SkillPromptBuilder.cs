using System.Collections.Generic;
using System.Text;
using FarmGame.Combat.Data;

namespace FarmGame.Combat.LLM
{
    /// <summary>
    /// 技能 Prompt 构建器 - 从材料构建 LLM 请求 Prompt
    /// </summary>
    public static class SkillPromptBuilder
    {
        #region Prompt 模板

        private const string SYSTEM_PROMPT = @"你是一个游戏技能设计师。根据玩家提供的材料，生成一个创意且平衡的技能。

## 技能原子字段说明

### 弹道行为
- bounce (int 0-5): 弹射次数，命中后弹向下一个敌人
- tracking (float 0-360): 追踪角度，0=直飞，360=全向追踪
- pierce (int 0-10): 穿透目标数，0=命中即消失
- split (int 0-5): 分裂数量，命中时分裂出子投射物
- returning (bool): 是否像回旋镖一样返回释放者
- attract (float -10~10): 正=吸引敌人，负=排斥敌人

### 范围规则
- aoeRadius (float 0-8): AOE半径，0=单体
- shape (string): Point/Circle/Fan/Line
- projectileWidth (float 0-3): 弹道宽度

### 数值效果
- directHP (float -500~500): 正=治疗，负=伤害
- dotHP (float -50~50): 每秒持续生命变化
- moveSpeedMod (float -100~100): 移速百分比变化
- attackMod (float -200~200): 攻击力百分比变化
- defenseMod (float -100~100): 防御百分比变化

### 状态效果
- slowPercent (float 0-90): 减速百分比
- silenceDuration (float 0-10): 沉默秒数
- damageMultiplier (float 0-3): 易伤倍率，>1=易伤，<1=减伤
- stealthDuration (float 0-15): 隐身秒数

### 触发与目标
- trigger (string): Immediate/OnHit/HPThreshold/Interval/OnKill
- target (string): Self/SingleEnemy/Area/AllEnemies/Nearest

### 时间参数
- delay (float 0-5): 延迟释放秒数
- duration (float 0-30): 效果持续时间
- cooldown (float 0.1-60): 冷却时间

### 元信息
- displayName (string): 技能名称
- projectileSpeed (float 1-30): 飞行速度

## 输出格式
严格输出 JSON，不要markdown代码块，不要额外解释：
{""displayName"":""技能名"",""bounce"":0,...}";

        private const string USER_PROMPT_TEMPLATE = @"玩家使用以下材料合成技能：

{MATERIALS}

请根据材料的属性和特征，设计一个有创意的技能。材料的属性应该影响技能效果：
- 攻击性材料 → 增加伤害、穿透、弹射
- 防御性材料 → 治疗、护盾、减伤
- 速度材料 → 追踪、分裂、移速效果
- 控制材料 → 减速、沉默、吸引

只输出 JSON。";

        #endregion

        #region 公共方法

        /// <summary>
        /// 构建系统 Prompt
        /// </summary>
        /// <returns>系统 Prompt</returns>
        public static string BuildSystemPrompt()
        {
            return SYSTEM_PROMPT;
        }

        /// <summary>
        /// 构建用户 Prompt
        /// </summary>
        /// <param name="materials">材料列表</param>
        /// <returns>用户 Prompt</returns>
        public static string BuildUserPrompt(List<MaterialData> materials)
        {
            var sb = new StringBuilder();

            if (materials == null || materials.Count == 0)
            {
                sb.AppendLine("无特殊材料，生成一个基础攻击技能。");
            }
            else
            {
                foreach (var material in materials)
                {
                    sb.AppendLine($"- {material.Name}");
                    if (!string.IsNullOrEmpty(material.Description))
                    {
                        sb.AppendLine($"  描述: {material.Description}");
                    }
                    if (!string.IsNullOrEmpty(material.Attributes))
                    {
                        sb.AppendLine($"  属性: {material.Attributes}");
                    }
                }
            }

            return USER_PROMPT_TEMPLATE.Replace("{MATERIALS}", sb.ToString());
        }

        /// <summary>
        /// 构建完整 Prompt（用于调试）
        /// </summary>
        /// <param name="materials">材料列表</param>
        /// <returns>完整 Prompt 字符串</returns>
        public static string BuildFullPrompt(List<MaterialData> materials)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== SYSTEM ===");
            sb.AppendLine(BuildSystemPrompt());
            sb.AppendLine();
            sb.AppendLine("=== USER ===");
            sb.AppendLine(BuildUserPrompt(materials));
            return sb.ToString();
        }

        #endregion
    }

    /// <summary>
    /// 材料数据 - 用于技能合成
    /// </summary>
    public class MaterialData
    {
        /// <summary>材料名称</summary>
        public string Name { get; set; }

        /// <summary>材料描述</summary>
        public string Description { get; set; }

        /// <summary>材料属性（如：攻击+10，火属性）</summary>
        public string Attributes { get; set; }

        /// <summary>材料品质（1-5）</summary>
        public int Quality { get; set; } = 1;

        /// <summary>材料类型</summary>
        public MaterialType Type { get; set; } = MaterialType.Normal;

        public MaterialData() { }

        public MaterialData(string name, string description = null, string attributes = null)
        {
            Name = name;
            Description = description;
            Attributes = attributes;
        }
    }

    /// <summary>
    /// 材料类型
    /// </summary>
    public enum MaterialType
    {
        /// <summary>普通材料</summary>
        Normal,
        /// <summary>攻击型材料</summary>
        Offensive,
        /// <summary>防御型材料</summary>
        Defensive,
        /// <summary>速度型材料</summary>
        Speed,
        /// <summary>控制型材料</summary>
        Control,
        /// <summary>稀有材料</summary>
        Rare
    }
}
