## 天气类型常量
## 对应 Unity 的 WeatherType
class_name WeatherTypes
extends RefCounted

const SUNNY = 0
const CLOUDY = 1
const RAINY = 2

static func get_name(weather_type: int) -> String:
	match weather_type:
		SUNNY:
			return "晴天"
		CLOUDY:
			return "多云"
		RAINY:
			return "雨天"
		_:
			return "未知"
