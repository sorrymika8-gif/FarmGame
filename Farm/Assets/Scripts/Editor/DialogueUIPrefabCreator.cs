#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace FarmGame.Editor
{
    /// <summary>
    /// 对话UI预制体生成工具
    /// 用于快速创建/更新对话框预制体
    /// </summary>
    public class DialogueUIPrefabCreator : EditorWindow
    {
        [MenuItem("FarmGame/UI工具/创建对话框预制体")]
        public static void ShowWindow()
        {
            GetWindow<DialogueUIPrefabCreator>("对话框预制体创建器");
        }

        private void OnGUI()
        {
            GUILayout.Label("对话框预制体创建工具", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "此工具将在 Resources/UI/DialogUI/ 目录下创建或更新对话框预制体。\n" +
                "预制体包含：立绘Image、名字文本、对话文本、输入框、发送/关闭按钮。",
                MessageType.Info);

            GUILayout.Space(20);

            if (GUILayout.Button("创建/更新 DialogueUIPanel 预制体", GUILayout.Height(40)))
            {
                CreateDialogueUIPrefab();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("创建/更新 NPCBubble 预制体", GUILayout.Height(40)))
            {
                CreateNPCBubblePrefab();
            }
        }

        /// <summary>
        /// 创建对话框预制体
        /// </summary>
        private static void CreateDialogueUIPrefab()
        {
            // 确保目录存在
            string folderPath = "Assets/Resources/UI/DialogUI";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh();
            }

            // 创建根Canvas
            GameObject root = new GameObject("DiaLogUiPab");
            
            // 添加RectTransform
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            // 添加CanvasGroup用于淡入淡出
            root.AddComponent<CanvasGroup>();

            // === 背景遮罩 ===
            GameObject bgMask = CreateUIElement("BackgroundMask", root.transform);
            Image bgMaskImg = bgMask.AddComponent<Image>();
            bgMaskImg.color = new Color(0, 0, 0, 0.5f);
            SetFullStretch(bgMask.GetComponent<RectTransform>());

            // === 对话框容器（底部） ===
            GameObject dialogueBox = CreateUIElement("DialogueBox", root.transform);
            RectTransform dialogueBoxRect = dialogueBox.GetComponent<RectTransform>();
            dialogueBoxRect.anchorMin = new Vector2(0, 0);
            dialogueBoxRect.anchorMax = new Vector2(1, 0.35f); // 底部35%
            dialogueBoxRect.offsetMin = new Vector2(20, 20);
            dialogueBoxRect.offsetMax = new Vector2(-20, 0);
            
            Image dialogueBoxBg = dialogueBox.AddComponent<Image>();
            dialogueBoxBg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            // === 表情区域（左侧） ===
            GameObject portraitArea = CreateUIElement("PortraitArea", dialogueBox.transform);
            RectTransform portraitRect = portraitArea.GetComponent<RectTransform>();
            portraitRect.anchorMin = new Vector2(0, 0);
            portraitRect.anchorMax = new Vector2(0.25f, 1);
            portraitRect.offsetMin = new Vector2(10, 10);
            portraitRect.offsetMax = new Vector2(0, -10);

            // 表情Image
            GameObject portraitImg = CreateUIElement("PortraitImage", portraitArea.transform);
            Image portrait = portraitImg.AddComponent<Image>();
            portrait.preserveAspect = true;
            portrait.color = Color.white;
            SetFullStretch(portraitImg.GetComponent<RectTransform>());

            // === 文本区域（右侧） ===
            GameObject textArea = CreateUIElement("TextArea", dialogueBox.transform);
            RectTransform textAreaRect = textArea.GetComponent<RectTransform>();
            textAreaRect.anchorMin = new Vector2(0.25f, 0);
            textAreaRect.anchorMax = new Vector2(1, 1);
            textAreaRect.offsetMin = new Vector2(10, 10);
            textAreaRect.offsetMax = new Vector2(-10, -10);

            // NPC名字
            GameObject nameObj = CreateUIElement("NameText", textArea.transform);
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.85f);
            nameRect.anchorMax = new Vector2(0.5f, 1);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "NPC名字";
            nameText.fontSize = 24;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = new Color(1f, 0.9f, 0.6f);
            nameText.alignment = TextAlignmentOptions.Left;

            // 对话内容
            GameObject dialogueObj = CreateUIElement("DialogueText", textArea.transform);
            RectTransform dialogueRect = dialogueObj.GetComponent<RectTransform>();
            dialogueRect.anchorMin = new Vector2(0, 0.35f);
            dialogueRect.anchorMax = new Vector2(1, 0.85f);
            dialogueRect.offsetMin = Vector2.zero;
            dialogueRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI dialogueText = dialogueObj.AddComponent<TextMeshProUGUI>();
            dialogueText.text = "对话内容显示在这里...";
            dialogueText.fontSize = 20;
            dialogueText.color = Color.white;
            dialogueText.alignment = TextAlignmentOptions.TopLeft;

            // === 输入区域 ===
            GameObject inputArea = CreateUIElement("InputArea", textArea.transform);
            RectTransform inputAreaRect = inputArea.GetComponent<RectTransform>();
            inputAreaRect.anchorMin = new Vector2(0, 0);
            inputAreaRect.anchorMax = new Vector2(1, 0.3f);
            inputAreaRect.offsetMin = Vector2.zero;
            inputAreaRect.offsetMax = Vector2.zero;

            // 输入框
            GameObject inputFieldObj = CreateUIElement("InputField", inputArea.transform);
            RectTransform inputFieldRect = inputFieldObj.GetComponent<RectTransform>();
            inputFieldRect.anchorMin = new Vector2(0, 0);
            inputFieldRect.anchorMax = new Vector2(0.7f, 1);
            inputFieldRect.offsetMin = Vector2.zero;
            inputFieldRect.offsetMax = new Vector2(-5, 0);

            Image inputBg = inputFieldObj.AddComponent<Image>();
            inputBg.color = new Color(0.2f, 0.2f, 0.25f);

            TMP_InputField inputField = inputFieldObj.AddComponent<TMP_InputField>();
            
            // 输入框文本区域
            GameObject textAreaObj = CreateUIElement("Text Area", inputFieldObj.transform);
            SetFullStretch(textAreaObj.GetComponent<RectTransform>(), new Vector2(10, 5), new Vector2(-10, -5));
            
            GameObject inputText = CreateUIElement("Text", textAreaObj.transform);
            SetFullStretch(inputText.GetComponent<RectTransform>());
            TextMeshProUGUI inputTMP = inputText.AddComponent<TextMeshProUGUI>();
            inputTMP.fontSize = 18;
            inputTMP.color = Color.white;
            
            GameObject placeholder = CreateUIElement("Placeholder", textAreaObj.transform);
            SetFullStretch(placeholder.GetComponent<RectTransform>());
            TextMeshProUGUI placeholderTMP = placeholder.AddComponent<TextMeshProUGUI>();
            placeholderTMP.text = "输入消息...";
            placeholderTMP.fontSize = 18;
            placeholderTMP.fontStyle = FontStyles.Italic;
            placeholderTMP.color = new Color(0.5f, 0.5f, 0.5f);

            inputField.textViewport = textAreaObj.GetComponent<RectTransform>();
            inputField.textComponent = inputTMP;
            inputField.placeholder = placeholderTMP;

            // 发送按钮
            GameObject sendBtnObj = CreateUIElement("SendButton", inputArea.transform);
            RectTransform sendBtnRect = sendBtnObj.GetComponent<RectTransform>();
            sendBtnRect.anchorMin = new Vector2(0.7f, 0);
            sendBtnRect.anchorMax = new Vector2(0.85f, 1);
            sendBtnRect.offsetMin = new Vector2(5, 0);
            sendBtnRect.offsetMax = Vector2.zero;

            Image sendBtnBg = sendBtnObj.AddComponent<Image>();
            sendBtnBg.color = new Color(0.2f, 0.6f, 0.3f);
            Button sendBtn = sendBtnObj.AddComponent<Button>();
            sendBtn.targetGraphic = sendBtnBg;

            GameObject sendBtnText = CreateUIElement("Text", sendBtnObj.transform);
            SetFullStretch(sendBtnText.GetComponent<RectTransform>());
            TextMeshProUGUI sendTMP = sendBtnText.AddComponent<TextMeshProUGUI>();
            sendTMP.text = "发送";
            sendTMP.fontSize = 18;
            sendTMP.color = Color.white;
            sendTMP.alignment = TextAlignmentOptions.Center;

            // 关闭按钮
            GameObject closeBtnObj = CreateUIElement("CloseButton", inputArea.transform);
            RectTransform closeBtnRect = closeBtnObj.GetComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(0.85f, 0);
            closeBtnRect.anchorMax = new Vector2(1, 1);
            closeBtnRect.offsetMin = new Vector2(5, 0);
            closeBtnRect.offsetMax = Vector2.zero;

            Image closeBtnBg = closeBtnObj.AddComponent<Image>();
            closeBtnBg.color = new Color(0.6f, 0.2f, 0.2f);
            Button closeBtn = closeBtnObj.AddComponent<Button>();
            closeBtn.targetGraphic = closeBtnBg;

            GameObject closeBtnText = CreateUIElement("Text", closeBtnObj.transform);
            SetFullStretch(closeBtnText.GetComponent<RectTransform>());
            TextMeshProUGUI closeTMP = closeBtnText.AddComponent<TextMeshProUGUI>();
            closeTMP.text = "关闭";
            closeTMP.fontSize = 18;
            closeTMP.color = Color.white;
            closeTMP.alignment = TextAlignmentOptions.Center;

            // === 添加DialogueUIPanel脚本 ===
            var panel = root.AddComponent<FarmGame.UI.DialogueUIPanel>();
            
            // 通过SerializedObject设置引用
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("PortraitImage").objectReferenceValue = portrait;
            so.FindProperty("NameText").objectReferenceValue = nameText;
            so.FindProperty("DialogueText").objectReferenceValue = dialogueText;
            so.FindProperty("InputField").objectReferenceValue = inputField;
            so.FindProperty("SendButton").objectReferenceValue = sendBtn;
            so.FindProperty("CloseButton").objectReferenceValue = closeBtn;
            so.ApplyModifiedProperties();

            // 保存预制体
            string prefabPath = $"{folderPath}/DiaLogUiPab.prefab";
            
            // 删除旧预制体
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            DestroyImmediate(root);

            AssetDatabase.Refresh();
            Debug.Log($"[DialogueUIPrefabCreator] 对话框预制体已创建: {prefabPath}");
            EditorUtility.DisplayDialog("完成", $"对话框预制体已创建:\n{prefabPath}", "确定");
        }

        /// <summary>
        /// 创建气泡预制体
        /// </summary>
        private static void CreateNPCBubblePrefab()
        {
            string folderPath = "Assets/Resources/UI/BubbleUI";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh();
            }

            // 创建根对象
            GameObject root = new GameObject("NPCBubble");
            
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(250, 80);

            CanvasGroup canvasGroup = root.AddComponent<CanvasGroup>();

            // 背景
            GameObject bg = CreateUIElement("Background", root.transform);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(1, 1, 1, 0.95f);
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            SetFullStretch(bgRect);

            // 文本
            GameObject textObj = CreateUIElement("Text", root.transform);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "对话内容";
            text.fontSize = 16;
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Center;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            SetFullStretch(textRect, new Vector2(10, 5), new Vector2(-10, -5));

            // 添加NPCBubble脚本
            var bubble = root.AddComponent<FarmGame.Game.NPC.NPCBubble>();
            
            SerializedObject so = new SerializedObject(bubble);
            so.FindProperty("mText").objectReferenceValue = text;
            so.FindProperty("mBackground").objectReferenceValue = bgImg;
            so.FindProperty("mCanvasGroup").objectReferenceValue = canvasGroup;
            so.ApplyModifiedProperties();

            // 保存预制体
            string prefabPath = $"{folderPath}/NPCBubble.prefab";
            
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            DestroyImmediate(root);

            AssetDatabase.Refresh();
            Debug.Log($"[DialogueUIPrefabCreator] 气泡预制体已创建: {prefabPath}");
            EditorUtility.DisplayDialog("完成", $"气泡预制体已创建:\n{prefabPath}", "确定");
        }

        #region 辅助方法

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static void SetFullStretch(RectTransform rect, Vector2? offsetMin = null, Vector2? offsetMax = null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin ?? Vector2.zero;
            rect.offsetMax = offsetMax ?? Vector2.zero;
        }

        #endregion
    }
}
#endif
