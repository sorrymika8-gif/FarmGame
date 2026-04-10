## 天气管理器
## 负责天气系统的模拟和切换
extends Node

## 天气类型枚举
enum WeatherType {
	SUNNY = 0,
	CLOUDY = 1,
	RAINY = 2,
}

var _is_initialized: bool = false
var _current_weather: int = WeatherType.SUNNY
var _weather_timer: Timer = null
var _rain_timer: Timer = null

## 天气切换间隔（秒）
var weather_change_interval: float = 60.0
## 下雨时降雨间隔（秒）
var rain_interval: float = 5.0
## 每次降雨量
var rain_amount: float = 10.0

## 当前天气
var current_weather: int:
	get:
		return _current_weather

signal weather_changed(new_weather: int)
signal rain_loop(amount: float)

func initialize() -> void:
	if _is_initialized:
		return
	
	# 从配置读取天气参数
	var settings = ConfigManager.get_all("game_settings")
	for setting in settings:
		var key = setting.get("key", "")
		match key:
			"weather_change_interval":
				weather_change_interval = float(setting.get("value", 60.0))
			"rain_interval":
				rain_interval = float(setting.get("value", 5.0))
			"rain_amount":
				rain_amount = float(setting.get("value", 10.0))
	
	# 天气切换定时器
	_weather_timer = Timer.new()
	_weather_timer.wait_time = weather_change_interval
	_weather_timer.autostart = true
	_weather_timer.timeout.connect(_on_weather_timer)
	add_child(_weather_timer)
	
	# 降雨定时器
	_rain_timer = Timer.new()
	_rain_timer.wait_time = rain_interval
	_rain_timer.autostart = false
	_rain_timer.timeout.connect(_on_rain_timer)
	add_child(_rain_timer)
	
	_is_initialized = true
	print("[WeatherManager] 初始化完成, 当前天气: %s" % _weather_name(_current_weather))

## 设置天气
func set_weather(weather: int) -> void:
	if _current_weather == weather:
		return
	
	_current_weather = weather
	weather_changed.emit(_current_weather)
	
	# 下雨时启动降雨循环
	if _current_weather == WeatherType.RAINY:
		_rain_timer.start()
	else:
		_rain_timer.stop()
	
	print("[WeatherManager] 天气变化: %s" % _weather_name(_current_weather))

## 获取天气名称
func get_weather_name() -> String:
	return _weather_name(_current_weather)

# --- 私有方法 ---

func _on_weather_timer() -> void:
	# 随机切换天气
	var new_weather = randi() % 3
	set_weather(new_weather)

func _on_rain_timer() -> void:
	if _current_weather == WeatherType.RAINY:
		rain_loop.emit(rain_amount)

func _weather_name(weather: int) -> String:
	match weather:
		WeatherType.SUNNY:
			return "晴天"
		WeatherType.CLOUDY:
			return "多云"
		WeatherType.RAINY:
			return "雨天"
		_:
			return "未知"
