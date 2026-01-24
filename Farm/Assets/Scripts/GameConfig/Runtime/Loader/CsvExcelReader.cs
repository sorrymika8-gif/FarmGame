// ==========================================================
// 自动生成配置系统 - CSV 文件读取器
// 轻量级实现，不依赖外部库
// ==========================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FarmGame.GameConfig
{
    /// <summary>
    /// CSV 文件读取器
    /// 支持标准 CSV 格式（逗号分隔，双引号转义）
    /// </summary>
    public class CsvExcelReader : IExcelReader
    {
        private string[][] mData;
        private string mFilePath;
        private int mRowCount;
        private int mColumnCount;

        public CsvExcelReader()
        {
            mData = Array.Empty<string[]>();
        }

        /// <summary>
        /// 打开 CSV 文件
        /// </summary>
        public void Open(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"CSV 文件不存在: {filePath}");
            }

            mFilePath = filePath;
            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            var rows = new List<string[]>();

            foreach (var line in lines)
            {
                var cells = ParseCsvLine(line);
                rows.Add(cells);

                if (cells.Length > mColumnCount)
                {
                    mColumnCount = cells.Length;
                }
            }

            mData = rows.ToArray();
            mRowCount = mData.Length;
        }

        /// <summary>
        /// 关闭文件
        /// </summary>
        public void Close()
        {
            mData = Array.Empty<string[]>();
            mRowCount = 0;
            mColumnCount = 0;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// 获取单元格值
        /// </summary>
        public string GetCellValue(int row, int col)
        {
            if (row < 0 || row >= mRowCount)
            {
                return null;
            }

            var rowData = mData[row];
            if (col < 0 || col >= rowData.Length)
            {
                return null;
            }

            return rowData[col];
        }

        /// <summary>
        /// 获取总行数
        /// </summary>
        public int GetRowCount()
        {
            return mRowCount;
        }

        /// <summary>
        /// 获取总列数
        /// </summary>
        public int GetColumnCount()
        {
            return mColumnCount;
        }

        /// <summary>
        /// 检查工作表是否存在（CSV 只有一个工作表）
        /// </summary>
        public bool HasSheet(string sheetName)
        {
            return true;
        }

        /// <summary>
        /// 设置当前工作表（CSV 忽略此操作）
        /// </summary>
        public void SetActiveSheet(string sheetName)
        {
            // CSV 只有一个工作表，忽略
        }

        /// <summary>
        /// 设置当前工作表（CSV 忽略此操作）
        /// </summary>
        public void SetActiveSheet(int sheetIndex)
        {
            // CSV 只有一个工作表，忽略
        }

        /// <summary>
        /// 获取所有工作表名称
        /// </summary>
        public IEnumerable<string> GetSheetNames()
        {
            yield return Path.GetFileNameWithoutExtension(mFilePath ?? "Sheet1");
        }

        /// <summary>
        /// 解析 CSV 行（支持引号转义）
        /// </summary>
        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;
            var i = 0;

            while (i < line.Length)
            {
                var c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        // 检查是否是转义的引号 ""
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current.Append('"');
                            i += 2;
                            continue;
                        }
                        else
                        {
                            // 结束引号
                            inQuotes = false;
                            i++;
                            continue;
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        // 开始引号
                        inQuotes = true;
                    }
                    else if (c == ',')
                    {
                        // 字段分隔符
                        result.Add(current.ToString());
                        current.Clear();
                    }
                    else
                    {
                        current.Append(c);
                    }
                }

                i++;
            }

            // 添加最后一个字段
            result.Add(current.ToString());

            return result.ToArray();
        }
    }

    /// <summary>
    /// CSV 读取器工厂
    /// </summary>
    public class CsvExcelReaderFactory : IExcelReaderFactory
    {
        public IExcelReader Create()
        {
            return new CsvExcelReader();
        }
    }
}
