using UnityEngine;
using QFramework;

namespace FarmGame.UI
{
    /// <summary>
    /// 存档/加载面板数据
    /// </summary>
    public class SaveLoadPanelData : UIPanelData
    {
        /// <summary>
        /// 面板模式：true为保存模式，false为加载模式
        /// </summary>
        public bool IsSaveMode { get; set; } = true;
    }

    /// <summary>
    /// 存档/加载UI面板（简化版本）
    /// </summary>
    public class SaveLoadPanel : UIPanel
    {
        private SaveLoadPanelData mData;
        
        protected override void OnInit(IUIData uiData = null)
        {
            mData = uiData as SaveLoadPanelData ?? new SaveLoadPanelData();
        }
        
        protected override void OnOpen(IUIData uiData = null)
        {
            Debug.Log($"SaveLoadPanel opened in {(mData.IsSaveMode ? "Save" : "Load")} mode");
        }
        
        protected override void OnClose()
        {
            Debug.Log("SaveLoadPanel closed");
        }
    }
}