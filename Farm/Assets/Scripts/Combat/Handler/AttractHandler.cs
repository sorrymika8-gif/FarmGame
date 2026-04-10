using UnityEngine;
using FarmGame.Combat.Entity;
using FarmGame.Combat.Data;

namespace FarmGame.Combat.Handler
{
    /// <summary>
    /// 吸引/排斥处理器 - 对周围敌人施加吸引或排斥力
    /// </summary>
    public static class AttractHandler
    {
        /// <summary>吸引/排斥作用范围</summary>
        private const float ATTRACT_RANGE = 8f;

        /// <summary>力的基础系数</summary>
        private const float FORCE_MULTIPLIER = 50f;

        /// <summary>
        /// 执行吸引/排斥逻辑
        /// </summary>
        /// <param name="entity">技能实体</param>
        public static void Execute(SkillEntity entity)
        {
            if (entity == null || entity.Data == null) return;
            if (Mathf.Approximately(entity.Data.attract, 0f)) return;

            // 确定目标类型
            EntityType targetType = EntityType.Enemy;
            if (entity.Owner != null)
            {
                targetType = entity.Owner.EntityType == EntityType.Player
                    ? EntityType.Enemy
                    : EntityType.Player;
            }

            // 获取范围内的目标
            var colliders = Physics2D.OverlapCircleAll(
                entity.transform.position,
                ATTRACT_RANGE
            );

            float attractForce = entity.Data.attract * FORCE_MULTIPLIER;

            foreach (var collider in colliders)
            {
                var charEntity = collider.GetComponent<CharEntity>();
                if (charEntity == null) continue;
                if (charEntity.EntityType != targetType) continue;
                if (!charEntity.IsAlive) continue;

                Vector3 toEntity = charEntity.Position - entity.transform.position;
                float distance = toEntity.magnitude;

                if (distance < 0.1f) continue;  // 太近跳过

                // 力的方向：正数吸引（指向投射物），负数排斥（远离投射物）
                Vector2 forceDirection;
                if (attractForce > 0)
                {
                    // 吸引：指向投射物
                    forceDirection = -toEntity.normalized;
                }
                else
                {
                    // 排斥：远离投射物
                    forceDirection = toEntity.normalized;
                }

                // 力的大小随距离衰减
                float forceMagnitude = Mathf.Abs(attractForce) / distance;
                Vector2 force = forceDirection * forceMagnitude * Time.deltaTime;

                charEntity.ApplyForce(force, ForceMode2D.Force);
            }
        }

        /// <summary>
        /// 在指定位置应用一次性吸引/排斥脉冲
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="radius">作用半径</param>
        /// <param name="force">力的大小（正=吸引，负=排斥）</param>
        /// <param name="targetType">目标类型</param>
        public static void ApplyPulse(
            Vector3 center,
            float radius,
            float force,
            EntityType targetType)
        {
            if (Mathf.Approximately(force, 0f)) return;

            var colliders = Physics2D.OverlapCircleAll(center, radius);

            foreach (var collider in colliders)
            {
                var charEntity = collider.GetComponent<CharEntity>();
                if (charEntity == null) continue;
                if (charEntity.EntityType != targetType) continue;
                if (!charEntity.IsAlive) continue;

                Vector3 toEntity = charEntity.Position - center;
                float distance = toEntity.magnitude;

                if (distance < 0.1f) continue;

                Vector2 forceDirection = force > 0
                    ? -toEntity.normalized  // 吸引
                    : toEntity.normalized;  // 排斥

                float forceMagnitude = Mathf.Abs(force) * (1f - distance / radius);
                Vector2 impulse = forceDirection * forceMagnitude;

                charEntity.ApplyForce(impulse, ForceMode2D.Impulse);
            }
        }
    }
}
