#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using FarmGame.Combat.Entity;

namespace FarmGame.Combat.Test
{
    /// <summary>
    /// 战斗预制体生成器
    /// 用于在编辑器中一键生成 SkillEntity 和 CharEntity 预制体
    /// </summary>
    public static class CombatPrefabGenerator
    {
        private const string PREFAB_PATH = "Assets/Resources/Combat";

        [MenuItem("FarmGame/Combat/生成战斗预制体", false, 100)]
        public static void GenerateAllPrefabs()
        {
            EnsureDirectoryExists();
            GenerateSkillEntityPrefab();
            GenerateCharEntityPrefab();
            AssetDatabase.Refresh();
            Debug.Log("[CombatPrefabGenerator] 所有预制体已生成完毕！");
        }

        [MenuItem("FarmGame/Combat/生成 SkillEntity 预制体", false, 101)]
        public static void GenerateSkillEntityPrefab()
        {
            EnsureDirectoryExists();

            // 创建 GameObject
            var go = new GameObject("SkillEntity");

            // 添加 SpriteRenderer
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateProjectileSprite();
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 10;

            // 添加 Rigidbody2D
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // 添加 CircleCollider2D
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.25f;
            col.isTrigger = true;

            // 添加 SkillEntity 脚本
            go.AddComponent<SkillEntity>();

            // 保存为预制体
            string prefabPath = $"{PREFAB_PATH}/SkillEntity.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            
            // 清理临时对象
            Object.DestroyImmediate(go);

            Debug.Log($"[CombatPrefabGenerator] SkillEntity 预制体已生成: {prefabPath}");
        }

        [MenuItem("FarmGame/Combat/生成 CharEntity 预制体", false, 102)]
        public static void GenerateCharEntityPrefab()
        {
            EnsureDirectoryExists();

            // 创建 GameObject
            var go = new GameObject("CharEntity");

            // 添加 SpriteRenderer
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCharacterSprite();
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 5;

            // 添加 Rigidbody2D
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.drag = 5f;
            rb.angularDrag = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // 添加 CircleCollider2D
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
            col.isTrigger = false;

            // 添加 CharEntity 脚本
            go.AddComponent<CharEntity>();

            // 保存为预制体
            string prefabPath = $"{PREFAB_PATH}/CharEntity.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            
            // 清理临时对象
            Object.DestroyImmediate(go);

            Debug.Log($"[CombatPrefabGenerator] CharEntity 预制体已生成: {prefabPath}");
        }

        [MenuItem("FarmGame/Combat/配置 Combat 场景", false, 200)]
        public static void SetupCombatScene()
        {
            // 检查当前场景
            var currentScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            if (!currentScene.name.ToLower().Contains("combat"))
            {
                Debug.LogWarning("[CombatPrefabGenerator] 请先打开 Combat 场景再执行此操作");
                return;
            }

            // 查找或创建 Main Camera
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraGO = new GameObject("Main Camera");
                cameraGO.tag = "MainCamera";
                camera = cameraGO.AddComponent<Camera>();
                cameraGO.AddComponent<AudioListener>();
            }

            // 配置相机
            camera.orthographic = true;
            camera.orthographicSize = 7.5f; // 适配 15 高度
            camera.transform.position = new Vector3(0, 0, -10);
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
            camera.clearFlags = CameraClearFlags.SolidColor;

            // 查找或创建 CombatTestRunner
            var testRunner = Object.FindFirstObjectByType<CombatTestRunner>();
            if (testRunner == null)
            {
                var testGO = new GameObject("CombatTestRunner");
                testRunner = testGO.AddComponent<CombatTestRunner>();
            }

            // 标记场景为已修改
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(currentScene);

            Debug.Log("[CombatPrefabGenerator] Combat 场景配置完成！记得保存场景。");
        }

        #region 辅助方法

        private static void EnsureDirectoryExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            if (!AssetDatabase.IsValidFolder(PREFAB_PATH))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Combat");
            }
        }

        /// <summary>
        /// 创建投射物占位符 Sprite（小圆形，白色）
        /// </summary>
        private static Sprite CreateProjectileSprite()
        {
            int size = 16;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.name = "ProjectileSprite";

            float radius = size / 2f;
            Vector2 center = new Vector2(radius, radius);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    if (distance <= radius - 0.5f)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                    else if (distance <= radius)
                    {
                        float alpha = 1f - (distance - (radius - 0.5f)) * 2f;
                        texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(alpha)));
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();

            // 保存纹理到 Assets
            SaveTextureAsAsset(texture, $"{PREFAB_PATH}/ProjectileSprite.asset");

            return Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                16f
            );
        }

        /// <summary>
        /// 创建角色占位符 Sprite（较大圆形，灰色）
        /// </summary>
        private static Sprite CreateCharacterSprite()
        {
            int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.name = "CharacterSprite";

            float radius = size / 2f;
            Vector2 center = new Vector2(radius, radius);
            Color baseColor = new Color(0.7f, 0.7f, 0.7f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    if (distance <= radius - 1f)
                    {
                        texture.SetPixel(x, y, baseColor);
                    }
                    else if (distance <= radius)
                    {
                        float alpha = 1f - (distance - (radius - 1f));
                        texture.SetPixel(x, y, new Color(baseColor.r, baseColor.g, baseColor.b, alpha));
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();

            // 保存纹理到 Assets
            SaveTextureAsAsset(texture, $"{PREFAB_PATH}/CharacterSprite.asset");

            return Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                32f
            );
        }

        private static void SaveTextureAsAsset(Texture2D texture, string path)
        {
            // 将 Texture2D 保存为 PNG 文件
            string pngPath = path.Replace(".asset", ".png");
            byte[] pngData = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(pngPath, pngData);
            AssetDatabase.ImportAsset(pngPath);
            
            // 配置纹理导入设置
            var importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
        }

        #endregion
    }
}
#endif
