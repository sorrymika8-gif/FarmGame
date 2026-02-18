using UnityEngine;
using UnityEngine.UI;
using QFramework;
using FarmGame.Player;
using FarmGame.Item;

namespace FarmGame.UI
{
    /// <summary>
    /// 主界面UI面板数据
    /// </summary>
    public class MainUIPanelData : UIPanelData
    {
        // 可以添加主界面特定的数据
    }

    /// <summary>
    /// 主界面UI面板
    /// 显示游戏主界面的功能按钮（背包、设置等）
    /// </summary>
    public partial class MainUIPanel : UIPanel
    {
        #region 私有字段

        private MainUIPanelData mData;

        #endregion

        #region UIPanel生命周期

        protected override void OnInit(IUIData uiData)
        {
            mData = uiData as MainUIPanelData ?? new MainUIPanelData();
            
            // 绑定背包按钮点击事件
            if (BackpackButton != null)
            {
                BackpackButton.onClick.RemoveAllListeners();
                BackpackButton.onClick.AddListener(OnBackpackButtonClick);
            }
        }

        protected override void OnOpen(IUIData uiData)
        {
            // 主界面打开时的逻辑
        }

        protected override void OnShow()
        {
            // 主界面显示时的逻辑
        }

        protected override void OnHide()
        {
            // 主界面隐藏时的逻辑
        }

        protected override void OnClose()
        {
            // 清理按钮事件监听
            if (BackpackButton != null)
            {
                BackpackButton.onClick.RemoveAllListeners();
            }
        }

        #endregion

        #region 按钮事件处理

        /// <summary>
        /// 背包按钮点击事件
        /// </summary>
        private void OnBackpackButtonClick()
        {
            // 打开背包面板
            var uiManager = FarmGame.Core.UIManager.Instance;
            if (uiManager != null)
            {
                // 获取玩家背包并打开背包界面
                var player = PlayerManager.Instance?.Player;
                if (player != null && player.Inventory != null)
                {
                    uiManager.OpenBackpackPanel(player.Inventory);
                }
                else
                {
                    Debug.LogWarning("无法获取玩家背包数据，玩家或背包组件为空");
                }
            }
        }

        #endregion
    }
}