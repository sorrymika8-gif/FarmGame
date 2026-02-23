using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;

namespace FarmGame.Editor
{
    /// <summary>
    /// 作物UI预制体创建工具
    /// 用于自动生成 CropDetailBubblePanel 预制体
    /// </summary>
    public class CropUIPrefabCreator : EditorWindow
    {
        /// <summary>
        /// 一键创建所有作物相关预制体
        /// </summary>
        [MenuItem("FarmGame/创建所有作物UI预制体")]
        public static void CreateAllPrefabs()
        {
            CreateHarvestItemPrefab();
            CreateCropDetailBubblePrefab();
            EditorUtility.DisplayDialog("完成", "所有预制体已创建完成！", "确定");
        }

        [MenuItem("FarmGame/创建作物详情气泡预制体")]
        public static void CreateCropDetailBubblePrefab()
        {
            // 确保目录存在
            string uiPath = "Assets/Resources/UI/CropBubble";
            if (!Directory.Exists(uiPath))
            {
                Directory.CreateDirectory(uiPath);
                AssetDatabase.Refresh();
            }

            string prefabPath = $"{uiPath}/CropDetailBubblePanel.prefab";

            // 检查是否已存在
            if (File.Exists(prefabPath))
            {
                if (!EditorUtility.DisplayDialog("确认覆盖", 
                    "CropDetailBubblePanel.prefab 已存在，是否覆盖？", "覆盖", "取消"))
                {
                    return;
                }
            }

            // 创建根对象
            GameObject root = new GameObject("CropDetailBubblePanel");
            
            // 添加 Canvas 相关组件（作为UIPanel需要）
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            root.AddComponent<GraphicRaycaster>();

            // 添加 CropDetailBubblePanel 组件
            var panelScript = root.AddComponent<FarmGame.UI.CropDetailBubblePanel>();

            // === 创建背景遮罩 ===
            GameObject bgMask = CreateUIObject("BackgroundMask", root.transform);
            RectTransform bgMaskRect = bgMask.GetComponent<RectTransform>();
            StretchToParent(bgMaskRect);
            
            Image bgMaskImage = bgMask.AddComponent<Image>();
            bgMaskImage.color = new Color(0, 0, 0, 0.3f);
            
            Button bgMaskButton = bgMask.AddComponent<Button>();
            bgMaskButton.transition = Selectable.Transition.None;

            // === 创建气泡容器 ===
            GameObject bubbleContainer = CreateUIObject("BubbleContainer", root.transform);
            RectTransform bubbleRect = bubbleContainer.GetComponent<RectTransform>();
            bubbleRect.sizeDelta = new Vector2(260, 200);  // 紧凑布局
            bubbleRect.anchoredPosition = Vector2.zero;

            // 气泡背景
            Image bubbleBg = bubbleContainer.AddComponent<Image>();
            bubbleBg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            // 添加圆角效果的Outline
            var outline = bubbleContainer.AddComponent<Outline>();
            outline.effectColor = new Color(0.4f, 0.6f, 0.4f, 1f);
            outline.effectDistance = new Vector2(2, 2);

            // === 创建标题区域 ===
            GameObject headerArea = CreateUIObject("HeaderArea", bubbleContainer.transform);
            RectTransform headerRect = headerArea.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.pivot = new Vector2(0.5f, 1);
            headerRect.anchoredPosition = new Vector2(0, -5);
            headerRect.sizeDelta = new Vector2(-10, 50);  // 紧凑

            // 作物图标
            GameObject iconObj = CreateUIObject("Icon", headerArea.transform);
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0.5f);
            iconRect.anchorMax = new Vector2(0, 0.5f);
            iconRect.pivot = new Vector2(0, 0.5f);
            iconRect.anchoredPosition = new Vector2(5, 0);
            iconRect.sizeDelta = new Vector2(40, 40);  // 缩小图标
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.white;

            // 作物名称
            GameObject nameObj = CreateUIObject("NameText", headerArea.transform);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.5f);
            nameRect.anchorMax = new Vector2(1, 0.5f);
            nameRect.pivot = new Vector2(0, 0.5f);
            nameRect.anchoredPosition = new Vector2(50, 8);
            nameRect.sizeDelta = new Vector2(-60, 25);
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "作物名称";
            nameText.fontSize = 18;  // 缩小字号
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Left;

