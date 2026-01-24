#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using FarmGame.GameConfig.Editor;
using System.IO;

public static class XlsxDebug
{
    [MenuItem("Tools/FarmGame/Debug Xlsx Reader")]
    public static void DebugGameSettings()
    {
        string path = "Assets/Configs/game_settings.xlsx";
        if (!File.Exists(path))
        {
            Debug.LogError("File not found: " + path);
            return;
        }

        var rows = XlsxReader.Read(path);
        Debug.Log($"Read {rows.Length} rows from {path}");

        for (int i = 0; i < Mathf.Min(rows.Length, 10); i++)
        {
            string rowStr = string.Join(" | ", rows[i]);
            Debug.Log($"Row {i}: {rowStr}");
        }
    }
}
#endif
