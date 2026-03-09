using System.Collections.Generic;
using UnityEngine;
using QFramework;
using FarmGame.LLMCore.Brain;
using FarmGame.Core;
using FarmGame.GameConfig;
using FarmGame.GameConfig.Generated;
using FarmGame.Map;
using Cysharp.Threading.Tasks;

namespace FarmGame.Game.NPC
{
    /// <summary>
    /// NPC 管理器
    /// 负责管理所有 NPC 的生命周期和驱动逻辑
    /// </summary>
    public class NPCManager : MonoSingleton<NPCManager>
    {
        private readonly Dictionary<string, NPCEntity> mNpcDict = new();
        
        // View 层控制器映射 (ID -> Controller)
        private readonly Dictionary<string, NPCController> mControllerDict = new();

        private Brain mBrain;
        private Transform mNpcRoot;

        /// <summary>
        /// 获取共享的 Brain 实例
        /// </summary>
        public Brain SharedBrain => mBrain;

        public void Initialize()
        {
            // 创建 NPC 根节点，防止切场景销毁
            mNpcRoot = new GameObject("NPCRoot").transform;
            mNpcRoot.SetParent(transform);

            // 1. 初始化 Brain (使用工厂创建完整功能的 Brain)
            mBrain = BrainFactory.CreateFullBrain();
            
            // 3. 根据配置生成 NPC
            SpawnNPCsFromConfig();

            Debug.Log("[NPCManager] Initialized");
        }

        private void SpawnNPCsFromConfig()
        {
            try
            {
                // 获取 NPC 配置列表 (假设 class_id 是 key)
                var configMap = ConfigManager.Instance.GetMap<int, NpcConfig>();
                
                if (configMap == null)
                {
                    Debug.LogError("[NPCManager] NpcConfig map is NULL! Configs might not be loaded.");
                    return;
                }

                var allConfigs = configMap.GetAll();
                Debug.Log($"[NPCManager] Found {System.Linq.Enumerable.Count(allConfigs)} NPC configs.");

                // 通过 GetAll() 接口迭代所有 NPC 配置
                foreach (var config in allConfigs)
                {
                    Debug.Log($"[NPCManager] Spawning NPC: {config.name} (ID: {config.class_id})");
                    SpawnNPC(config);
                }

                Debug.Log($"[NPCManager] Spawned NPCs from config");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[NPCManager] Failed to spawn NPCs from config: {e}");
            }
        }

        private void SpawnNPC(NpcConfig config)
        {
            if (config == null) return;

            // 1. 加载 Prefab
            // 配置代码的文件名读取逻辑，从读取表第一行，改为读取文件名。
            // npc的预制体，统一在resource/prefabs/npcs/目录下。具体预制体与配置表中model_name字段配置的同名
            
            // model_name 只是文件名，例如 "haman"
            // 实际上 prefab 可能是: resources://Prefabs/Npcs/haman/haman (子文件夹)
            // 或者 resources://Prefabs/Npcs/haman (直接文件)
            var prefabPath = $"Prefabs/Npcs/{config.model_name}/{config.model_name}";
            var prefab = ResourceManager.Instance.Load<GameObject>(prefabPath);

            // 如果加载失败，尝试旧路径（直接在 Npcs 目录下）
            if (prefab == null)
            {
                prefabPath = $"Prefabs/Npcs/{config.model_name}";
                prefab = ResourceManager.Instance.Load<GameObject>(prefabPath);
            }

            if (prefab == null)
            {
                Debug.LogError($"[NPCManager] Cannot find NPC prefab: {config.model_name} at path: {prefabPath}");
                return;
            }

            // 2. 实例化
            var go = Object.Instantiate(prefab, mNpcRoot);
            go.name = config.name; // 修改 GameObject 名字
            
            // 设置初始位置
            Vector3 spawnPos = Vector3.zero;
            if (config.init_pos != null)
            {
                 Debug.Log($"[NPCManager] Config init_pos for '{config.name}': Length={config.init_pos.Length}, Values=[{string.Join(",", config.init_pos)}]");
            }
            
            if (config.init_pos != null && config.init_pos.Length >= 2)
            {
                // 使用 MapManager 转换网格坐标到世界坐标
                spawnPos = MapManager.Instance.GridToWorld(config.init_pos[0], config.init_pos[1]);
            }
            
            go.transform.position = spawnPos;
            
            // 3. 构建 Entity
            var entity = new NPCEntity(config.class_id.ToString(), config.name);
            entity.Position = spawnPos; // 同步位置到数据实体，防止 Bind 时被重置为 0
            entity.Gender = config.gender ?? "未知";
            
            // 设置交互距离 (从配置读取)
            entity.InteractionDistance = config.interaction_dis > 0 ? config.interaction_dis : 2f;
            
            // 设置NPC专属提示词文件路径
            entity.PromptFilePath = config.prompt;
            
            // 4. 注册 Entity
            Register(entity);

            // 5. 绑定 Controller
            var controller = go.GetComponent<NPCController>();
            if (controller == null)
            {
                controller = go.AddComponent<NPCController>();
            }
            
            // 6. 绑定 DialogueSystem
            var dialogueSystem = go.GetComponent<DialogueSystem>();
            if (dialogueSystem == null)
            {
                dialogueSystem = go.AddComponent<DialogueSystem>();
            }
            if (dialogueSystem != null)
            {
                dialogueSystem.Bind(entity);
            }

            if (controller != null)
            {
                controller.Bind(entity);
                RegisterController(controller);
            }

            // 7. 添加基于Y坐标的排序组件
            var sortByY = go.GetComponent<SpriteSortByY>();
            if (sortByY == null)
            {
                sortByY = go.AddComponent<SpriteSortByY>();
            }
            sortByY.SortingLayerName = SortingLayerConfig.Characters;
        }

        public void Register(NPCEntity npc)
        {
            if (npc == null || mNpcDict.ContainsKey(npc.Id)) return;
            mNpcDict[npc.Id] = npc;
        }

        public void Unregister(string id)
        {
            if (mNpcDict.ContainsKey(id)) mNpcDict.Remove(id);
        }

        public NPCEntity Get(string id)
        {
            return mNpcDict.TryGetValue(id, out var npc) ? npc : null;
        }

        #region Controller Management

        public void RegisterController(NPCController controller)
        {
            if (controller == null || string.IsNullOrEmpty(controller.NpcId)) return;
            mControllerDict[controller.NpcId] = controller;
        }

        public void UnregisterController(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (mControllerDict.ContainsKey(id)) mControllerDict.Remove(id);
        }

        public NPCController GetController(string id)
        {
            return mControllerDict.TryGetValue(id, out var controller) ? controller : null;
        }

        #endregion

        public IEnumerable<NPCEntity> GetAll()
        {
            return mNpcDict.Values;
        }

        private void Update()
        {
            // 可以在这里统一驱动所有 NPC 的 CommandQueue
            foreach (var npc in mNpcDict.Values)
            {
                // 简单的轮询：如果不为空则尝试处理下一条
                // 真正的执行逻辑可能需要更复杂的调度，比如分帧处理
                if (npc.CommandQueue.Count > 0)
                {
                    // 此处暂时同步处理，也可以改为异步控制
                    // 注意：CommandQueue.ProcessNext() 通常是非阻塞的，只负责发起
                    // 如果指令是异步的 (如移动)，它会启动但立即返回
                    // 具体行为取决于 mBrain.ExecutorRegistry 中的实现
                    npc.CommandQueue.ProcessNext(); 
                    // TODO: 暂时注释，等待 Unity 侧的 Executor 实现
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            mBrain?.Dispose();
        }
    }
}
