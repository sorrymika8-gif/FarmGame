using UnityEngine;
using QFramework;
using FarmGame.Map;
using FarmGame.Player;
using FarmGame.Movement;

namespace FarmGame.Game
{
    public class GameManager : MonoSingleton<GameManager>
    {
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
    }
}
