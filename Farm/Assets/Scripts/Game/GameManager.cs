using UnityEngine;
using QFramework;
using FarmGame.Map;
using FarmGame.Player;
using FarmGame.Movement;
using FarmGame.Farm;
using FarmGame.Farm.View;

namespace FarmGame.Game
{
    public class GameManager : MonoSingleton<GameManager>
    {
        private const string INITIAL_MAP_NAME = "init_map";

        public void EnterScene(string mapName, Vector3 spawnPos)
        {
            // 1. 加载地图
            if (!MapManager.Instance.LoadMap(mapName))
            {
                Debug.LogError($"[GameManager] Failed to load map: {mapName}");
                return;
            }

            // 2. 确保玩家存在
            if (PlayerManager.Instance.Player == null)
            {
                if (!PlayerManager.Instance.CreatePlayer())
                {
                    Debug.LogError("[GameManager] Failed to create player");
                    return;
                }
            }

            // 3. 设置玩家位置
            PlayerManager.Instance.SetPlayerPosition(spawnPos);

            // 4. 绑定相机
            SetupCamera();

            // 5. 初始化农田视图（仅初始地图）
            SetupFarmOnInitMap(mapName);

            Debug.Log($"[GameManager] Entered scene: {mapName} at {spawnPos}");
        }

        private void SetupCamera()
        {
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                var cameraFollow = mainCamera.GetComponent<CameraFollow>();
                if (cameraFollow == null)
                {
                    cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
                }
                
                if (cameraFollow != null && PlayerManager.Instance.Player != null)
                {
                    cameraFollow.SetTarget(PlayerManager.Instance.Player.transform);
                }
            }
        }

        private void SetupFarmOnInitMap(string mapName)
        {
            if (mapName != INITIAL_MAP_NAME)
            {
                return;
            }

            var currentMap = MapManager.Instance.CurrentMap;
            if (currentMap == null)
            {
                Debug.LogWarning("[GameManager] CurrentMap 为空，无法挂载农田资源");
                return;
            }

            var farmView = currentMap.GetComponentInChildren<FarmTilemapView>(true);

            if (farmView == null)
            {
                Debug.LogWarning("[GameManager] init_map 中未挂载 FarmTilemapView");
                return;
            }

            if (farmView == null)
            {
                Debug.LogWarning("[GameManager] FarmMap 上缺少 FarmTilemapView 组件");
                return;
            }

            if (FarmManager.Instance.CurrentMap == null)
            {
                Debug.LogWarning("[GameManager] FarmManager.CurrentMap 为空，无法初始化农田视图");
                return;
            }

            farmView.Initialize(FarmManager.Instance.CurrentMap);

            var player = PlayerManager.Instance.Player;
            if (player != null)
            {
                var inputHandler = player.GetComponent<PlayerInputHandler>();
                if (inputHandler != null)
                {
                    inputHandler.SetFarmView(farmView);
                }
            }

            Debug.Log("[GameManager] init_map 农田资源挂载完成");
        }
    }
}
