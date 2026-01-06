using UnityEngine;
using QFramework;

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

        /// <summary>
        /// 检查指定类型的面板是否已打开
        /// </summary>
        /// <typeparam name="T">面板类型</typeparam>
        /// <returns>是否已打开</returns>
        public bool IsPanelOpen<T>() where T : UIPanel
        {
            if (!ValidateInitialized()) return false;

            return UIKit.GetPanel<T>() != null;
        }

        /// <summary>
        /// 关闭所有已打开的 UI 面板
        /// </summary>
        public void CloseAllPanels()
        {
            if (!mIsInitialized) return;

            UIKit.CloseAllPanel();
        }

        /// <summary>
        /// 隐藏所有已打开的 UI 面板
        /// </summary>
        public void HideAllPanels()
        {
            if (!ValidateInitialized()) return;

            UIKit.HideAllPanel();
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

        #endregion
    }
}
