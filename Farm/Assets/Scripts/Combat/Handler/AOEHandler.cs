using UnityEngine;
using FarmGame.Combat.Entity;
using FarmGame.Combat.Data;

namespace FarmGame.Combat.Handler
{
    /// <summary>
    /// AOE 处理器 - 范围效果处理
    /// </summary>
    public static class AOEHandler
    {
        /// <summary>
        /// 执行 AOE 效果
        /// </summary>
        /// <param name="entity">技能实体</param>
        /// <param name="center">AOE 中心点</param>
        public static void Execute(SkillEntity entity, Vector3 center)
        {
            if (entity == null || entity.Data == null) return;
            if (entity.Data.aoeRadius <= 0) return;

            // 确定目标类型
            EntityType targetType = EntityType.Enemy;
            if (entity.Owner != null)
            {
                targetType = entity.Owner.EntityType == EntityType.Player
                    ? EntityType.Enemy
                    : EntityType.Player;
            }

            ApplyAOE(entity.Data, center, targetType, entity.Owner);
        }

        /// <summary>
        /// 在指定位置应用 AOE 效果
        /// </summary>
        /// <param name="data">技能原子数据</param>
        /// <param name="center">中心点</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="source">效果来源</param>
        public static void ApplyAOE(
            SkillAtomData data,
            Vector3 center,
            EntityType targetType,
            CharEntity source = null)
        {
            if (data == null || data.aoeRadius <= 0) return;

            var targets = GetTargetsInArea(center, data.aoeRadius, data.shape, targetType);

            foreach (var target in targets)
            {
                if (target == null || !target.IsAlive) continue;

                // 应用效果
                EffectApplier.Apply(data, target, source);
            }
        }

        /// <summary>
        /// 获取区域内的所有目标
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="radius">半径</param>
        /// <param name="shape">形状</param>
        /// <param name="targetType">目标类型</param>
        /// <returns>目标列表</returns>
        public static CharEntity[] GetTargetsInArea(
            Vector3 center,
            float radius,
            ShapeType shape,
            EntityType targetType)
        {
            Collider2D[] colliders;

            switch (shape)
            {
                case ShapeType.Circle:
                case ShapeType.Point:
                    colliders = Physics2D.OverlapCircleAll(center, radius);
                    break;

                case ShapeType.Line:
                    // 直线用细长的矩形近似
                    colliders = Physics2D.OverlapBoxAll(
                        center,
                        new Vector2(radius * 2f, 0.5f),
                        0f
                    );
                    break;

                case ShapeType.Fan:
                    // 扇形暂时用圆形近似，TODO: 精确扇形检测
                    colliders = Physics2D.OverlapCircleAll(center, radius);
                    break;

                default:
                    colliders = Physics2D.OverlapCircleAll(center, radius);
                    break;
            }

            // 筛选目标
            var result = new System.Collections.Generic.List<CharEntity>();

            foreach (var collider in colliders)
            {
                var charEntity = collider.GetComponent<CharEntity>();
                if (charEntity == null) continue;
                if (charEntity.EntityType != targetType) continue;
                if (!charEntity.IsAlive) continue;

                result.Add(charEntity);
            }

            return result.ToArray();
        }

        /// <summary>
        /// 获取区域内目标数量
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="radius">半径</param>
        /// <param name="targetType">目标类型</param>
        /// <returns>目标数量</returns>
        public static int CountTargetsInArea(
            Vector3 center,
            float radius,
            EntityType targetType)
        {
            var colliders = Physics2D.OverlapCircleAll(center, radius);
            int count = 0;

            foreach (var collider in colliders)
            {
                var charEntity = collider.GetComponent<CharEntity>();
                if (charEntity == null) continue;
                if (charEntity.EntityType != targetType) continue;
                if (!charEntity.IsAlive) continue;
                count++;
            }

            return count;
        }
    }
}