            // 生长阶段
            GameObject stageObj = CreateUIObject("StageText", headerArea.transform);
            RectTransform stageRect = stageObj.GetComponent<RectTransform>();
            stageRect.anchorMin = new Vector2(0, 0.5f);
            stageRect.anchorMax = new Vector2(1, 0.5f);
            stageRect.pivot = new Vector2(0, 0.5f);
            stageRect.anchoredPosition = new Vector2(50, -12);
            stageRect.sizeDelta = new Vector2(-60, 20);
            TextMeshProUGUI stageText = stageObj.AddComponent<TextMeshProUGUI>();
            stageText.text = "生长期";
            stageText.fontSize = 14;  // 缩小
            stageText.color = new Color(0.5f, 0.85f, 0.5f);
            stageText.alignment = TextAlignmentOptions.Left;

            // === 创建进度区域 ===
            GameObject progressArea = CreateUIObject("ProgressArea", bubbleContainer.transform);
            RectTransform progressAreaRect = progressArea.GetComponent<RectTransform>();
            progressAreaRect.anchorMin = new Vector2(0, 1);
            progressAreaRect.anchorMax = new Vector2(1, 1);
            progressAreaRect.pivot = new Vector2(0.5f, 1);
            progressAreaRect.anchoredPosition = new Vector2(0, -58);  // 紧凑
            progressAreaRect.sizeDelta = new Vector2(-10, 40);  // 缩小高度

            // 进度条背景
            GameObject progressBg = CreateUIObject("ProgressBg", progressArea.transform);
            RectTransform progressBgRect = progressBg.GetComponent<RectTransform>();
            progressBgRect.anchorMin = new Vector2(0, 0.5f);
            progressBgRect.anchorMax = new Vector2(1, 0.5f);
            progressBgRect.pivot = new Vector2(0.5f, 0.5f);
            progressBgRect.anchoredPosition = new Vector2(-25, 5);  // 为百分比文本留空间
            progressBgRect.sizeDelta = new Vector2(-50, 16);  // 缩小高度
            Image progressBgImage = progressBg.AddComponent<Image>();
            progressBgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // 进度条填充
            GameObject progressFill = CreateUIObject("ProgressFill", progressBg.transform);
            RectTransform progressFillRect = progressFill.GetComponent<RectTransform>();
            progressFillRect.anchorMin = Vector2.zero;
            progressFillRect.anchorMax = Vector2.one;
            progressFillRect.offsetMin = Vector2.zero;
            progressFillRect.offsetMax = Vector2.zero;
            Image progressFillImage = progressFill.AddComponent<Image>();
            progressFillImage.color = new Color(0.3f, 0.8f, 0.3f, 1f);
            progressFillImage.type = Image.Type.Filled;
            progressFillImage.fillMethod = Image.FillMethod.Horizontal;
            progressFillImage.fillAmount = 0.6f;

            // 进度百分比文本
            GameObject progressTextObj = CreateUIObject("ProgressText", progressArea.transform);
            RectTransform progressTextRect = progressTextObj.GetComponent<RectTransform>();
            progressTextRect.anchorMin = new Vector2(1, 0.5f);
            progressTextRect.anchorMax = new Vector2(1, 0.5f);
            progressTextRect.pivot = new Vector2(1, 0.5f);
            progressTextRect.anchoredPosition = new Vector2(0, 5);
            progressTextRect.sizeDelta = new Vector2(45, 20);  // 缩小
            TextMeshProUGUI progressText = progressTextObj.AddComponent<TextMeshProUGUI>();
            progressText.text = "60%";
            progressText.fontSize = 14;  // 缩小
            progressText.color = Color.white;
            progressText.alignment = TextAlignmentOptions.Right;

