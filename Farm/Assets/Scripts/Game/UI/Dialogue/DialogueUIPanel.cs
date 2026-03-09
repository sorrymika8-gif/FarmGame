using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using Cysharp.Threading.Tasks;
using QFramework;
using FarmGame.Game.NPC;
using FarmGame.LLMCore.Brain;

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

    /// <summary>
    /// 正式对话面板
    /// 底部全宽布局，显示NPC表情、对话内容和输入框
    /// </summary>
    public class DialogueUIPanel : UIPanel
    {
        [Header("布局组件")]
        [Tooltip("NPC表情Image（显示差分表情图）")]
        public Image PortraitImage;
        
        [Tooltip("NPC名字文本")]
        public TextMeshProUGUI NameText;
        
        [Tooltip("对话内容文本")]
        public TextMeshProUGUI DialogueText;
        
        [Tooltip("输入框")]
        public TMP_InputField InputField;
        
        [Tooltip("发送按钮")]
        public Button SendButton;

        [Tooltip("关闭按钮")]
        public Button CloseButton;
        
        [Header("动画设置")]
        [Tooltip("表情切换淡入淡出时长")]
        public float PortraitFadeDuration = 0.2f;

        private NPCEntity mCurrentEntity;
        private DialogueSystem mDialogueSystem;
        private string mCurrentExpression;
        private bool mIsSwitchingPortrait;

        protected override void OnInit(IUIData uiData = null)
        {
            // 绑定按钮事件
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
            if (uiData is DialogueUIData data && data.Entity != null)
            {
                mCurrentEntity = data.Entity;
                
                // 设置NPC名字
                if (NameText != null)
                {
                    NameText.text = mCurrentEntity.Name;
                }
                
                // 清空对话内容
                if (DialogueText != null)
                {
                    DialogueText.text = "";
                }
                
                // 显示初始立绘
                UpdatePortrait(mCurrentEntity.CurrentExpression);
                
                // 订阅表情变更事件
                mCurrentEntity.OnExpressionChanged += HandleExpressionChanged;
                
                // 订阅对话框说话事件
                LLMCore.Brain.SpeakExecutor.OnDialogueSpeak += HandleDialogueSpeak;
                
                // 获取DialogueSystem引用
                var npcController = NPCManager.Instance?.GetController(mCurrentEntity.Id);
                if (npcController != null)
                {
                    mDialogueSystem = npcController.GetComponent<DialogueSystem>();
                }
            }
            
            // 清空输入框
            if (InputField != null) 
            {
                InputField.text = "";
            }
        }

        protected override void OnClose()
        {
            // 取消订阅事件
            if (mCurrentEntity != null)
            {
                mCurrentEntity.OnExpressionChanged -= HandleExpressionChanged;
            }
            LLMCore.Brain.SpeakExecutor.OnDialogueSpeak -= HandleDialogueSpeak;
            
            // 通知DialogueSystem对话结束
            if (mDialogueSystem != null)
            {
                mDialogueSystem.EndDialogue();
            }
            
            mCurrentEntity = null;
            mDialogueSystem = null;
            mCurrentExpression = null;
        }

        /// <summary>
        /// 处理对话框说话事件
        /// </summary>
        private void HandleDialogueSpeak(NPCEntity entity, string content)
        {
            // 只处理当前对话的NPC
            if (mCurrentEntity == null || entity.Id != mCurrentEntity.Id) return;
            
            // 显示对话内容
            if (DialogueText != null)
            {
                DialogueText.text = content;
            }
        }

        /// <summary>
        /// 处理表情变更事件
        /// </summary>
        private void HandleExpressionChanged(string oldExpression, string newExpression)
        {
            SwitchPortraitAsync(newExpression).Forget();
        }

        /// <summary>
        /// 更新表情显示（无动画）
        /// </summary>
        private void UpdatePortrait(string expression)
        {
            if (mCurrentEntity == null || PortraitImage == null) return;
            
            var sprite = PortraitService.Instance.GetPortraitSprite(mCurrentEntity.Id, expression);
            if (sprite != null)
            {
                PortraitImage.sprite = sprite;
                PortraitImage.enabled = true;
                mCurrentExpression = expression;
            }
            else
            {
                // 如果没有表情图，隐藏Image但不影响布局
                PortraitImage.enabled = false;
            }
        }

        /// <summary>
        /// 切换表情（带动画）
        /// </summary>
        private async UniTaskVoid SwitchPortraitAsync(string newExpression)
        {
            if (mCurrentEntity == null || PortraitImage == null) return;
            if (newExpression == mCurrentExpression) return;
            if (mIsSwitchingPortrait) return;
            
            mIsSwitchingPortrait = true;
            
            try
            {
                // 预加载新表情
                var newSprite = PortraitService.Instance.GetPortraitSprite(mCurrentEntity.Id, newExpression);
                if (newSprite == null) return;

                // 淡出
                if (PortraitFadeDuration > 0 && PortraitImage.enabled)
                {
                    await FadePortraitAsync(1f, 0f, PortraitFadeDuration / 2);
                }

                // 切换
                PortraitImage.sprite = newSprite;
                mCurrentExpression = newExpression;

                // 淡入
                if (PortraitFadeDuration > 0)
                {
                    PortraitImage.enabled = true;
                    await FadePortraitAsync(0f, 1f, PortraitFadeDuration / 2);
                }
                else
                {
                    PortraitImage.enabled = true;
                    SetPortraitAlpha(1f);
                }
            }
            finally
            {
                mIsSwitchingPortrait = false;
            }
        }

        private async UniTask FadePortraitAsync(float from, float to, float duration)
        {
            float elapsed = 0f;
            SetPortraitAlpha(from);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetPortraitAlpha(Mathf.Lerp(from, to, t));
                await UniTask.Yield();
            }

            SetPortraitAlpha(to);
        }

        private void SetPortraitAlpha(float alpha)
        {
            if (PortraitImage == null) return;
            var color = PortraitImage.color;
            color.a = alpha;
            PortraitImage.color = color;
        }

        private void OnSendClick()
        {
            if (string.IsNullOrWhiteSpace(InputField?.text)) return;
            if (mCurrentEntity == null) return;

            var content = InputField.text;
            
            // 发送给Entity处理
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
