using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace FarmGame.Game.NPC
{
    /// <summary>
    /// NPC 头顶气泡对话框
    /// 用于显示 NPC 说话内容，支持定时自动消失和手动关闭
    /// </summary>
    public class NPCBubble : MonoBehaviour
    {
        #region 序列化字段

        [Header("UI组件")]
        [Tooltip("气泡文本组件")]
        [SerializeField] private TextMeshProUGUI mText;

        [Tooltip("气泡背景图片")]
        [SerializeField] private Image mBackground;

        [Tooltip("用于淡入淡出的CanvasGroup")]
        [SerializeField] private CanvasGroup mCanvasGroup;

        [Header("配置")]
        [Tooltip("默认显示时长（秒），0表示不自动消失")]
        [SerializeField] private float mDefaultDuration = 4f;

        [Tooltip("淡入时长（秒）")]
        [SerializeField] private float mFadeInDuration = 0.2f;

        [Tooltip("淡出时长（秒）")]
        [SerializeField] private float mFadeOutDuration = 0.5f;

        [Tooltip("最大文本宽度")]
        [SerializeField] private float mMaxWidth = 200f;

        #endregion

        #region 私有字段

        private CancellationTokenSource mAutoHideCts;
        private bool mIsShowing;

        #endregion

        #region 公共属性

        /// <summary>
        /// 气泡是否正在显示
        /// </summary>
        public bool IsShowing => mIsShowing;

        #endregion

        #region 生命周期

        private void Awake()
        {
            // 初始状态隐藏
            if (mCanvasGroup != null)
            {
                mCanvasGroup.alpha = 0f;
            }
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            CancelAutoHide();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 显示气泡
        /// </summary>
        /// <param name="content">显示内容</param>
        /// <param name="duration">显示时长（秒），0表示不自动消失，-1使用默认值</param>
        public void Show(string content, float duration = -1f)
        {
            ShowWithMood(content, null, duration);
        }

        /// <summary>
        /// 显示带心情emoji的气泡
        /// </summary>
        /// <param name="content">显示内容</param>
        /// <param name="mood">心情emoji（显示在文本前面）</param>
        /// <param name="duration">显示时长（秒），0表示不自动消失，-1使用默认值</param>
        public void ShowWithMood(string content, string mood, float duration = -1f)
        {
            if (string.IsNullOrEmpty(content))
            {
                Debug.LogWarning("[NPCBubble] 显示内容为空");
                return;
            }

            // 取消之前的自动隐藏
            CancelAutoHide();

            // 构建带emoji的文本
            string displayText = content;
            if (!string.IsNullOrEmpty(mood))
            {
                displayText = $"{mood} {content}";
            }

            // 设置文本
            if (mText != null)
            {
                mText.text = displayText;
            }

            // 使用默认时长
            if (duration < 0)
            {
                duration = mDefaultDuration;
            }

            // 显示气泡
            gameObject.SetActive(true);
            mIsShowing = true;

            // 启动显示流程
            ShowAsync(duration).Forget();
        }

        /// <summary>
        /// 立即隐藏气泡
        /// </summary>
        public void Hide()
        {
            CancelAutoHide();
            
            if (mCanvasGroup != null)
            {
                mCanvasGroup.alpha = 0f;
            }
            
            gameObject.SetActive(false);
            mIsShowing = false;
        }

        /// <summary>
        /// 淡出隐藏气泡
        /// </summary>
        /// <param name="fadeTime">淡出时长，-1使用默认值</param>
        public void HideWithFade(float fadeTime = -1f)
        {
            if (!mIsShowing) return;

            if (fadeTime < 0)
            {
                fadeTime = mFadeOutDuration;
            }

            CancelAutoHide();
            FadeOutAsync(fadeTime).Forget();
        }

        /// <summary>
        /// 点击气泡时调用（可在Unity事件中绑定）
        /// </summary>
        public void OnBubbleClicked()
        {
            HideWithFade();
        }

        #endregion

        #region 私有方法

        private async UniTaskVoid ShowAsync(float duration)
        {
            mAutoHideCts = new CancellationTokenSource();
            var token = mAutoHideCts.Token;

            try
            {
                // 淡入
                await FadeInAsync(mFadeInDuration, token);

                // 如果设置了自动消失时长
                if (duration > 0)
                {
                    // 等待指定时长
                    await UniTask.Delay((int)(duration * 1000), cancellationToken: token);

                    // 淡出
                    await FadeOutInternalAsync(mFadeOutDuration, token);
                }
            }
            catch (System.OperationCanceledException)
            {
                // 被取消，正常情况
            }
        }

        private async UniTask FadeInAsync(float duration, CancellationToken token)
        {
            if (mCanvasGroup == null)
            {
                return;
            }

            float elapsed = 0f;
            mCanvasGroup.alpha = 0f;

            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();
                
                elapsed += Time.deltaTime;
                mCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                await UniTask.Yield(token);
            }

            mCanvasGroup.alpha = 1f;
        }

        private async UniTaskVoid FadeOutAsync(float duration)
        {
            mAutoHideCts = new CancellationTokenSource();
            var token = mAutoHideCts.Token;

            try
            {
                await FadeOutInternalAsync(duration, token);
            }
            catch (System.OperationCanceledException)
            {
                // 被取消，正常情况
            }
        }

        private async UniTask FadeOutInternalAsync(float duration, CancellationToken token)
        {
            if (mCanvasGroup == null)
            {
                Hide();
                return;
            }

            float elapsed = 0f;
            float startAlpha = mCanvasGroup.alpha;

            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();
                
                elapsed += Time.deltaTime;
                mCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
                await UniTask.Yield(token);
            }

            mCanvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            mIsShowing = false;
        }

        private void CancelAutoHide()
        {
            if (mAutoHideCts != null)
            {
                mAutoHideCts.Cancel();
                mAutoHideCts.Dispose();
                mAutoHideCts = null;
            }
        }

        #endregion
    }
}
