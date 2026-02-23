// ==========================================================
// MiniJSON - 轻量级 JSON 解析器
// 基于 MIT 协议开源
// ==========================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FarmGame.GameConfig
{
    /// <summary>
    /// 轻量级 JSON 序列化/反序列化工具
    /// </summary>
    public static class MiniJSON
    {
        /// <summary>
        /// 将 JSON 字符串解析为对象
        /// </summary>
        /// <param name="json">JSON 字符串</param>
        /// <returns>解析后的对象（Dictionary 或 List）</returns>
        public static object Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return Parser.Parse(json);
        }

        /// <summary>
        /// 将对象序列化为 JSON 字符串
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <returns>JSON 字符串</returns>
        public static string Serialize(object obj)
        {
            return Serializer.Serialize(obj);
        }

        #region Parser

        private sealed class Parser : IDisposable
        {
            private const string WORD_BREAK = "{}[],:\"";

            private StringReader mJson;

            private Parser(string jsonString)
            {
                mJson = new StringReader(jsonString);
            }

            public static object Parse(string jsonString)
            {
                using (var instance = new Parser(jsonString))
                {
                    return instance.ParseValue();
                }
            }

            public void Dispose()
            {
                mJson.Dispose();
                mJson = null;
            }

            private char PeekChar
            {
                get
                {
                    var peek = mJson.Peek();
                    return peek == -1 ? '\0' : Convert.ToChar(peek);
                }
            }

            private char NextChar => Convert.ToChar(mJson.Read());

            private string NextWord
            {
                get
                {
                    var word = new StringBuilder();

                    while (!IsWordBreak(PeekChar))
                    {
                        word.Append(NextChar);

                        if (mJson.Peek() == -1)
                        {
                            break;
                        }
                    }

                    return word.ToString();
                }
            }

            private TOKEN NextToken
            {
                get
                {
                    EatWhitespace();

                    if (mJson.Peek() == -1)
                    {
                        return TOKEN.NONE;
                    }

                    switch (PeekChar)
                    {
                        case '{':
                            return TOKEN.CURLY_OPEN;
                        case '}':
                            mJson.Read();
                            return TOKEN.CURLY_CLOSE;
                        case '[':
                            return TOKEN.SQUARED_OPEN;
                        case ']':
                            mJson.Read();
                            return TOKEN.SQUARED_CLOSE;
                        case ',':
                            mJson.Read();
                            return TOKEN.COMMA;
                        case '"':
                            return TOKEN.STRING;
                        case ':':
                            return TOKEN.COLON;
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                        case '-':
                            return TOKEN.NUMBER;
                    }

                    switch (NextWord)
                    {
                        case "false":
                            return TOKEN.FALSE;
                        case "true":
                            return TOKEN.TRUE;
                        case "null":
                            return TOKEN.NULL;
                    }

                    return TOKEN.NONE;
                }
            }

            private static bool IsWordBreak(char c)
            {
                return char.IsWhiteSpace(c) || WORD_BREAK.IndexOf(c) != -1;
            }

            private enum TOKEN
            {
                NONE,
                CURLY_OPEN,
                CURLY_CLOSE,
                SQUARED_OPEN,
                SQUARED_CLOSE,
                COLON,
                COMMA,
                STRING,
                NUMBER,
                TRUE,
                FALSE,
                NULL
            }

            private object ParseValue()
            {
                var nextToken = NextToken;
                return ParseByToken(nextToken);
            }

            private object ParseByToken(TOKEN token)
            {
                switch (token)
                {
                    case TOKEN.STRING:
                        return ParseString();
                    case TOKEN.NUMBER:
                        return ParseNumber();
                    case TOKEN.CURLY_OPEN:
                        return ParseObject();
                    case TOKEN.SQUARED_OPEN:
                        return ParseArray();
                    case TOKEN.TRUE:
                        return true;
                    case TOKEN.FALSE:
                        return false;
                    case TOKEN.NULL:
                        return null;
                    default:
                        return null;
                }
            }

            private Dictionary<string, object> ParseObject()
            {
                var table = new Dictionary<string, object>();

                // 跳过 '{'
                mJson.Read();

                while (true)
                {
                    var nextToken = NextToken;

                    switch (nextToken)
                    {
                        case TOKEN.NONE:
                            return null;
                        case TOKEN.COMMA:
                            continue;
                        case TOKEN.CURLY_CLOSE:
                            return table;
                        default:
                            // 解析键
                            string key = ParseString();
                            if (key == null)
                            {
                                return null;
                            }

                            // 跳过冒号
                            if (NextToken != TOKEN.COLON)
                            {
                                return null;
                            }
                            mJson.Read();

                            // 解析值
                            table[key] = ParseValue();
                            break;
                    }
                }
            }

            private List<object> ParseArray()
            {
                var array = new List<object>();

                // 跳过 '['
                mJson.Read();

                var parsing = true;
                while (parsing)
                {
                    var nextToken = NextToken;

                    switch (nextToken)
                    {
                        case TOKEN.NONE:
                            return null;
                        case TOKEN.COMMA:
                            continue;
                        case TOKEN.SQUARED_CLOSE:
                            parsing = false;
                            break;
                        default:
                            array.Add(ParseByToken(nextToken));
                            break;
                    }
                }

                return array;
            }

            private string ParseString()
            {
                var s = new StringBuilder();

                // 跳过 '"'
                mJson.Read();

                var parsing = true;
                while (parsing)
                {
                    if (mJson.Peek() == -1)
                    {
                        parsing = false;
                        break;
                    }

                    var c = NextChar;
                    switch (c)
                    {
                        case '"':
                            parsing = false;
                            break;
                        case '\\':
                            if (mJson.Peek() == -1)
                            {
                                parsing = false;
                                break;
                            }

                            c = NextChar;
                            switch (c)
                            {
                                case '"':
                                case '\\':
                                case '/':
                                    s.Append(c);
                                    break;
                                case 'b':
                                    s.Append('\b');
                                    break;
                                case 'f':
                                    s.Append('\f');
                                    break;
                                case 'n':
                                    s.Append('\n');
                                    break;
                                case 'r':
                                    s.Append('\r');
                                    break;
                                case 't':
                                    s.Append('\t');
                                    break;
                                case 'u':
                                    var hex = new char[4];
                                    for (int i = 0; i < 4; i++)
                                    {
                                        hex[i] = NextChar;
                                    }
                                    s.Append((char)Convert.ToInt32(new string(hex), 16));
                                    break;
                            }
                            break;
                        default:
                            s.Append(c);
                            break;
                    }
                }

                return s.ToString();
            }

            private object ParseNumber()
            {
                var number = NextWord;

                if (number.IndexOf('.') == -1 && number.IndexOf('E') == -1 && number.IndexOf('e') == -1)
                {
                    if (long.TryParse(number, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out long parsedLong))
                    {
                        // 如果值在 int 范围内，返回 int
                        if (parsedLong >= int.MinValue && parsedLong <= int.MaxValue)
                        {
                            return (int)parsedLong;
                        }
                        return parsedLong;
                    }
                }

                if (double.TryParse(number, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double parsedDouble))
                {
                    return parsedDouble;
                }

                return 0;
            }

            private void EatWhitespace()
            {
                while (char.IsWhiteSpace(PeekChar))
                {
                    mJson.Read();

                    if (mJson.Peek() == -1)
                    {
                        break;
                    }
                }
            }
        }

        #endregion

        #region Serializer

        private sealed class Serializer
        {
            private StringBuilder mBuilder;

            private Serializer()
            {
                mBuilder = new StringBuilder();
            }

            public static string Serialize(object obj)
            {
                var instance = new Serializer();
                instance.SerializeValue(obj);
                return instance.mBuilder.ToString();
            }

            private void SerializeValue(object value)
            {
                if (value == null)
                {
                    mBuilder.Append("null");
                }
                else if (value is string str)
                {
                    SerializeString(str);
                }
                else if (value is bool b)
                {
                    mBuilder.Append(b ? "true" : "false");
                }
                else if (value is IList list)
                {
                    SerializeArray(list);
                }
                else if (value is IDictionary dict)
                {
                    SerializeObject(dict);
                }
                else if (value is char c)
                {
                    SerializeString(new string(c, 1));
                }
                else
                {
                    SerializeOther(value);
                }
            }

            private void SerializeObject(IDictionary obj)
            {
                var first = true;
                mBuilder.Append('{');

                foreach (var key in obj.Keys)
                {
                    if (!first)
                    {
                        mBuilder.Append(',');
                    }

                    SerializeString(key.ToString());
                    mBuilder.Append(':');
                    SerializeValue(obj[key]);

                    first = false;
                }

                mBuilder.Append('}');
            }

            private void SerializeArray(IList array)
            {
                mBuilder.Append('[');

                var first = true;
                foreach (var item in array)
                {
                    if (!first)
                    {
                        mBuilder.Append(',');
                    }

                    SerializeValue(item);
                    first = false;
                }

                mBuilder.Append(']');
            }

            private void SerializeString(string str)
            {
                mBuilder.Append('\"');

                foreach (var c in str)
                {
                    switch (c)
                    {
                        case '"':
                            mBuilder.Append("\\\"");
                            break;
                        case '\\':
                            mBuilder.Append("\\\\");
                            break;
                        case '\b':
                            mBuilder.Append("\\b");
                            break;
                        case '\f':
                            mBuilder.Append("\\f");
                            break;
                        case '\n':
                            mBuilder.Append("\\n");
                            break;
                        case '\r':
                            mBuilder.Append("\\r");
                            break;
                        case '\t':
                            mBuilder.Append("\\t");
                            break;
                        default:
                            var codepoint = Convert.ToInt32(c);
                            if (codepoint >= 32 && codepoint <= 126)
                            {
                                mBuilder.Append(c);
                            }
                            else
                            {
                                mBuilder.Append("\\u");
                                mBuilder.Append(codepoint.ToString("x4"));
                            }
                            break;
                    }
                }

                mBuilder.Append('\"');
            }

            private void SerializeOther(object value)
            {
                if (value is float f)
                {
                    mBuilder.Append(f.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
                }
                else if (value is int || value is uint || value is long || value is sbyte ||
                         value is byte || value is short || value is ushort || value is ulong)
                {
                    mBuilder.Append(value);
                }
                else if (value is double || value is decimal)
                {
                    mBuilder.Append(Convert.ToDouble(value).ToString("R",
                        System.Globalization.CultureInfo.InvariantCulture));
                }
                else
                {
                    SerializeString(value.ToString());
                }
            }
        }

        #endregion
    }
}
