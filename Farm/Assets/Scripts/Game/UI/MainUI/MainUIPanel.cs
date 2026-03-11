using UnityEngine;
using UnityEngine.UI;
using QFramework;
using FarmGame.Player;
using FarmGame.Item;
using TMPro;
using FarmGame.Weather;

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
        #region 适配参数

        // 基准分辨率（设计时的参考分辨率）
        private const float BASE_WIDTH = 1280f;
        private const float BASE_HEIGHT = 720f;
        
        // 按钮基准尺寸
        private const float BASE_BUTTON_SIZE = 64f;
        private const float MIN_BUTTON_SIZE = 48f;
        private const float MAX_BUTTON_SIZE = 96f;
        
        // 边距基准值
        private const float BASE_MARGIN = 20f;
        private const float MIN_MARGIN = 10f;
        private const float MAX_MARGIN = 40f;

        #endregion

        #region 私有字段
        
		[SerializeField] private TextMeshProUGUI mWeatherText;
        [SerializeField] private Image mWeatherIcon;
        [SerializeField] private TextMeshProUGUI mEnvironmentText;
		
		[Header("Weather Icons")]
        public Sprite SunnyIcon;
        public Sprite RainyIcon;
        public Sprite CloudyIcon;


        private MainUIPanelData mData;
        
        [SerializeField]
        private Button mSaveButton;

        #endregion

        #region UIPanel生命周期

        protected override void OnInit(IUIData uiData)
        {
            mData = uiData as MainUIPanelData ?? new MainUIPanelData();
            
            // 应用屏幕适配
            ApplyScreenAdaptation();
            
            // 绑定背包按钮点击事件
            if (BackpackButton != null)
            {
                BackpackButton.onClick.RemoveAllListeners();
                BackpackButton.onClick.AddListener(OnBackpackButtonClick);
            }
            // 1. 初始显示当前状态
            UpdateWeatherUI(WeatherManager.Instance.CurrentWeather);

            // 2. 订阅天气变化事件
            WeatherManager.Instance.OnWeatherChanged += OnWeatherChanged;
            
            // 绑定存档按钮点击事件
            if (mSaveButton != null)
            {
                mSaveButton.onClick.RemoveAllListeners();
                mSaveButton.onClick.AddListener(OnSaveButtonClick);
            }
        }

        /// <summary>
        /// 应用屏幕适配
        /// </summary>
        private void ApplyScreenAdaptation()
        {
            // 获取Canvas的尺寸
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            var canvasRect = canvas.GetComponent<RectTransform>();
            float canvasWidth = canvasRect.rect.width;
            float canvasHeight = canvasRect.rect.height;

            // 如果Canvas尺寸无效，使用屏幕尺寸
            if (canvasWidth <= 0 || canvasHeight <= 0)
            {
                canvasWidth = Screen.width;
                canvasHeight = Screen.height;
            }

            // 计算缩放因子
            float scaleX = canvasWidth / BASE_WIDTH;
            float scaleY = canvasHeight / BASE_HEIGHT;
            float scaleFactor = Mathf.Min(scaleX, scaleY);

            // 计算适配后的按钮尺寸
            float buttonSize = Mathf.Clamp(BASE_BUTTON_SIZE * scaleFactor, MIN_BUTTON_SIZE, MAX_BUTTON_SIZE);
            float margin = Mathf.Clamp(BASE_MARGIN * scaleFactor, MIN_MARGIN, MAX_MARGIN);

            // 适配背包按钮
            AdaptButton(BackpackButton, buttonSize, margin, AnchorPosition.BottomRight);
            
            // 适配存档按钮（如果有的话，放在背包按钮上方）
            if (mSaveButton != null)
            {
                var saveButtonRect = mSaveButton.GetComponent<RectTransform>();
                if (saveButtonRect != null)
                {
                    saveButtonRect.sizeDelta = new Vector2(buttonSize, buttonSize);
                    // 放在背包按钮上方
                    saveButtonRect.anchorMin = new Vector2(1, 0);
                    saveButtonRect.anchorMax = new Vector2(1, 0);
                    saveButtonRect.pivot = new Vector2(1, 0);
                    saveButtonRect.anchoredPosition = new Vector2(-margin, margin + buttonSize + 10f);
                }
            }

            Debug.Log($"[MainUIPanel] 屏幕适配完成: Canvas({canvasWidth}x{canvasHeight}), 缩放因子:{scaleFactor:F2}, 按钮尺寸:{buttonSize:F0}, 边距:{margin:F0}");
        }

        /// <summary>
        /// 锚点位置枚举
        /// </summary>
        private enum AnchorPosition
        {
            TopLeft, TopRight, BottomLeft, BottomRight
        }

        /// <summary>
        /// 适配单个按钮
        /// </summary>
        private void AdaptButton(Button button, float size, float margin, AnchorPosition anchor)
        {
            if (button == null) return;

            var rectTransform = button.GetComponent<RectTransform>();
            if (rectTransform == null) return;

            // 设置尺寸
            rectTransform.sizeDelta = new Vector2(size, size);

            // 根据锚点位置设置
            switch (anchor)
            {
                case AnchorPosition.TopLeft:
                    rectTransform.anchorMin = new Vector2(0, 1);
                    rectTransform.anchorMax = new Vector2(0, 1);
                    rectTransform.pivot = new Vector2(0, 1);
                    rectTransform.anchoredPosition = new Vector2(margin, -margin);
                    break;
                case AnchorPosition.TopRight:
                    rectTransform.anchorMin = new Vector2(1, 1);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.pivot = new Vector2(1, 1);
                    rectTransform.anchoredPosition = new Vector2(-margin, -margin);
                    break;
                case AnchorPosition.BottomLeft:
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(0, 0);
                    rectTransform.pivot = new Vector2(0, 0);
                    rectTransform.anchoredPosition = new Vector2(margin, margin);
                    break;
                case AnchorPosition.BottomRight:
                    rectTransform.anchorMin = new Vector2(1, 0);
                    rectTransform.anchorMax = new Vector2(1, 0);
                    rectTransform.pivot = new Vector2(1, 0);
                    rectTransform.anchoredPosition = new Vector2(-margin, margin);
                    break;
            }
        }

        protected override void OnOpen(IUIData uiData)
        {
            // 主界面打开时的逻辑
        }

        protected override void OnShow()
        {
            // 主界面显示时的逻辑
            UpdateEnvironmentStats();
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

            if (WeatherManager.Instance != null)
            {
                WeatherManager.Instance.OnWeatherChanged -= OnWeatherChanged;
            }
            
            if (mSaveButton != null)
            {
                mSaveButton.onClick.RemoveAllListeners();
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
        
        /// <summary>
        /// 存档按钮点击事件
        /// </summary>
        private void OnSaveButtonClick()
        {
            // 打开存档面板（默认为保存模式）
            var uiManager = FarmGame.Core.UIManager.Instance;
            if (uiManager != null)
            {
                uiManager.OpenSaveLoadPanel(true);
            }
        }

        #endregion

        #region 天气系统相关

        private void OnWeatherChanged(WeatherType newWeather)
        {
            UpdateWeatherUI(newWeather);
        }
		private void UpdateWeatherUI(WeatherType weather)
        {
            // 更新文字
            switch (weather)
            {
                case WeatherType.Sunny:
                    mWeatherText.text = "天气：晴朗";
                    mWeatherIcon.sprite = SunnyIcon;
                    break;
                case WeatherType.Rainy:
                    mWeatherText.text = "天气：下雨";
                    mWeatherIcon.sprite = RainyIcon;
                    break;
                case WeatherType.Cloudy:
                    mWeatherText.text = "天气：多云";
                    mWeatherIcon.sprite = CloudyIcon;
                    break;
            }
        }

        private void UpdateEnvironmentStats()
        {
            if (mEnvironmentText != null)
            {
                // 从 WeatherManager 实时读取数据
                float temp = WeatherManager.Instance.CurrentTemperature;
                float hum = WeatherManager.Instance.CurrentHumidity;
                mEnvironmentText.text = $"温度: {temp:F1}°C  湿度: {hum:F1}%";
            }
        }

        #endregion
    }
}