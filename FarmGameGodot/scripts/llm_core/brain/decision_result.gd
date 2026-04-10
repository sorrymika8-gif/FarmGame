## 决策结果
class_name DecisionResult
extends RefCounted

## 是否成功
var success: bool = false

## 错误信息
var error_message: String = ""

## 解析出的指令列表 (Array of Dictionary)
var commands: Array = []

## LLM 原始输出
var raw_output: String = ""

## 处理时间（秒）
var processing_time: float = 0.0
