using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FarmGame.Farm.Editor
{
    /// <summary>
    /// 牧草配置资源生成器
    /// 根据现有 pasture 资源生成 Tile 并自动配置 FarmTileSet
    /// </summary>
    public class PastureConfigGenerator : EditorWindow
    {
        private const string TilePath = "Assets/Resources/Tiles/Farm";
        private const string PlantTilePath = "Assets/Resources/Tiles/Farm/Plants";
        private const string PlantSpritePath = "Assets/Resources/prefabs/plants/pasture.png";
        private const string FarmTileSetPath = "Assets/ScriptableObjects/Farm/FarmTileSet.asset";

        private const int PasturePlantConfigId = 10000;
        private const string PasturePlantName = "牧草";

        [MenuItem("FarmGame/Farm/Generate Pasture Config")]
        public static void ShowWindow()
        {
            GetWindow<PastureConfigGenerator>("牧草配置生成器");
        }

        private void OnGUI()
        {
            GUILayout.Label("牧草配置资源生成器", EditorStyles.boldLabel);
            GUILayout.Space(8);
            EditorGUILayout.HelpBox("将生成仅牧草(PlantConfigId=10000)的 Tile 配置，并写入 FarmTileSet。", MessageType.Info);
            GUILayout.Space(10);

            if (GUILayout.Button("生成牧草配置资源", GUILayout.Height(36)))
            {
                GeneratePastureConfig();
            }
        }

        /// <summary>
        /// 一键生成牧草配置
        /// </summary>
        private static void GeneratePastureConfig()
        {
            EnsureDirectories();

            var pastureSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PlantSpritePath);
            if (pastureSprite == null)
            {
                Debug.LogError($"[PastureConfigGenerator] 未找到牧草资源: {PlantSpritePath}");
                EditorUtility.DisplayDialog("生成失败", "未找到 pasture.png 资源，请检查路径。", "确定");
                return;
            }

            // 根据 seed.xlsx 中 maturity_stage=[50,100] 生成 2 阶段
            var stage0 = CreateOrUpdateTile($"{PlantTilePath}/Tile_{PasturePlantName}_0.asset", pastureSprite);
            var stage1 = CreateOrUpdateTile($"{PlantTilePath}/Tile_{PasturePlantName}_1.asset", pastureSprite);

            var tileSet = AssetDatabase.LoadAssetAtPath<FarmTileSet>(FarmTileSetPath);
            if (tileSet == null)
            {
                Debug.LogError($"[PastureConfigGenerator] 未找到 FarmTileSet: {FarmTileSetPath}");
                EditorUtility.DisplayDialog("生成失败", "未找到 FarmTileSet.asset，请先创建该资源。", "确定");
                return;
            }

            tileSet.untilledTile = AssetDatabase.LoadAssetAtPath<Tile>($"{TilePath}/Tile_Untilled.asset");
            tileSet.tilledTile = AssetDatabase.LoadAssetAtPath<Tile>($"{TilePath}/Tile_Tilled.asset");
            tileSet.highlightTile = AssetDatabase.LoadAssetAtPath<Tile>($"{TilePath}/Tile_Highlight.asset");

            tileSet.plantTileConfigs.Clear();
            tileSet.plantTileConfigs.Add(new PlantTileStages
            {
                plantConfigId = PasturePlantConfigId,
                plantName = PasturePlantName,
                stageTiles = new Tile[] { stage0, stage1 }
            });

            EditorUtility.SetDirty(tileSet);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = tileSet;
            Debug.Log("[PastureConfigGenerator] 牧草配置资源生成完成。FarmTileSet 已更新为仅牧草。");
            EditorUtility.DisplayDialog("完成", "牧草配置资源已生成并写入 FarmTileSet。", "确定");
        }

        /// <summary>
        /// 创建目录
        /// </summary>
        private static void EnsureDirectories()
        {
            if (!Directory.Exists(TilePath))
            {
                Directory.CreateDirectory(TilePath);
            }

            if (!Directory.Exists(PlantTilePath))
            {
                Directory.CreateDirectory(PlantTilePath);
            }
        }

        /// <summary>
        /// 创建或更新 Tile 资源
        /// </summary>
        private static Tile CreateOrUpdateTile(string assetPath, Sprite sprite)
        {
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(assetPath);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                tile.color = Color.white;
                AssetDatabase.CreateAsset(tile, assetPath);
            }
            else
            {
                tile.sprite = sprite;
                tile.color = Color.white;
                EditorUtility.SetDirty(tile);
            }

            return tile;
        }
    }
}
