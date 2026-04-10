namespace FarmGame.Combat.Data
{
    /// <summary>
    /// 原子数值常量 - 数值平衡的唯一入口
    /// LLMBridge 使用这些常量进行 Clamp，确保 LLM 输出不超出合法范围
    /// </summary>
    public static class AtomConstants
    {
        #region 弹道行为限制

        /// <summary>最大弹射次数</summary>
        public const int MaxBounce = 5;

        /// <summary>最大分裂数量</summary>
        public const int MaxSplit = 5;

        /// <summary>最大穿透目标数</summary>
        public const int MaxPierce = 10;

        /// <summary>最大追踪角度</summary>
        public const float MaxTracking = 360f;

        /// <summary>最大吸引/排斥强度</summary>
        public const float MaxAttract = 10f;

        #endregion

        #region 范围限制

        /// <summary>最大 AOE 半径</summary>
        public const float MaxAOE = 8f;

        /// <summary>最大弹道宽度</summary>
        public const float MaxProjectileWidth = 3f;

        #endregion

        #region 数值效果限制

        /// <summary>最大单次伤害/治疗</summary>
        public const float MaxDirectHP = 500f;

        /// <summary>最大每秒持续生命变化</summary>
        public const float MaxDotHP = 50f;

        /// <summary>最大移速变化百分比</summary>
        public const float MaxMoveSpeedMod = 100f;

        /// <summary>最大攻击力变化百分比</summary>
        public const float MaxAttackMod = 200f;

        /// <summary>最大防御变化百分比</summary>
        public const float MaxDefenseMod = 100f;

        #endregion

        #region 状态限制

        /// <summary>最大减速百分比</summary>
        public const float MaxSlowPercent = 90f;

        /// <summary>最大沉默持续时间</summary>
        public const float MaxSilenceDuration = 10f;

        /// <summary>最大易伤/减伤倍率</summary>
        public const float MaxDamageMultiplier = 3f;

        /// <summary>最大隐身持续时间</summary>
        public const float MaxStealthDuration = 15f;

        #endregion

        #region 时间限制

        /// <summary>最大延迟释放时间</summary>
        public const float MaxDelay = 5f;

        /// <summary>最大效果持续时长</summary>
        public const float MaxDuration = 30f;

        /// <summary>最小冷却时间</summary>
        public const float MinCooldown = 0.1f;

        /// <summary>最大冷却时间</summary>
        public const float MaxCooldown = 60f;

        #endregion

        #region 速度限制

        /// <summary>最小投射物飞行速度</summary>
        public const float MinProjectileSpeed = 1f;

        /// <summary>最大投射物飞行速度</summary>
        public const float MaxProjectileSpeed = 30f;

        /// <summary>默认投射物飞行速度</summary>
        public const float DefaultProjectileSpeed = 10f;

        #endregion

        #region 生成调度阀门

        /// <summary>生成队列容量</summary>
        public const int SpawnQueueCapacity = 256;

        /// <summary>每帧生成上限（默认）</summary>
        public const int MaxSpawnPerFrame = 10;

        /// <summary>低帧率时每帧生成上限</summary>
        public const int AdaptiveSpawnFloor = 5;

        /// <summary>高帧率时每帧生成上限</summary>
        public const int AdaptiveSpawnCeiling = 15;

        /// <summary>FPS 自适应阈值</summary>
        public const float FPSAdaptiveThreshold = 40f;

        /// <summary>实体池最大容量</summary>
        public const int EntityPoolCapacity = 200;

        #endregion
    }
}
