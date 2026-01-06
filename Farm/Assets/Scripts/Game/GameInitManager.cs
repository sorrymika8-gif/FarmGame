using UnityEngine;
using QFramework;
using FarmGame.Map;
using FarmGame.Player;

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

            // 检查是否为新玩家
            if (!playerData.IsNewPlayer)
            {
                Debug.Log("[GameInitManager] Not a new player, skipping initialization");
                return;
            }

            Debug.Log("[GameInitManager] Starting new game initialization...");

            // 1. 加载初始地图
            if (!MapManager.Instance.LoadMap(INITIAL_MAP))
            {
                Debug.LogError("[GameInitManager] Failed to load initial map");
                return;
            }

            // 2. 设置玩家初始位置
            PlayerManager.Instance.SetPlayerPosition(INITIAL_SPAWN_POSITION);

            // 3. 标记为非新玩家
            playerData.IsNewPlayer = false;

            Debug.Log("[GameInitManager] New game initialization completed");
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
