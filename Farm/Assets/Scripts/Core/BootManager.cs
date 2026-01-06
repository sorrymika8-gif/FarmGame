using UnityEngine;
using QFramework;
using FarmGame.Map;
using FarmGame.Player;
using FarmGame.Game;
using FarmGame.Movement;

namespace FarmGame.Core
{
    /// <summary>
    /// 启动管理器
    /// 负责游戏启动时按顺序初始化各个管理器
    /// </summary>
    public class BootManager : MonoSingleton<BootManager>
    {
        #region 私有字段

        private bool mIsInitialized;

        #endregion

        #region 生命周期

        private void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// 初始化所有管理器
        /// </summary>
        public void Initialize()
        {
            if (mIsInitialized) return;

            Debug.Log("[BootManager] Starting initialization...");

            // 1. 初始化资源管理器（最先，其他模块可能依赖资源）
            ResourceManager.Instance.Initialize();
            Debug.Log("[BootManager] ResourceManager initialized");

            // 2. 初始化UI管理器
            UIManager.Instance.Initialize();
            Debug.Log("[BootManager] UIManager initialized");

            // 3. 初始化地图管理器
            MapManager.Instance.Initialize();
            Debug.Log("[BootManager] MapManager initialized");

            // 4. 初始化玩家管理器
            PlayerManager.Instance.Initialize();
            Debug.Log("[BootManager] PlayerManager initialized");

            // 5. 初始化移动管理器（依赖玩家管理器）
            MovementManager.Instance.Initialize();
            Debug.Log("[BootManager] MovementManager initialized");

            // 6. 初始化游戏初始化管理器
            GameInitManager.Instance.Initialize();
            Debug.Log("[BootManager] GameInitManager initialized");

            // 7. 在此处添加其他管理器的初始化...

            mIsInitialized = true;
            Debug.Log("[BootManager] All managers initialized successfully");

            // 启动新游戏（检查是否为新玩家）
            GameInitManager.Instance.StartNewGame();
        }

        #endregion
    }
}
