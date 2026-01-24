using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;

namespace FarmGame.GameConfig.Editor
{
    /// <summary>
    /// 简易 Xlsx 读取器 (无需外部 DLL)
    /// 注意：仅支持读取第一个 Sheet，仅支持基础数据类型
    /// </summary>
    public static class XlsxReader
    {
        public static string[][] Read(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"[XlsxReader] File not found: {filePath}");
                return new string[0][];
            }

            try
            {
                // 读取文件到内存流，避免文件锁定
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var ms = new MemoryStream())
                {
                    fs.CopyTo(ms);
                    ms.Position = 0;

                    using (var archive = new ZipArchive(ms, ZipArchiveMode.Read))
                    {
                        var sharedStrings = ReadSharedStrings(archive);
                        return ReadWorksheet(archive, sharedStrings);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[XlsxReader] Failed to read {filePath}: {ex.Message}");
                return new string[0][];
            }
        }

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            var list = new List<string>();
            var entry = archive.GetEntry("xl/sharedStrings.xml");
            if (entry == null) 
            {
                // Try case insensitive find because sometimes it is capitalized? Standard says sharedStrings.xml
                return list;
            }

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
            Debug.Log($"[XlsxReader] Loaded {list.Count} shared strings.");
            return list;
        }

        private static string[][] ReadWorksheet(ZipArchive archive, List<string> sharedStrings)
        {
            ZipArchiveEntry entry = null;
            entry = archive.GetEntry("xl/worksheets/sheet1.xml");
            if (entry == null)
            {
                foreach (var e in archive.Entries)
                {
                    if (e.FullName.StartsWith("xl/worksheets/sheet") && e.FullName.EndsWith(".xml"))
                    {
                        entry = e;
                        break;
                    }
                }
            }

            if (entry == null) 
            {
                Debug.LogError("[XlsxReader] No worksheet found!");
                return new string[0][];
            }

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
                            
                            if (reader.IsEmptyElement)
                            {
                                // Empty row, commit immediately
                                CommitRow(rows, currentRow, currentRowIndex);
                                currentRowIndex++;
                            }
                        }
                        else if (localName == "c")
                        {
                            string r = reader.GetAttribute("r");
                            cellType = reader.GetAttribute("t");
                            currentColIndex = GetColIndex(r);
                            currentValContent = "";
                            
                            if (reader.IsEmptyElement)
                            {
                                // Empty cell, commit
                                currentRow[currentColIndex] = "";
                            }
                        }
                        else if (localName == "v")
                        {
                            inValue = true;
                        }
                        else if (localName == "t")
                        {
                            inInline = true;
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.Text)
                    {
                        if (inValue || inInline)
                        {
                            currentValContent += reader.Value;
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        string localName = reader.LocalName;

                        if (localName == "row")
                        {
                            CommitRow(rows, currentRow, currentRowIndex);
                            currentRowIndex++;
                        }
                        else if (localName == "c")
                        {
                            string finalValue = currentValContent;
                            
                            if (cellType == "s" && int.TryParse(finalValue, out int ssid))
                            {
                                if (ssid >= 0 && ssid < sharedStrings.Count) finalValue = sharedStrings[ssid];
                            }
                            else if (cellType == "b") // boolean
                            {
                                finalValue = (finalValue == "1") ? "TRUE" : "FALSE";
                            }
                            
                            currentRow[currentColIndex] = finalValue;
                        }
                        else if (localName == "v")
                        {
                            inValue = false;
                        }
                        else if (localName == "t")
                        {
                            inInline = false;
                        }
                    }
                }
            }
            
            // Fill nulls
            for(int i=0; i<rows.Count; i++)
            {
                if(rows[i] == null) rows[i] = new string[0];
            }

            return rows.ToArray();
        }
        
        private static void CommitRow(List<string[]> rows, Dictionary<int, string> currentRow, int rowIndex)
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

        private static void AddRow(List<string[]> rows, Dictionary<int, string> currentRow, int maxCol)
        {
             // Deprecated
        }

        private static int GetColIndex(string cellRef)
        {
            // A1 -> 0, B1 -> 1, AA1 -> 26
            // 去掉数字
            string colName = Regex.Match(cellRef, "[A-Z]+").Value;
            int sum = 0;
            for (int i = 0; i < colName.Length; i++)
            {
                sum *= 26;
                sum += (colName[i] - 'A' + 1);
            }
            return sum - 1;
        }
    }
}
