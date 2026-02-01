using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;

namespace FarmGame.GameConfig
{
    /// <summary>
    /// Runtime Xlsx 读取器
    /// </summary>
    public class RuntimeXlsxReader : IExcelReader
    {
        private string[][] mData;
        private int mRowCount;
        private int mColumnCount;

        public RuntimeXlsxReader()
        {
            mData = Array.Empty<string[]>();
        }

        public void Open(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"[RuntimeXlsxReader] File not found: {filePath}");
                return;
            }

            try
            {
                // 读取文件到内存流
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var ms = new MemoryStream())
                {
                    fs.CopyTo(ms);
                    ms.Position = 0;

                    using (var archive = new ZipArchive(ms, ZipArchiveMode.Read))
                    {
                        var sharedStrings = ReadSharedStrings(archive);
                        mData = ReadWorksheet(archive, sharedStrings);
                        
                        mRowCount = mData.Length;
                        if (mRowCount > 0)
                        {
                            foreach (var row in mData)
                            {
                                if (row != null && row.Length > mColumnCount)
                                    mColumnCount = row.Length;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RuntimeXlsxReader] Failed to read {filePath}: {ex}");
            }
        }

        private List<string> ReadSharedStrings(ZipArchive archive)
        {
            var list = new List<string>();
            var entry = archive.GetEntry("xl/sharedStrings.xml");
            if (entry == null) return list;

            using (var stream = entry.Open())
            using (var reader = XmlReader.Create(stream))
            {
                bool inSi = false;
                bool inT = false; 
                string currentContent = "";

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.LocalName == "si")
                        {
                            inSi = true;
                            currentContent = "";
                        }
                        else if (reader.LocalName == "t" && inSi)
                        {
                            inT = true;
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.Text && inT && inSi)
                    {
                        currentContent += reader.Value;
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (reader.LocalName == "si")
                        {
                            list.Add(currentContent);
                            inSi = false;
                        }
                        else if (reader.LocalName == "t")
                        {
                            inT = false;
                        }
                    }
                }
            }
            return list;
        }

        private string[][] ReadWorksheet(ZipArchive archive, List<string> sharedStrings)
        {
            ZipArchiveEntry entry = null;
            // 简单逻辑：尝试找到第一个非空的 sheet
            foreach (var e in archive.Entries)
            {
                if (e.FullName.StartsWith("xl/worksheets/sheet") && e.FullName.EndsWith(".xml"))
                {
                    entry = e;
                    break;
                }
            }

            if (entry == null) return new string[0][];

            var rows = new List<string[]>();
            var currentRow = new Dictionary<int, string>();
            int currentRowIndex = 0;

            bool inValue = false;
            bool inInline = false;
            string currentValContent = "";
            string cellType = "";
            int currentColIndex = 0;

            using (var stream = entry.Open())
            using (var reader = XmlReader.Create(stream))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        string localName = reader.LocalName;

                        if (localName == "row")
                        {
                            currentRow.Clear();
                            string r = reader.GetAttribute("r");
                            if (int.TryParse(r, out int rVal)) currentRowIndex = rVal - 1;
                        }
                        else if (localName == "c")
                        {
                            string r = reader.GetAttribute("r");
                            cellType = reader.GetAttribute("t");
                            currentColIndex = GetColIndex(r);
                            currentValContent = "";
                        }
                        else if (localName == "v") { inValue = true; }
                        else if (localName == "t") { inInline = true; }
                    }
                    else if (reader.NodeType == XmlNodeType.Text)
                    {
                        if (inValue || inInline) currentValContent += reader.Value;
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (reader.LocalName == "row")
                        {
                            CommitRow(rows, currentRow, currentRowIndex);
                            currentRowIndex++;
                        }
                        else if (reader.LocalName == "c")
                        {
                            string finalValue = currentValContent;
                            if (cellType == "s" && int.TryParse(finalValue, out int ssid))
                            {
                                if (ssid >= 0 && ssid < sharedStrings.Count) finalValue = sharedStrings[ssid];
                            }
                            else if (cellType == "b")
                            {
                                finalValue = (finalValue == "1") ? "TRUE" : "FALSE";
                            }
                            currentRow[currentColIndex] = finalValue;
                        }
                        else if (reader.LocalName == "v") { inValue = false; }
                        else if (reader.LocalName == "t") { inInline = false; }
                    }
                }
            }
            return rows.ToArray();
        }

        private void CommitRow(List<string[]> rows, Dictionary<int, string> currentRow, int rowIndex)
        {
            while (rows.Count <= rowIndex) rows.Add(null);
            
            int maxCol = -1;
            foreach(var k in currentRow.Keys) if(k > maxCol) maxCol = k;
            
            if (maxCol == -1)
            {
                rows[rowIndex] = new string[0];
            }
            else
            {
                string[] rowData = new string[maxCol + 1];
                for (int i = 0; i <= maxCol; i++) rowData[i] = currentRow.TryGetValue(i, out var val) ? val : "";
                rows[rowIndex] = rowData;
            }
        }

        private int GetColIndex(string cellRef)
        {
            string colName = Regex.Match(cellRef, "[A-Z]+").Value;
            int sum = 0;
            for (int i = 0; i < colName.Length; i++)
            {
                sum *= 26;
                sum += (colName[i] - 'A' + 1);
            }
            return sum - 1;
        }

        public void Close() { mData = Array.Empty<string[]>(); }
        public void Dispose() { Close(); }
        public string GetCellValue(int row, int col) 
        {
            if (row < 0 || row >= mRowCount) return null;
            if (col < 0 || col >= mData[row].Length) return null;
            return mData[row][col];
        }
        public int GetRowCount() => mRowCount;
        public int GetColumnCount() => mColumnCount;
        
        // 占位实现
        public bool HasSheet(string sheetName) => true;
        public void SetActiveSheet(string sheetName) { }
        public void SetActiveSheet(int sheetIndex) { }
        public IEnumerable<string> GetSheetNames() => new string[] { "Sheet1" };
    }

    public class RuntimeXlsxReaderFactory : IExcelReaderFactory
    {
        public IExcelReader Create() => new RuntimeXlsxReader();
    }
}
