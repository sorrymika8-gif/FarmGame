using UnityEngine;
using FarmGame.Combat.Entity;
using FarmGame.Combat.Data;

namespace FarmGame.Combat.Handler
{
    /// <summary>
    /// 追踪处理器 - 让投射物追踪最近敌人
    /// </summary>
    public static class TrackingHandler
    {
        /// <summary>追踪检测范围</summary>
        private const float TRACKING_RANGE = 15f;

        /// <summary>追踪灵敏度（每秒最大转向速度）</summary>
        private const float TRACKING_SENSITIVITY = 5f;

        /// <summary>
        /// 执行追踪逻辑
        /// </summary>
        /// <param name="entity">技能实体</param>
        public static void Execute(SkillEntity entity)
        {
            if (entity == null || entity.Data == null) return;
            if (entity.Data.tracking <= 0) return;

            // 查找最近的敌方目标
            var target = FindNearestTarget(entity);
            if (target == null) return;

            // 计算目标方向
            Vector3 toTarget = target.Position - entity.transform.position;
            if (toTarget.sqrMagnitude < 0.01f) return;

            Vector3 targetDirection = toTarget.normalized;
            Vector3 currentDirection = entity.MoveDirection;

            // 计算当前方向和目标方向的夹角
            float angle = Vector3.SignedAngle(currentDirection, targetDirection, Vector3.forward);

            // 限制最大转向角度
            float maxTurnAngle = entity.Data.tracking;
            float turnAngle = Mathf.Clamp(angle, -maxTurnAngle, maxTurnAngle);

            // 平滑转向
            float smoothTurn = turnAngle * TRACKING_SENSITIVITY * Time.deltaTime;

            // 应用旋转
            entity.RotateDirection(smoothTurn);
        }

        /// <summary>
        /// 查找最近的敌方目标
        /// </summary>
        /// <param name="entity">技能实体</param>
        /// <returns>最近的敌方目标，如果没有则返回 null</returns>
        private static CharEntity FindNearestTarget(SkillEntity entity)
        {
            // 确定敌方类型
            EntityType targetType = EntityType.Enemy;
            if (entity.Owner != null)
            {
                targetType = entity.Owner.EntityType == EntityType.Player
                    ? EntityType.Enemy
                    : EntityType.Player;
            }

            // 获取范围内所有碰撞体
            var colliders = Physics2D.OverlapCircleAll(
                entity.transform.position,
                TRACKING_RANGE
            );

            CharEntity nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var collider in colliders)
            {
                var charEntity = collider.GetComponent<CharEntity>();
                if (charEntity == null) continue;
                if (charEntity.EntityType != targetType) continue;
                if (!charEntity.IsAlive) continue;
                if (charEntity.Stats.IsStealthed) continue;  // 隐身目标不可追踪

                float distance = Vector3.Distance(
                    entity.transform.position,
                    charEntity.Position
                );

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = charEntity;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 查找范围内的指定类型目标
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="range">范围</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="exclude">排除的目标</param>
        /// <returns>最近的目标</returns>
        public static CharEntity FindNearestInRange(
            Vector3 center,
            float range,
            EntityType targetType,
            CharEntity exclude = null)
        {
            var colliders = Physics2D.OverlapCircleAll(center, range);

            CharEntity nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var collider in colliders)
            {
                var charEntity = collider.GetComponent<CharEntity>();
                if (charEntity == null) continue;
                if (charEntity == exclude) continue;
                if (charEntity.EntityType != targetType) continue;
                if (!charEntity.IsAlive) continue;

                float distance = Vector3.Distance(center, charEntity.Position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = charEntity;
                }
            }

            return nearest;
        }
    }
}
