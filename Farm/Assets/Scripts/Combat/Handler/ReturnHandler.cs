using UnityEngine;
using FarmGame.Combat.Entity;

namespace FarmGame.Combat.Handler
{
    /// <summary>
    /// 返回处理器 - 回旋镖效果
    /// </summary>
    public static class ReturnHandler
    {
        /// <summary>返回触发距离</summary>
        private const float RETURN_TRIGGER_DISTANCE = 15f;

        /// <summary>返回到达判定距离</summary>
        private const float RETURN_ARRIVE_DISTANCE = 0.5f;

        /// <summary>
        /// 执行返回逻辑
        /// </summary>
        /// <param name="entity">技能实体</param>
        public static void Execute(SkillEntity entity)
        {
            if (entity == null || entity.Data == null) return;
            if (!entity.Data.returning) return;

            Vector3 ownerPos = entity.GetOwnerPosition();

            if (!entity.IsReturning)
            {
                // 检查是否应该开始返回
                float distanceFromStart = Vector3.Distance(
                    entity.transform.position,
                    entity.StartPosition
                );

                if (distanceFromStart >= RETURN_TRIGGER_DISTANCE)
                {
                    StartReturn(entity);
                }
            }
            else
            {
                // 正在返回，检查是否到达
                float distanceToOwner = Vector3.Distance(
                    entity.transform.position,
                    ownerPos
                );

                if (distanceToOwner <= RETURN_ARRIVE_DISTANCE)
                {
                    // 到达，回收
                    entity.ReturnToPool();
                    return;
                }

                // 持续追踪所有者
                Vector3 toOwner = ownerPos - entity.transform.position;
                entity.SetDirection(toOwner.normalized);
            }
        }

        /// <summary>
        /// 开始返回
        /// </summary>
        /// <param name="entity">技能实体</param>
        private static void StartReturn(SkillEntity entity)
        {
            entity.SetReturning(true);

            // 调转方向
            Vector3 toOwner = entity.GetOwnerPosition() - entity.transform.position;
            entity.SetDirection(toOwner.normalized);
        }

        /// <summary>
        /// 强制开始返回
        /// </summary>
        /// <param name="entity">技能实体</param>
        public static void ForceReturn(SkillEntity entity)
        {
            if (entity == null) return;
            if (!entity.Data.returning) return;

            StartReturn(entity);
        }
    }
}