            // 预计时间文本
            GameObject timeObj = CreateUIObject("TimeText", progressArea.transform);
            RectTransform timeRect = timeObj.GetComponent<RectTransform>();
            timeRect.anchorMin = new Vector2(0, 0);
            timeRect.anchorMax = new Vector2(1, 0);
            timeRect.pivot = new Vector2(0.5f, 0);
            timeRect.anchoredPosition = new Vector2(0, 0);
            timeRect.sizeDelta = new Vector2(0, 18);  // 缩小
            TextMeshProUGUI timeText = timeObj.AddComponent<TextMeshProUGUI>();
            timeText.text = "预计 5 分钟后成熟";
            timeText.fontSize = 12;  // 缩小
            timeText.color = new Color(0.7f, 0.7f, 0.7f);
            timeText.alignment = TextAlignmentOptions.Center;

            // === 创建收获预览区域 ===
            GameObject harvestArea = CreateUIObject("HarvestPreviewArea", bubbleContainer.transform);
            RectTransform harvestAreaRect = harvestArea.GetComponent<RectTransform>();
            harvestAreaRect.anchorMin = new Vector2(0, 1);
            harvestAreaRect.anchorMax = new Vector2(1, 1);
            harvestAreaRect.pivot = new Vector2(0.5f, 1);
            harvestAreaRect.anchoredPosition = new Vector2(0, -100);  // 紧凑
            harvestAreaRect.sizeDelta = new Vector2(-10, 55);  // 缩小

            // 收获预览标题
            GameObject harvestTitle = CreateUIObject("HarvestTitle", harvestArea.transform);
            RectTransform harvestTitleRect = harvestTitle.GetComponent<RectTransform>();
            harvestTitleRect.anchorMin = new Vector2(0, 1);
            harvestTitleRect.anchorMax = new Vector2(1, 1);
            harvestTitleRect.pivot = new Vector2(0.5f, 1);
            harvestTitleRect.anchoredPosition = Vector2.zero;
            harvestTitleRect.sizeDelta = new Vector2(0, 18);  // 缩小
            TextMeshProUGUI harvestTitleText = harvestTitle.AddComponent<TextMeshProUGUI>();
            harvestTitleText.text = "收获预览";
            harvestTitleText.fontSize = 12;  // 缩小
            harvestTitleText.color = new Color(0.8f, 0.8f, 0.8f);
            harvestTitleText.alignment = TextAlignmentOptions.Left;

            // 收获物容器
            GameObject harvestContainer = CreateUIObject("HarvestPreviewContainer", harvestArea.transform);
            RectTransform harvestContainerRect = harvestContainer.GetComponent<RectTransform>();
            harvestContainerRect.anchorMin = new Vector2(0, 0);
            harvestContainerRect.anchorMax = new Vector2(1, 1);
            harvestContainerRect.pivot = new Vector2(0.5f, 0.5f);
            harvestContainerRect.offsetMin = new Vector2(0, 0);
            harvestContainerRect.offsetMax = new Vector2(0, -20);  // 缩小距离

            // 添加水平布局
            HorizontalLayoutGroup harvestLayout = harvestContainer.AddComponent<HorizontalLayoutGroup>();
            harvestLayout.spacing = 5;  // 缩小间距
            harvestLayout.childAlignment = TextAnchor.MiddleLeft;
            harvestLayout.childControlWidth = false;
            harvestLayout.childControlHeight = false;
            harvestLayout.childForceExpandWidth = false;
            harvestLayout.childForceExpandHeight = false;

            // === 创建收获按钮 ===
            GameObject harvestBtn = CreateUIObject("HarvestButton", bubbleContainer.transform);
            RectTransform harvestBtnRect = harvestBtn.GetComponent<RectTransform>();
            harvestBtnRect.anchorMin = new Vector2(0.5f, 0);
            harvestBtnRect.anchorMax = new Vector2(0.5f, 0);
            harvestBtnRect.pivot = new Vector2(0.5f, 0);
            harvestBtnRect.anchoredPosition = new Vector2(0, 8);  // 缩小边距
            harvestBtnRect.sizeDelta = new Vector2(100, 35);  // 缩小按钮

            Image harvestBtnImage = harvestBtn.AddComponent<Image>();
            harvestBtnImage.color = new Color(0.2f, 0.6f, 0.2f, 1f);

