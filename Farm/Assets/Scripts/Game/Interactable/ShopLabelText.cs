using UnityEngine;

namespace FarmGame.Game.Interactable
{
    /// <summary>
    /// 商店世界文字标签
    /// </summary>
    [RequireComponent(typeof(Transform))]
    public class ShopLabelText : MonoBehaviour
    {
        [SerializeField]
        private string mLabelText = "种子商店";

        [SerializeField]
        private Vector3 mOffset = new Vector3(0f, 0.8f, 0f);

        [SerializeField]
        private int mFontSize = 48;

        [SerializeField]
        private Color mColor = Color.white;

        private void Awake()
        {
            var textObject = new GameObject("ShopLabel");
            textObject.transform.SetParent(transform, false);
            textObject.transform.localPosition = mOffset;

            var textMesh = textObject.AddComponent<TextMesh>();
            textMesh.text = mLabelText;
            textMesh.characterSize = 0.08f;
            textMesh.fontSize = mFontSize;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = mColor;

            var meshRenderer = textObject.GetComponent<MeshRenderer>();
            meshRenderer.sortingOrder = 10;
        }
    }
}
