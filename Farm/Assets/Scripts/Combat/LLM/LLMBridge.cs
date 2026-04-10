using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using FarmGame.GameLLM;
using FarmGame.Combat.Data;

namespace FarmGame.Combat.LLM
{
    /// <summary>
    /// LLM 桥接层 - 调用 LLM 生成技能数据
    /// </summary>
    public class LLMBridge
    {
        #region 单例

        private static LLMBridge mInstance;

        /// <summary>单例实例</summary>
        public static LLMBridge Instance
        {
            get
            {
                mInstance ??= new LLMBridge();
                return mInstance;
            }
        }

        #endregion

        #region 私有字段

        private bool mIsInitialized;

        #endregion

        #region 构造函数

        private LLMBridge() { }

        #endregion

        #region 公共方法

        /// <summary>
        /// 初始化 LLM 桥接
        /// </summary>
        public void Initialize()
        {
            if (mIsInitialized) return;

            // 验证 LLMService 是否可用
            if (LLMService.Client == null)
            {
                Debug.LogWarning("[LLMBridge] LLMService.Client is null, skill synthesis may not work");
            }

            mIsInitialized = true;
            Debug.Log("[LLMBridge] Initialized");
        }

        /// <summary>
        /// 合成技能
        /// </summary>
        /// <param name="materials">材料列表</param>
        /// <param name="ct">取消令牌</param>
        /// <returns>技能原子数据</returns>
        public async UniTask<SkillAtomData> SynthesizeAsync(
            List<MaterialData> materials,
            CancellationToken ct = default)
        {
            if (LLMService.Client == null)
            {
                Debug.LogError("[LLMBridge] LLMService.Client is null");
                return CreateFallbackSkill(materials);
            }

            try
            {
                // 构建请求
                var request = new LLMRequest()
                    .AddSystem(SkillPromptBuilder.BuildSystemPrompt())
                    .AddUser(SkillPromptBuilder.BuildUserPrompt(materials));

                request.Temperature = 0.8f;  // 稍高温度以获得更多创意
                request.MaxTokens = 512;

                // 调用 LLM
                var (success, data, error) = await LLMService.Client.SendAsync<SkillAtomData>(request, ct);

                if (!success || data == null)
                {
                    Debug.LogWarning($"[LLMBridge] LLM synthesis failed: {error}");
                    return CreateFallbackSkill(materials);
                }

                // 应用安全阀
                ClampValues(data);

                Debug.Log($"[LLMBridge] Synthesized skill: {data.displayName}");
                return data;
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[LLMBridge] Synthesis cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LLMBridge] Synthesis error: {ex.Message}");
                return CreateFallbackSkill(materials);
            }
        }

        /// <summary>
        /// 批量合成技能
        /// </summary>
        /// <param name="materialsList">材料列表的列表</param>
        /// <param name="ct">取消令牌</param>
        /// <returns>技能数据列表</returns>
        public async UniTask<List<SkillAtomData>> SynthesizeBatchAsync(
            List<List<MaterialData>> materialsList,
            CancellationToken ct = default)
        {
            var results = new List<SkillAtomData>();

            foreach (var materials in materialsList)
            {
                ct.ThrowIfCancellationRequested();

                var skill = await SynthesizeAsync(materials, ct);
                results.Add(skill);

                // 添加小延迟避免 API 限流
                await UniTask.Delay(100, cancellationToken: ct);
            }

            return results;
        }

        #endregion

        #region 安全阀

        /// <summary>
        /// 应用数值限制（安全阀）
        /// 确保 LLM 输出不超出合法范围
        /// </summary>
        /// <param name="data">技能数据</param>
        public void ClampValues(SkillAtomData data)
        {
            if (data == null) return;

            // 使用 SkillAtomData 内置的 Clamp 方法
            data.Clamp();
        }

        #endregion

        #region 回退技能

        /// <summary>
        /// 创建回退技能（当 LLM 不可用时）
        /// </summary>
        /// <param name="materials">材料列表</param>
        /// <returns>基础技能数据</returns>
        private SkillAtomData CreateFallbackSkill(List<MaterialData> materials)
        {
            var skill = new SkillAtomData
            {
                displayName = "基础攻击",
                directHP = -20f,
                projectileSpeed = AtomConstants.DefaultProjectileSpeed,
                pierce = 0,
                bounce = 0,
                split = 0,
                tracking = 0f,
                aoeRadius = 0f,
                shape = ShapeType.Point,
                trigger = TriggerType.Immediate,
                target = TargetType.SingleEnemy,
                cooldown = 1f
            };

            // 根据材料数量稍微增强
            if (materials != null && materials.Count > 0)
            {
                skill.directHP = -20f - (materials.Count * 5f);
                skill.displayName = $"合成攻击 Lv.{materials.Count}";

                // 根据材料类型添加简单效果
                foreach (var material in materials)
                {
                    switch (material.Type)
                    {
                        case MaterialType.Offensive:
                            skill.directHP -= 10f;
                            break;
                        case MaterialType.Defensive:
                            skill.directHP += 5f;  // 减少伤害但增加其他效果
                            skill.duration = 3f;
                            skill.defenseMod = 10f;
                            break;
                        case MaterialType.Speed:
                            skill.projectileSpeed += 2f;
                            skill.tracking = 45f;
                            break;
                        case MaterialType.Control:
                            skill.slowPercent = 20f;
                            skill.duration = 2f;
                            break;
                    }
                }
            }

            skill.Clamp();
            return skill;
        }

        #endregion

        #region 测试方法

        /// <summary>
        /// 创建测试用技能数据
        /// </summary>
        /// <param name="skillType">技能类型名</param>
        /// <returns>测试用技能数据</returns>
        public static SkillAtomData CreateTestSkill(string skillType = "basic")
        {
            var skill = skillType.ToLower() switch
            {
                "tracking" => new SkillAtomData
                {
                    displayName = "追踪弹",
                    directHP = -15f,
                    projectileSpeed = 8f,
                    tracking = 180f,
                    pierce = 0
                },
                "split" => new SkillAtomData
                {
                    displayName = "分裂弹",
                    directHP = -10f,
                    projectileSpeed = 10f,
                    split = 3,
                    pierce = 0
                },
                "bounce" => new SkillAtomData
                {
                    displayName = "弹射弹",
                    directHP = -12f,
                    projectileSpeed = 12f,
                    bounce = 3,
                    pierce = 0
                },
                "aoe" => new SkillAtomData
                {
                    displayName = "范围爆发",
                    directHP = -25f,
                    projectileSpeed = 6f,
                    aoeRadius = 3f,
                    shape = ShapeType.Circle
                },
                "dot" => new SkillAtomData
                {
                    displayName = "毒弹",
                    directHP = -5f,
                    dotHP = -8f,
                    duration = 5f,
                    projectileSpeed = 10f
                },
                "combo" => new SkillAtomData
                {
                    displayName = "涌现风暴",
                    directHP = -8f,
                    projectileSpeed = 10f,
                    tracking = 90f,
                    split = 2,
                    bounce = 2,
                    pierce = 1
                },
                _ => new SkillAtomData
                {
                    displayName = "基础弹",
                    directHP = -20f,
                    projectileSpeed = AtomConstants.DefaultProjectileSpeed
                }
            };

            skill.Clamp();
            return skill;
        }

        #endregion
    }
}
