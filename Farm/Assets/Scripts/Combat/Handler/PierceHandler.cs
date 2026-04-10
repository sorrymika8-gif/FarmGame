using FarmGame.Combat.Entity;

namespace FarmGame.Combat.Handler
{
    /// <summary>
    /// 穿透处理器 - 管理穿透计数
    /// </summary>
    public static class PierceHandler
    {
        /// <summary>
        /// 检查是否可以继续穿透
        /// </summary>
        /// <param name="entity">技能实体</param>
        /// <returns>是否可以继续穿透</returns>
        public static bool CanPierce(SkillEntity entity)
        {
            if (entity == null) return false;
            return entity.PierceRemaining > 0;
        }

        /// <summary>
        /// 处理穿透（在 SkillEntity.OnTriggerEnter2D 中已内置处理）
        /// 此方法用于外部检查和特殊逻辑
        /// </summary>
        /// <param name="entity">技能实体</param>
        /// <returns>穿透后是否应该销毁</returns>
        public static bool ShouldDestroyAfterHit(SkillEntity entity)
        {
            if (entity == null) return true;

            // 如果没有穿透能力，命中后应该销毁
            return entity.PierceRemaining <= 0;
        }
    }
}