            Button harvestButton = harvestBtn.AddComponent<Button>();
            ColorBlock colors = harvestButton.colors;
            colors.normalColor = new Color(0.2f, 0.6f, 0.2f, 1f);
            colors.highlightedColor = new Color(0.3f, 0.7f, 0.3f, 1f);
            colors.pressedColor = new Color(0.15f, 0.5f, 0.15f, 1f);
            harvestButton.colors = colors;

            // 收获按钮文本
            GameObject harvestBtnTextObj = CreateUIObject("Text", harvestBtn.transform);
            RectTransform harvestBtnTextRect = harvestBtnTextObj.GetComponent<RectTransform>();
            StretchToParent(harvestBtnTextRect);
            TextMeshProUGUI harvestBtnText = harvestBtnTextObj.AddComponent<TextMeshProUGUI>();
            harvestBtnText.text = "收获";
            harvestBtnText.fontSize = 16;  // 缩小
            harvestBtnText.fontStyle = FontStyles.Bold;
            harvestBtnText.color = Color.white;
            harvestBtnText.alignment = TextAlignmentOptions.Center;

            // === 绑定序列化字段 ===
            SerializedObject serializedPanel = new SerializedObject(panelScript);
            
            serializedPanel.FindProperty("mBackgroundMask").objectReferenceValue = bgMaskButton;
            serializedPanel.FindProperty("mBubbleContainer").objectReferenceValue = bubbleRect;
            serializedPanel.FindProperty("mNameText").objectReferenceValue = nameText;
            serializedPanel.FindProperty("mIconImage").objectReferenceValue = iconImage;
            serializedPanel.FindProperty("mStageText").objectReferenceValue = stageText;
            serializedPanel.FindProperty("mProgressFill").objectReferenceValue = progressFillImage;
            serializedPanel.FindProperty("mProgressText").objectReferenceValue = progressText;
            serializedPanel.FindProperty("mTimeText").objectReferenceValue = timeText;
            serializedPanel.FindProperty("mHarvestPreviewContainer").objectReferenceValue = harvestContainerRect.transform;
            serializedPanel.FindProperty("mHarvestButton").objectReferenceValue = harvestButton;
            serializedPanel.FindProperty("mHarvestButtonText").objectReferenceValue = harvestBtnText;
            
            serializedPanel.ApplyModifiedProperties();

            // 保存预制体
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            DestroyImmediate(root);

            AssetDatabase.Refresh();
            Debug.Log($"[CropUIPrefabCreator] 成功创建预制体: {prefabPath}");
            EditorUtility.DisplayDialog("完成", $"预制体已创建:\n{prefabPath}", "确定");
        }

        /// <summary>
        /// 创建收获物预览项预制体
        /// </summary>
        [MenuItem("FarmGame/创建收获物预览项预制体")]
        public static void CreateHarvestItemPrefab()
        {
            string uiPath = "Assets/Resources/UI/CropBubble";
            if (!Directory.Exists(uiPath))
            {
                Directory.CreateDirectory(uiPath);
                AssetDatabase.Refresh();
            }

            string prefabPath = $"{uiPath}/HarvestPreviewItem.prefab";

            GameObject root = new GameObject("HarvestPreviewItem");
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(60, 45);  // 缩小预览项

            // 图标
            GameObject iconObj = CreateUIObject("Icon", root.transform);
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 1);
            iconRect.anchorMax = new Vector2(0.5f, 1);
            iconRect.pivot = new Vector2(0.5f, 1);
            iconRect.anchoredPosition = new Vector2(0, -2);
            iconRect.sizeDelta = new Vector2(30, 30);  // 缩小图标
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.white;

            // 数量文本
            GameObject textObj = CreateUIObject("Text", root.transform);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 0);
            textRect.pivot = new Vector2(0.5f, 0);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(0, 15);  // 缩小
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "x1";
            text.fontSize = 10;  // 缩小
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            DestroyImmediate(root);

            AssetDatabase.Refresh();
            Debug.Log($"[CropUIPrefabCreator] 成功创建预制体: {prefabPath}");
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.localPosition = Vector3.zero;
            rect.localScale = Vector3.one;
            return go;
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
