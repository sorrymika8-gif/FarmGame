using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using FarmGame.GameLLM; 
using Cysharp.Threading.Tasks;
using QFramework;
using FarmGame.Game.NPC; // for NPCEntity

namespace FarmGame.UI
{
    public class DialogueUIData : UIPanelData
    {
        public NPCEntity Entity { get; }
        public DialogueUIData(NPCEntity entity)
        {
            Entity = entity;
        }
    }

    public class DialogueUIPanel : UIPanel
    {
        [Header("UI Components")]
        [Tooltip("拖拽你的 TMP_InputField 到这里")]
        public TMP_InputField InputField;
        
        [Tooltip("拖拽你的发送按钮到这里")]
        public Button SendButton;

        [Tooltip("拖拽你的关闭按钮到这里")]
        public Button CloseButton;

        private NPCEntity mCurrentEntity;

        protected override void OnInit(IUIData uiData = null)
        {
            // 绑定事件
            if (SendButton != null)
                SendButton.onClick.AddListener(OnSendClick);
            else
                Debug.LogWarning("[DialogueUIPanel] SendButton is not assigned!");
                
            if (CloseButton != null)
                CloseButton.onClick.AddListener(OnCloseClick);
            else
                Debug.LogWarning("[DialogueUIPanel] CloseButton is not assigned!");
        }

        protected override void OnOpen(IUIData uiData = null)
        {
            if (uiData is DialogueUIData data)
            {
                mCurrentEntity = data.Entity;
            }
            
            // 清空输入框
            if(InputField) InputField.text = "";
        }

        protected override void OnClose()
        {
            mCurrentEntity = null;
        }

        private void OnSendClick()
        {
            if (string.IsNullOrWhiteSpace(InputField.text)) return;
            if (mCurrentEntity == null) return;

            var content = InputField.text;
            
            // 发送给 Entity 处理
            mCurrentEntity.ReceiveChatAsync(content).Forget();
            
            // 清空输入
            InputField.text = "";
        }

        private void OnCloseClick()
        {
            CloseSelf();
        }
    }
}
