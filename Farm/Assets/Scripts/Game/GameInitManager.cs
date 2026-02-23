using UnityEngine;
using QFramework;
using FarmGame.Map;
using FarmGame.Player;
using FarmGame.Core;

namespace FarmGame.Game
{
    /// <summary>
    /// 游戏初始化管理器
    /// 负责玩家首次进入游戏时的初始化逻辑
    /// </summary>
    public class GameInitManager : MonoSingleton<GameInitManager>
    {
        #region 常量

        // 硬编码配置，后续接入配置系统
        private const string INITIAL_MAP = "init_map";
        private static readonly Vector3 INITIAL_SPAWN_POSITION = new Vector3(0, 0, 0);

        // 初始物品配置（对应 seed.xlsx 中的 class_id）
        private const int GRASS_SEED_CONFIG_ID = 1000;
        private const int INITIAL_SEED_COUNT = 3;

        #endregion

        #region 私有字段

        private bool mIsInitialized;

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化游戏初始化管理器
        /// </summary>
        public void Initialize()
        {
            if (mIsInitialized) return;

            mIsInitialized = true;
            Debug.Log("[GameInitManager] Initialized");
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 开始新游戏
        /// 检查玩家是否为新玩家，若是则执行首次初始化逻辑
        /// </summary>
        public void StartNewGame()
        {
            if (!ValidateInitialized()) return;

            // 先创建玩家以获取 PlayerData
            if (!PlayerManager.Instance.CreatePlayer())
            {
                Debug.LogError("[GameInitManager] Failed to create player");
                return;
            }

            var playerData = PlayerManager.Instance.Player.Data;
            bool isNewPlayer = playerData.IsNewPlayer;

            // 无论是否新玩家，都需要进入场景
            // TODO: 非新玩家时应从存档读取地图名和位置，目前暂用初始值
            string mapToLoad = INITIAL_MAP;
            Vector3 spawnPosition = INITIAL_SPAWN_POSITION;

            Debug.Log($"[GameInitManager] Starting game, IsNewPlayer: {isNewPlayer}");

            // 使用 GameManager 进入场景
            GameManager.Instance.EnterScene(mapToLoad, spawnPosition);

            // 打开主界面
            FarmGame.Core.UIManager.Instance.OpenMainUIPanel();
            Debug.Log("[GameInitManager] MainUIPanel opened");

            // 如果是新玩家，发放初始物品并标记为非新玩家
            if (isNewPlayer)
            {
                // 发放初始物品：3颗牧草种子
                playerData.Inventory.AddItem(GRASS_SEED_CONFIG_ID, INITIAL_SEED_COUNT);
                Debug.Log($"[GameInitManager] Granted initial items: {INITIAL_SEED_COUNT} grass seeds (ConfigId: {GRASS_SEED_CONFIG_ID})");

                playerData.IsNewPlayer = false;
                Debug.Log("[GameInitManager] New player marked as initialized");
            }

            Debug.Log("[GameInitManager] Game initialization completed");
        }

        #endregion

        #region 私有方法

        private bool ValidateInitialized()
        {
            if (!mIsInitialized)
            {
                Debug.LogError("[GameInitManager] Not initialized. Call Initialize() first.");
                return false;
            }
            return true;
        }

        #endregion
    }
}
