using FarmGame.Combat.Data;

namespace FarmGame.Combat.Core
{
    /// <summary>
    /// 战斗配置常量
    /// </summary>
    public static class CombatConfig
    {
        #region 场景配置

        /// <summary>战斗场景名</summary>
        public const string COMBAT_SCENE_NAME = "Combat";

        /// <summary>战斗场景路径</summary>
        public const string COMBAT_SCENE_PATH = "Scenes/Combat";

        #endregion

        #region 预制体路径

        /// <summary>技能实体预制体路径</summary>
        public const string SKILL_ENTITY_PREFAB = "Combat/SkillEntity";

        /// <summary>角色实体预制体路径（敌人）</summary>
        public const string CHAR_ENTITY_PREFAB = "Combat/CharEntity";

        /// <summary>玩家战斗预制体路径</summary>
        public const string PLAYER_COMBAT_PREFAB = "Combat/PlayerCombat";

        /// <summary>伤害飘字预制体路径</summary>
        public const string DAMAGE_POPUP_PREFAB = "Combat/DamagePopup";

        #endregion

        #region 战斗区域

        /// <summary>战斗区域宽度</summary>
        public const float BATTLE_AREA_WIDTH = 20f;

        /// <summary>战斗区域高度</summary>
        public const float BATTLE_AREA_HEIGHT = 15f;

        /// <summary>玩家出生点 X 偏移</summary>
        public const float PLAYER_SPAWN_X = -7f;

        /// <summary>敌人出生点 X 偏移</summary>
        public const float ENEMY_SPAWN_X = 7f;

        #endregion

        #region 战斗参数

        /// <summary>战斗开始倒计时（秒）</summary>
        public const float BATTLE_START_COUNTDOWN = 3f;

        /// <summary>战斗结束延迟（秒）</summary>
        public const float BATTLE_END_DELAY = 2f;

        /// <summary>玩家无敌帧时间（秒）</summary>
        public const float PLAYER_INVINCIBILITY_TIME = 0.5f;

        /// <summary>技能释放最小间隔（秒）</summary>
        public const float MIN_SKILL_INTERVAL = 0.1f;

        #endregion

        #region 敌人配置

        /// <summary>默认敌人 HP</summary>
        public const float DEFAULT_ENEMY_HP = 100f;

        /// <summary>默认敌人攻击力</summary>
        public const float DEFAULT_ENEMY_ATTACK = 10f;

        /// <summary>默认敌人防御</summary>
        public const float DEFAULT_ENEMY_DEFENSE = 5f;

        /// <summary>默认敌人移速</summary>
        public const float DEFAULT_ENEMY_MOVE_SPEED = 3f;

        #endregion

        #region 玩家配置

        /// <summary>默认玩家战斗 HP</summary>
        public const float DEFAULT_PLAYER_HP = 200f;

        /// <summary>默认玩家攻击力</summary>
        public const float DEFAULT_PLAYER_ATTACK = 15f;

        /// <summary>默认玩家防御</summary>
        public const float DEFAULT_PLAYER_DEFENSE = 10f;

        /// <summary>默认玩家移速</summary>
        public const float DEFAULT_PLAYER_MOVE_SPEED = 5f;

        #endregion

        #region 碰撞层配置

        /// <summary>
        /// 碰撞层名称 - 需要在 Unity 编辑器 Tags & Layers 中配置
        /// </summary>
        public static class Layers
        {
            /// <summary>玩家层名</summary>
            public const string PLAYER = "Player";

            /// <summary>敌人层名</summary>
            public const string ENEMY = "Enemy";

            /// <summary>玩家投射物层名</summary>
            public const string PLAYER_PROJECTILE = "PlayerProjectile";

            /// <summary>敌人投射物层名</summary>
            public const string ENEMY_PROJECTILE = "EnemyProjectile";

            /// <summary>获取敌方实体层掩码（根据来源类型）</summary>
            public static int GetTargetLayerMask(EntityType sourceType)
            {
                return sourceType == EntityType.Player
                    ? UnityEngine.LayerMask.GetMask(ENEMY)
                    : UnityEngine.LayerMask.GetMask(PLAYER);
            }

            /// <summary>获取己方投射物层（根据来源类型）</summary>
            public static int GetProjectileLayer(EntityType sourceType)
            {
                string layerName = sourceType == EntityType.Player
                    ? PLAYER_PROJECTILE
                    : ENEMY_PROJECTILE;
                return UnityEngine.LayerMask.NameToLayer(layerName);
            }
        }

        #endregion

        #region AI配置

        /// <summary>敌人检测玩家范围</summary>
        public const float ENEMY_DETECTION_RANGE = 12f;

        /// <summary>敌人攻击范围</summary>
        public const float ENEMY_ATTACK_RANGE = 8f;

        /// <summary>敌人攻击冷却（秒）</summary>
        public const float ENEMY_ATTACK_COOLDOWN = 2f;

        /// <summary>敌人逃跑血量阈值（百分比）</summary>
        public const float ENEMY_FLEE_HP_THRESHOLD = 0.2f;

        /// <summary>敌人AI更新间隔（秒）</summary>
        public const float ENEMY_AI_UPDATE_INTERVAL = 0.1f;

        #endregion
    }
}
