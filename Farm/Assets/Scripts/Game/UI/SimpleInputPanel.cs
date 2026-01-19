using UnityEngine;
using UnityEngine.UI;
using TMPro; // 使用 TextMeshPro
using FarmGame.GameLLM; // 引用 LLMService
using Cysharp.Threading.Tasks;

namespace FarmGame.UI
{
    public class SimpleInputPanel : MonoBehaviour
    {
        [Header("UI Components")]
        [Tooltip("拖拽你的 TMP_InputField 到这里")]
        public TMP_InputField InputField;
        
        [Tooltip("拖拽你的发送按钮到这里")]
        public Button SendButton;

        void Start()
        {
            // 绑定按钮点击事件
            if (SendButton != null)
            {
                SendButton.onClick.AddListener(OnSendClicked);
            }

            // 也可以监听输入框的回车键
            if (InputField != null)
            {
                InputField.onSubmit.AddListener(OnSubmit);
            }
        }

        private void OnSendClicked()
        {
            if (InputField == null) return;
            SendToLLM(InputField.text);
        }

        private void OnSubmit(string text)
        {
            SendToLLM(text);
        }

        private async void SendToLLM(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            Debug.Log($"[InputPanel] User sent: {text}");

            // 清空输入框
            InputField.text = "";
            
            // 锁定输入
            if (InputField) InputField.interactable = false;
            if (SendButton) SendButton.interactable = false;

            try 
            {
                if (LLMService.Instance == null || LLMService.Client == null)
                {
                    Debug.LogError("[InputPanel] LLMService not initialized!");
                    return;
                }

                Debug.Log("[InputPanel] Sending request to LLM...");

                // 构造请求
                var request = new LLMRequest();
                request.AddUser(text);

                // 发送并等待结果（增加超时，DeepSeek 有时较慢）
                var cts = new System.Threading.CancellationTokenSource();
                cts.CancelAfterSlim(System.TimeSpan.FromSeconds(60)); // 60秒超时

                var response = await LLMService.Client.SendAsync(request, cts.Token);

                if (response.Success)
                {
                    Debug.Log($"[InputPanel] Response received from LLM:\n{response.Content}"); 
                }
                else
                {
                    Debug.LogError($"[InputPanel] LLM Error: {response.ErrorMessage}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error sending to LLM: {e.Message}");
            }
            finally
            {
                // 恢复输入
                if (InputField) 
                {
                    InputField.interactable = true;
                    InputField.ActivateInputField(); 
                }
                if (SendButton) SendButton.interactable = true;
            }
        }

        void OnDestroy()
        {
            if (SendButton != null) SendButton.onClick.RemoveListener(OnSendClicked);
            if (InputField != null) InputField.onSubmit.RemoveListener(OnSubmit);
        }
    }
}
