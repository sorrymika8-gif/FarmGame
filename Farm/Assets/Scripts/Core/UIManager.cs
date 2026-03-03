using UnityEngine;
using QFramework;
using FarmGame.UI;
using FarmGame.Item;
using FarmGame.Game.NPC;
using FarmGame.Shop;

namespace FarmGame.Core
{
    /// <summary>
    /// 通用 UI 管理器
    /// 封装 QFramework UIKit，提供 UI 面板的打开、关闭、显示、隐藏等基础管理接口
    /// </summary>
    public class UIManager : MonoSingleton<UIManager>
    {
        #region 私有字段

        private bool mIsInitialized;

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化 UI 管理器（需要在游戏启动时显式调用）
        /// </summary>
        public void Initialize()
        {
            if (mIsInitialized) return;

            // 确保 UIKit 根节点已创建
            var uiRoot = UIKit.Root;
            if (uiRoot == null)
            {
                Debug.LogError("[UIManager] Initialize failed: UIKit.Root is null");
                return;
            }

            mIsInitialized = true;
            Debug.Log("[UIManager] Initialized successfully");
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private void Dispose()
        {
            if (!mIsInitialized) return;

            // 关闭所有面板
            CloseAllPanels();

            mIsInitialized = false;
        }

        #endregion

        #region 公共接口 - 面板管理

        /// <summary>
        /// 打开指定类型的 UI 面板
        /// </summary>
        /// <typeparam name="T">面板类型（需继承 UIPanel）</typeparam>
        /// <param name="data">面板数据（可选）</param>
        /// <param name="level">UI 层级（默认 Common）</param>
        /// <returns>打开的面板实例，失败返回 null</returns>
        public T OpenPanel<T>(IUIData data = null, UILevel level = UILevel.Common) where T : UIPanel
        {
            if (!ValidateInitialized()) return null;

            var panel = UIKit.OpenPanel<T>(level, data);
            if (panel == null)
            {
                Debug.LogWarning($"[UIManager] OpenPanel failed: could not open panel '{typeof(T).Name}'");
            }

            return panel;
        }

        /// <summary>
        /// 打开指定类型的 UI 面板 (指定路径)
        /// </summary>
        /// <typeparam name="T">面板类型（需继承 UIPanel）</typeparam>
        /// <param name="path">UI Prefab 的完整路径（相对于 Resources 文件夹，不含扩展名）</param>
        /// <param name="data">面板数据（可选）</param>
        /// <param name="level">UI 层级（默认 Common）</param>
        /// <returns>打开的面板实例，失败返回 null</returns>
        public T OpenPanel<T>(string path, IUIData data = null, UILevel level = UILevel.Common) where T : UIPanel
        {
            Debug.Log($"[UIManager] OpenPanel<{typeof(T).Name}> with path: {path}");
            
            if (!ValidateInitialized())
            {
                Debug.LogError("[UIManager] OpenPanel failed: not initialized");
                return null;
            }

            // 直接使用 Resources.Load 加载 Prefab，绕过 ResKit
            var prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError($"[UIManager] OpenPanel failed: could not load prefab at path '{path}'");
                return null;
            }

            // 实例化
            var go = Object.Instantiate(prefab);
            var panel = go.GetComponent<T>();
            if (panel == null)
            {
                Debug.LogError($"[UIManager] OpenPanel failed: prefab '{path}' does not have component '{typeof(T).Name}'");
                Object.Destroy(go);
                return null;
            }

            // 设置到 UIKit 层级
            UIKit.Root.SetLevelOfPanel(level, panel);

            // 设置 RectTransform
            var rect = panel.transform as RectTransform;
            if (rect != null)
            {
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.anchoredPosition3D = Vector3.zero;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.localScale = Vector3.one;
            }

            // 设置名字
            go.name = typeof(T).Name;

            // 分配 Loader（重要：确保 CloseSelf 能正确工作）
            var panelInterface = panel as IPanel;
            var loader = UIKit.Config.PanelLoaderPool.AllocateLoader();
            panelInterface.Loader = loader;
            Debug.Log($"[UIManager] Loader allocated: {loader != null}, panelInterface.Loader: {panelInterface.Loader != null}");

            // 创建 PanelInfo 并注册到 UIKit Table
            panel.Info = PanelInfo.Allocate(go.name, level, data, typeof(T), null);
            UIKit.Table.Add(panel);
            Debug.Log($"[UIManager] Panel added to UIKit.Table, Info: {panel.Info != null}");

            // 初始化并打开
            panel.Init(data);
            panel.Open(data);
            
            Debug.Log($"[UIManager] OpenPanel<{typeof(T).Name}> completed successfully");

            return panel;
        }

