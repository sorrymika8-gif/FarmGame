using UnityEngine;
using UnityEngine.UI;
using QFramework;
using FarmGame.Weather;
using TMPro;

namespace QFramework.Example
{
	public class MainUIPanelData : UIPanelData
	{
	}
	public partial class MainUIPanel : UIPanel
	{

		[SerializeField] private TextMeshProUGUI mWeatherText;
        [SerializeField] private Image mWeatherIcon;
        [SerializeField] private TextMeshProUGUI mEnvironmentText;
		
		[Header("Weather Icons")]
        public Sprite SunnyIcon;
        public Sprite RainyIcon;
        public Sprite CloudyIcon;
		
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as MainUIPanelData ?? new MainUIPanelData();
			// please add init code here
		}
		
		protected override void OnOpen(IUIData uiData = null)
		{
			// 1. 初始显示当前状态
            UpdateWeatherUI(WeatherManager.Instance.CurrentWeather);

            // 2. 订阅天气变化事件
            WeatherManager.Instance.OnWeatherChanged += OnWeatherChanged;
		}
		
		protected override void OnShow()
		{
			UpdateEnvironmentStats();
		}
		
		protected override void OnHide()
		{
		}
		
		protected override void OnClose()
		{
			// 务必取消订阅，防止内存泄漏
            if (WeatherManager.Instance != null)
            {
                WeatherManager.Instance.OnWeatherChanged -= OnWeatherChanged;
            }
		}
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
	}
}
