// ==========================================================
// 自动生成配置系统 - 类型解析器
// ==========================================================

using System;
using System.Globalization;

namespace FarmGame.GameConfig
{
    /// <summary>
    /// 字符串值 → C# 类型值解析器
    /// </summary>
    public static class TypeParser
    {
        /// <summary>
        /// 数组元素分隔符
        /// </summary>
        public static char ArraySeparator = ',';

        /// <summary>
        /// 解析字符串为指定类型的值
        /// </summary>
        /// <param name="value">字符串值</param>
        /// <param name="excelType">Excel 类型名称</param>
        /// <returns>解析后的值</returns>
        public static object Parse(string value, string excelType)
        {
            var normalizedType = SupportedTypes.NormalizeType(excelType);

            // 空值处理
            if (string.IsNullOrWhiteSpace(value))
            {
                return GetDefaultValue(normalizedType);
            }

            return normalizedType switch
            {
                "int" => ParseInt(value),
                "long" => ParseLong(value),
                "float" => ParseFloat(value),
                "double" => ParseDouble(value),
                "bool" => ParseBool(value),
                "string" => value,
                "int[]" => ParseIntArray(value),
                "long[]" => ParseLongArray(value),
                "float[]" => ParseFloatArray(value),
                "double[]" => ParseDoubleArray(value),
                "bool[]" => ParseBoolArray(value),
                "string[]" => ParseStringArray(value),
                _ => throw new NotSupportedException($"不支持的类型: {excelType}")
            };
        }

        /// <summary>
        /// 获取类型的默认值
        /// </summary>
        private static object GetDefaultValue(string normalizedType)
        {
            return normalizedType switch
            {
                "int" => 0,
                "long" => 0L,
                "float" => 0f,
                "double" => 0d,
                "bool" => false,
                "string" => "",
                "int[]" => Array.Empty<int>(),
                "long[]" => Array.Empty<long>(),
                "float[]" => Array.Empty<float>(),
                "double[]" => Array.Empty<double>(),
                "bool[]" => Array.Empty<bool>(),
                "string[]" => Array.Empty<string>(),
                _ => null
            };
        }

        #region 基础类型解析

        /// <summary>
        /// 解析整数
        /// </summary>
        public static int ParseInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            // 尝试解析浮点数后取整（兼容 Excel 数值格式）
            if (value.Contains('.'))
            {
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                {
                    return (int)d;
                }
            }

            if (int.TryParse(value.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            throw new FormatException($"无法将 '{value}' 解析为 int");
        }

        /// <summary>
        /// 解析长整数
        /// </summary>
        public static long ParseLong(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0L;
            }

            if (value.Contains('.'))
            {
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                {
                    return (long)d;
                }
            }

            if (long.TryParse(value.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            throw new FormatException($"无法将 '{value}' 解析为 long");
        }

        /// <summary>
        /// 解析浮点数
        /// </summary>
        public static float ParseFloat(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0f;
            }

            if (float.TryParse(value.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            throw new FormatException($"无法将 '{value}' 解析为 float");
        }

        /// <summary>
        /// 解析双精度浮点数
        /// </summary>
        public static double ParseDouble(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0d;
            }

            if (double.TryParse(value.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            throw new FormatException($"无法将 '{value}' 解析为 double");
        }

        /// <summary>
        /// 解析布尔值
        /// </summary>
        public static bool ParseBool(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var lower = value.Trim().ToLower();
            return lower switch
            {
                "true" => true,
                "1" => true,
                "yes" => true,
                "是" => true,
                "false" => false,
                "0" => false,
                "no" => false,
                "否" => false,
                "" => false,
                _ => throw new FormatException($"无法将 '{value}' 解析为 bool")
            };
        }

        #endregion

        #region 数组类型解析

        /// <summary>
        /// 解析整数数组
        /// </summary>
        public static int[] ParseIntArray(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<int>();
            }

            // 移除引号
            value = value.Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<int>();
            }

            var parts = value.Split(ArraySeparator);
            var result = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                result[i] = ParseInt(parts[i]);
            }
            return result;
        }

        /// <summary>
        /// 解析长整数数组
        /// </summary>
        public static long[] ParseLongArray(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<long>();
            }

            value = value.Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<long>();
            }

            var parts = value.Split(ArraySeparator);
            var result = new long[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                result[i] = ParseLong(parts[i]);
            }
            return result;
        }

        /// <summary>
        /// 解析浮点数数组
        /// </summary>
        public static float[] ParseFloatArray(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<float>();
            }

            value = value.Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<float>();
            }

            var parts = value.Split(ArraySeparator);
            var result = new float[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                result[i] = ParseFloat(parts[i]);
            }
            return result;
        }

        /// <summary>
        /// 解析双精度浮点数数组
        /// </summary>
        public static double[] ParseDoubleArray(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<double>();
            }

            value = value.Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<double>();
            }

            var parts = value.Split(ArraySeparator);
            var result = new double[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                result[i] = ParseDouble(parts[i]);
            }
            return result;
        }

        /// <summary>
        /// 解析布尔数组
        /// </summary>
        public static bool[] ParseBoolArray(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<bool>();
            }

            value = value.Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<bool>();
            }

            var parts = value.Split(ArraySeparator);
            var result = new bool[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                result[i] = ParseBool(parts[i]);
            }
            return result;
        }

        /// <summary>
        /// 解析字符串数组
        /// </summary>
        public static string[] ParseStringArray(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<string>();
            }

            value = value.Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<string>();
            }

            var parts = value.Split(ArraySeparator);
            var result = new string[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                result[i] = parts[i].Trim();
            }
            return result;
        }

        #endregion
    }
}