        /// <summary>
        /// 关闭指定类型的 UI 面板
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        public void ClosePanel<T>() where T : UIPanel
        {
            if (!ValidateInitialized()) return;

            UIKit.ClosePanel<T>();
        }

        /// <summary>
        /// 显示指定类型的 UI 面板（仅当面板已打开时有效）
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        public void ShowPanel<T>() where T : UIPanel
        {
            if (!ValidateInitialized()) return;

            var panel = UIKit.GetPanel<T>();
            if (panel != null)
            {
                panel.Show();
            }
            else
            {
                Debug.LogWarning($"[UIManager] ShowPanel failed: panel '{typeof(T).Name}' is not opened");
            }
        }

        /// <summary>
        /// 隐藏指定类型的 UI 面板（不销毁，可再次显示）
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        public void HidePanel<T>() where T : UIPanel
        {
            if (!ValidateInitialized()) return;

            var panel = UIKit.GetPanel<T>();
            if (panel != null)
            {
                panel.Hide();
            }
            else
            {
                Debug.LogWarning($"[UIManager] HidePanel failed: panel '{typeof(T).Name}' is not opened");
            }
        }

        /// <summary>
        /// 获取已打开的 UI 面板实例
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        /// <returns>面板实例，未打开时返回 null</returns>
        public T GetPanel<T>() where T : UIPanel
        {
            if (!ValidateInitialized()) return null;

            return UIKit.GetPanel<T>();
        }

        #endregion

        #region 私有方法

