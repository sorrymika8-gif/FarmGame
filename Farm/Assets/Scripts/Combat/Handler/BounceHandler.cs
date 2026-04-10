using UnityEngine;
using FarmGame.Combat.Entity;
using FarmGame.Combat.Data;

namespace FarmGame.Combat.Handler
{
    /// <summary>
    /// 弹射处理器 - 命中后弹向下一个目标
    /// </summary>
    public static class BounceHandler
    {
        /// <summary>弹射搜索范围</summary>
        private const float BOUNCE_SEARCH_RANGE = 10f;

        /// <summary>
        /// 执行弹射逻辑
        /// </summary>
        /// <param name="entity">技能实体</param>
        /// <param name="hitTarget">刚刚命中的目标</param>
        /// <returns>是否成功弹射</returns>
        public static bool Execute(SkillEntity entity, CharEntity hitTarget)
        {
            if (entity == null || entity.Data == null) return false;
            if (entity.BounceRemaining <= 0) return false;

            // 确定敌方类型
            EntityType targetType = EntityType.Enemy;
            if (entity.Owner != null)
            {
                targetType = entity.Owner.EntityType == EntityType.Player
                    ? EntityType.Enemy
                    : EntityType.Player;
            }

            // 查找下一个弹射目标
            var nextTarget = FindBounceTarget(
                entity.transform.position,
                targetType,
                hitTarget
            );

            if (nextTarget == null)
            {
                // 没有可弹射的目标
                return false;
            }

            // 计算新方向
            Vector3 toNext = nextTarget.Position - entity.transform.position;
            entity.SetDirection(toNext.normalized);

            return true;
        }

        /// <summary>
        /// 查找弹射目标
        /// </summary>
        /// <param name="center">搜索中心</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="exclude">排除的目标（刚命中的）</param>
        /// <returns>弹射目标</returns>
        private static CharEntity FindBounceTarget(
            Vector3 center,
            EntityType targetType,
            CharEntity exclude)
        {
            return TrackingHandler.FindNearestInRange(
                center,
                BOUNCE_SEARCH_RANGE,
                targetType,
                exclude
            );
        }
    }
}
