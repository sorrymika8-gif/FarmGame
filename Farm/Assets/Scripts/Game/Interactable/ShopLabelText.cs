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
        private Vector3 mOffset = Vector3.zero;

        [SerializeField]
        private int mFontSize = 64;

        [SerializeField]
        private float mCharacterSize = 0.08f;

        [SerializeField]
        private float mFitWidthRatio = 0.75f;

        [SerializeField]
        private float mFitHeightRatio = 0.4f;

        [SerializeField]
        private Color mColor = Color.white;

        private void Awake()
        {
            var existing = transform.Find("ShopLabel");
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }

            var textObject = new GameObject("ShopLabel");
            textObject.transform.SetParent(transform, false);
            textObject.layer = gameObject.layer;

            Vector3 parentScale = transform.lossyScale;
            float safeScaleX = Mathf.Approximately(parentScale.x, 0f) ? 1f : parentScale.x;
            float safeScaleY = Mathf.Approximately(parentScale.y, 0f) ? 1f : parentScale.y;
            float safeScaleZ = Mathf.Approximately(parentScale.z, 0f) ? 1f : parentScale.z;

            textObject.transform.localPosition = new Vector3(
                mOffset.x / safeScaleX,
                mOffset.y / safeScaleY,
                mOffset.z / safeScaleZ);

            textObject.transform.localScale = new Vector3(
                1f / safeScaleX,
                1f / safeScaleY,
                1f / safeScaleZ);

            var textMesh = textObject.AddComponent<TextMesh>();
            textMesh.text = mLabelText;
            textMesh.characterSize = mCharacterSize;
            textMesh.fontSize = mFontSize;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = mColor;

            var meshRenderer = textObject.GetComponent<MeshRenderer>();
            meshRenderer.sortingLayerID = 0;
            meshRenderer.sortingOrder = 10;

            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                meshRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
                meshRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
                FitLabelToSprite(textObject.transform, meshRenderer, spriteRenderer);
            }
        }

        private void FitLabelToSprite(Transform labelTransform, MeshRenderer labelRenderer, SpriteRenderer spriteRenderer)
        {
            float targetWidth = spriteRenderer.bounds.size.x * mFitWidthRatio;
            float targetHeight = spriteRenderer.bounds.size.y * mFitHeightRatio;
            float currentWidth = labelRenderer.bounds.size.x;
            float currentHeight = labelRenderer.bounds.size.y;

            if (currentWidth <= 0f || currentHeight <= 0f)
            {
                return;
            }

            float widthScale = targetWidth / currentWidth;
            float heightScale = targetHeight / currentHeight;
            float fitScale = Mathf.Min(widthScale, heightScale);

            if (fitScale > 0f)
            {
                labelTransform.localScale *= fitScale;
            }
        }
    }
}