        private bool ValidateInitialized()
        {
            if (!mIsInitialized)
            {
                Debug.LogError("[UIManager] Operation failed: UIManager is not initialized");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 关闭所有已打开的 UI 面板
        /// </summary>
        private void CloseAllPanels()
        {
            UIKit.CloseAllPanel();
        }

        #endregion

        #region 背包面板专用方法

        /// <summary>
        /// 检查背包面板是否已打开
        /// </summary>
        /// <returns>是否已打开</returns>
        public bool IsBackpackPanelOpen()
        {
            return GetPanel<BackpackPanel>() != null;
        }

        /// <summary>
        /// 打开背包面板
        /// </summary>
        /// <param name="inventory">背包组件</param>
        /// <returns>打开的背包面板实例</returns>
        public BackpackPanel OpenBackpackPanel(InventoryComponent inventory)
        {
            if (!ValidateInitialized()) return null;

            var data = new BackpackPanelData()
            {
                Inventory = inventory
            };

            return OpenPanel<BackpackPanel>("UI/BackpackUI/BackpackPanel", data);
        }

        /// <summary>
        /// 关闭背包面板
        /// </summary>
        public void CloseBackpackPanel()
        {
            ClosePanel<BackpackPanel>();
        }

        /// <summary>
        /// 获取已打开的背包面板
        /// </summary>
        /// <returns>背包面板实例</returns>
        public BackpackPanel GetBackpackPanel()
        {
            return GetPanel<BackpackPanel>();
        }

        #endregion

        #region 主界面专用方法

        /// <summary>
        /// 检查主界面是否已打开
        /// </summary>
        /// <returns>是否已打开</returns>
        public bool IsMainUIPanelOpen()
        {
            return GetPanel<MainUIPanel>() != null;
        }

        /// <summary>
        /// 打开主界面
        /// </summary>
        /// <returns>打开的主界面实例</returns>
        public MainUIPanel OpenMainUIPanel()
        {
            if (!ValidateInitialized()) return null;

            var data = new MainUIPanelData();
            return OpenPanel<MainUIPanel>("UI/MainUI/MainUIPanel", data);
        }

        /// <summary>
        /// 关闭主界面
        /// </summary>
        public void CloseMainUIPanel()
        {
            ClosePanel<MainUIPanel>();
        }

        /// <summary>
        /// 获取已打开的主界面
        /// </summary>
        /// <returns>主界面实例</returns>
        public MainUIPanel GetMainUIPanel()
        {
            return GetPanel<MainUIPanel>();
        }

        #endregion

        #region 对话面板专用方法

        /// <summary>
        /// 检查对话面板是否已打开
        /// </summary>
        /// <returns>是否已打开</returns>
        public bool IsDialoguePanelOpen()
        {
            return GetPanel<DialogueUIPanel>() != null;
        }

        /// <summary>
        /// 打开对话面板
        /// </summary>
        /// <param name="npcEntity">NPC实体</param>
        /// <returns>打开的对话面板实例</returns>
        public DialogueUIPanel OpenDialoguePanel(NPCEntity npcEntity)
        {
            if (!ValidateInitialized()) return null;

            var data = new DialogueUIData(npcEntity);
            return OpenPanel<DialogueUIPanel>(data);
        }

        /// <summary>
        /// 关闭对话面板
        /// </summary>
        public void CloseDialoguePanel()
        {
            ClosePanel<DialogueUIPanel>();
        }

        /// <summary>
        /// 获取已打开的对话面板
        /// </summary>
        /// <returns>对话面板实例</returns>
        public DialogueUIPanel GetDialoguePanel()
        {
            return GetPanel<DialogueUIPanel>();
        }

        #endregion

        #region 商店面板专用方法

        /// <summary>
        /// 检查商店面板是否已打开
        /// </summary>
        /// <returns>是否已打开</returns>
        public bool IsShopPanelOpen()
        {
            return GetPanel<ShopPanel>() != null;
        }

        /// <summary>
        /// 打开商店面板
        /// </summary>
        /// <param name="shopType">商店类型</param>
        /// <param name="inventory">玩家背包组件（可选）</param>
        /// <returns>打开的商店面板实例</returns>
        public ShopPanel OpenShopPanel(ShopType shopType, InventoryComponent inventory = null)
        {
            if (!ValidateInitialized()) return null;

            var data = new ShopPanelData()
            {
                ShopType = shopType,
                PlayerInventory = inventory
            };

            return OpenPanel<ShopPanel>("UI/ShopUI/ShopPanel", data);
        }

        /// <summary>
        /// 关闭商店面板
        /// </summary>
        public void CloseShopPanel()
        {
            ClosePanel<ShopPanel>();
        }

        /// <summary>
        /// 获取已打开的商店面板
        /// </summary>
        /// <returns>商店面板实例</returns>
        public ShopPanel GetShopPanel()
        {
            return GetPanel<ShopPanel>();
        }

        #endregion

        #region 存档面板专用方法

        /// <summary>
        /// 检查存档面板是否已打开
        /// </summary>
        /// <returns>是否已打开</returns>
        public bool IsSaveLoadPanelOpen()
        {
            return GetPanel<SaveLoadPanel>() != null;
        }

        /// <summary>
        /// 打开存档面板
        /// </summary>
        /// <param name="isSaveMode">是否为保存模式（默认true）</param>
        /// <returns>打开的存档面板实例</returns>
        public SaveLoadPanel OpenSaveLoadPanel(bool isSaveMode = true)
        {
            if (!ValidateInitialized()) return null;

            var data = new SaveLoadPanelData()
            {
                IsSaveMode = isSaveMode
            };

            return OpenPanel<SaveLoadPanel>("UI/SaveUI/SaveLoadPanel", data);
        }

        /// <summary>
        /// 关闭存档面板
        /// </summary>
        public void CloseSaveLoadPanel()
        {
            ClosePanel<SaveLoadPanel>();
        }

        /// <summary>
        /// 获取已打开的存档面板
        /// </summary>
        /// <returns>存档面板实例</returns>
        public SaveLoadPanel GetSaveLoadPanel()
        {
            return GetPanel<SaveLoadPanel>();
        }

        #endregion
    }
}
