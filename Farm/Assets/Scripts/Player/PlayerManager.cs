using UnityEngine;
using QFramework;
using FarmGame.Core;

namespace FarmGame.Player
{
    /// <summary>
    /// 玩家管理器
    /// 负责玩家实体的创建、访问和生命周期管理
    /// </summary>
    public class PlayerManager : MonoSingleton<PlayerManager>
    {
        #region 常量

        private const string PLAYER_PREFAB_PATH = "prefabs/player/player";

        #endregion

        #region 私有字段

        private bool mIsInitialized;
        private PlayerController mPlayer;
        private Transform mPlayerRoot;

        #endregion

        #region 公共属性

        /// <summary>
        /// 玩家控制器实例
        /// </summary>
        public PlayerController Player => mPlayer;

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化玩家管理器
        /// </summary>
        public void Initialize()
        {
            if (mIsInitialized) return;

            // 创建玩家根节点
            mPlayerRoot = new GameObject("PlayerRoot").transform;
            mPlayerRoot.SetParent(transform);

            mIsInitialized = true;
            Debug.Log("[PlayerManager] Initialized");
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private void Dispose()
        {
            if (!mIsInitialized) return;

            DestroyPlayer();
            mIsInitialized = false;
        }

        #endregion

        #region 公共接口 - 玩家管理

        /// <summary>
        /// 创建玩家实例
        /// </summary>
        /// <returns>是否创建成功</returns>
        public bool CreatePlayer()
        {
            if (!ValidateInitialized()) return false;

            if (mPlayer != null)
            {
                Debug.LogWarning("[PlayerManager] Player already exists");
                return true;
            }

            // 加载玩家预制体
            GameObject playerPrefab = ResourceManager.Instance.Load<GameObject>(PLAYER_PREFAB_PATH);
            if (playerPrefab == null)
            {
                Debug.LogError($"[PlayerManager] Failed to load player prefab: {PLAYER_PREFAB_PATH}");
                return false;
            }

            // 实例化玩家
            GameObject playerObj = Instantiate(playerPrefab, mPlayerRoot);
            playerObj.name = "Player";

            // 获取并初始化控制器
            mPlayer = playerObj.GetComponent<PlayerController>();
            if (mPlayer == null)
            {
                Debug.LogError("[PlayerManager] PlayerController component not found on prefab");
                Destroy(playerObj);
                return false;
            }

            mPlayer.Initialize();

            Debug.Log("[PlayerManager] Player created");
            return true;
        }

        /// <summary>
        /// 销毁玩家实例
        /// </summary>
        public void DestroyPlayer()
        {
            if (mPlayer != null)
            {
                Destroy(mPlayer.gameObject);
                mPlayer = null;
                Debug.Log("[PlayerManager] Player destroyed");
            }
        }

        /// <summary>
        /// 设置玩家位置
        /// </summary>
        /// <param name="position">目标位置</param>
        public void SetPlayerPosition(Vector3 position)
        {
            if (!ValidatePlayer()) return;

            mPlayer.Data.Position = position;
            mPlayer.transform.position = position;
        }

        #endregion

        #region 私有方法

        private bool ValidateInitialized()
        {
            if (!mIsInitialized)
            {
                Debug.LogError("[PlayerManager] Not initialized. Call Initialize() first.");
                return false;
            }
            return true;
        }

        private bool ValidatePlayer()
        {
            if (!ValidateInitialized()) return false;

            if (mPlayer == null)
            {
                Debug.LogError("[PlayerManager] Player not created. Call CreatePlayer() first.");
                return false;
            }
            return true;
        }

        #endregion
    }
}
