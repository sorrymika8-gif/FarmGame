using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FarmGame.Farm;
using FarmGame.GameConfig;
using FarmGame.GameConfig.Generated;
using FarmGame.Core;

namespace FarmGame.Farm.View
{
    /// <summary>
    /// 作物视图组件
    /// 显示土地上种植的作物图标、状态和进度条
    /// 使用 SpriteRenderer 实现，显示在 Tilemap 上方
    /// </summary>
    public class CropView : MonoBehaviour
    {
        #region 序列化字段

        [Header("图标组件")]
        [Tooltip("作物图标 SpriteRenderer")]
        [SerializeField] private SpriteRenderer mIconRenderer;

        [Header("进度条组件")]
        [Tooltip("进度条背景")]
        [SerializeField] private SpriteRenderer mProgressBg;

        [Tooltip("进度条填充")]
        [SerializeField] private SpriteRenderer mProgressFill;

        [Header("文本组件")]
        [Tooltip("状态文本（幼苗/中期/成熟）")]
        [SerializeField] private TextMeshPro mStatusText;

        [Tooltip("描述文本（预留给LLM描述系统）")]
        [SerializeField] private TextMeshPro mDescriptionText;

        [Header("配置")]
        [Tooltip("进度条最大宽度")]
        [SerializeField] private float mProgressBarMaxWidth = 0.8f;

        [Tooltip("作物图标固定缩放 (90%填充格子)")]
        [SerializeField] private float mIconScale = 0.9f;

        #endregion

        #region 私有字段

        private PlantEntity mPlant;
        private SoilEntity mSoil;
        private SeedConfig mConfig;
        private Vector2Int mGridPos;

        /// <summary>
        /// 作物阶段名称
        /// </summary>
        private static readonly string[] STAGE_NAMES = { "幼苗", "中期", "成熟" };

        /// <summary>
        /// 各阶段颜色
        /// </summary>
        private static readonly Color[] STAGE_COLORS = 
        {
            new Color(0.5f, 0.85f, 0.5f),  // 幼苗 - 浅绿
            new Color(0.3f, 0.7f, 0.3f),   // 中期 - 中绿
            new Color(1f, 0.85f, 0.2f)     // 成熟 - 金色
        };

        #endregion

        #region 公共属性

        /// <summary>
        /// 关联的土地实体
        /// </summary>
        public SoilEntity Soil => mSoil;

        /// <summary>
        /// 关联的作物实体
        /// </summary>
        public PlantEntity Plant => mPlant;

        /// <summary>
        /// 网格坐标
        /// </summary>
        public Vector2Int GridPos => mGridPos;

        #endregion

        #region 公共静态方法

        /// <summary>
        /// 创建作物视图实例
        /// </summary>
        /// <param name="soil">土地实体</param>
        /// <param name="worldPos">世界坐标</param>
        /// <param name="parent">父对象</param>
        /// <returns>CropView 实例</returns>
        public static CropView Create(SoilEntity soil, Vector3 worldPos, Transform parent)
        {
            if (soil == null || !soil.HasPlant)
            {
                Debug.LogWarning("[CropView] 无法创建：土地为空或无作物");
                return null;
            }

            // 创建 GameObject
            var go = new GameObject($"CropView_{soil.GridPos.x}_{soil.GridPos.y}");
            go.transform.SetParent(parent);
            go.transform.position = worldPos;

            // 添加 CropView 组件
            var cropView = go.AddComponent<CropView>();
            cropView.SetupComponents();
            cropView.Bind(soil);

            return cropView;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 绑定土地和作物
        /// </summary>
        /// <param name="soil">土地实体</param>
        public void Bind(SoilEntity soil)
        {
            // 取消订阅旧事件
            if (mPlant != null)
            {
                mPlant.OnStageChanged -= OnPlantStageChanged;
            }

            mSoil = soil;
            mGridPos = soil.GridPos;
            mPlant = soil.Plant;
            mConfig = mPlant?.PlantData;

            // 订阅新事件
            if (mPlant != null)
            {
                mPlant.OnStageChanged += OnPlantStageChanged;
            }

            UpdateDisplay();
        }

        /// <summary>
        /// 更新显示
        /// </summary>
        public void UpdateDisplay()
        {
            if (mPlant == null || mConfig == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            // 更新图标
            UpdateIcon();

            // 更新状态文本
            UpdateStatusText();

            // 更新进度条
            UpdateProgressBar();

            // 更新描述（预留）
            UpdateDescription();
        }

        /// <summary>
        /// 设置描述文本（供LLM系统调用）
        /// </summary>
        /// <param name="description">描述内容</param>
        public void SetDescription(string description)
        {
            if (mDescriptionText != null)
            {
                mDescriptionText.text = description ?? string.Empty;
                mDescriptionText.gameObject.SetActive(!string.IsNullOrEmpty(description));
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 初始化组件（动态创建时使用）
        /// </summary>
        private void SetupComponents()
        {
            // 创建图标
            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(transform);
            iconGo.transform.localPosition = Vector3.zero;
            mIconRenderer = iconGo.AddComponent<SpriteRenderer>();
            mIconRenderer.sortingOrder = 10;

            // 创建进度条背景
            var progressBgGo = new GameObject("ProgressBg");
            progressBgGo.transform.SetParent(transform);
            progressBgGo.transform.localPosition = new Vector3(0, -0.55f, 0);  // 移到图标下方
            mProgressBg = progressBgGo.AddComponent<SpriteRenderer>();
            mProgressBg.sortingOrder = 11;
            mProgressBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            // 使用简单的白色方块作为进度条背景
            mProgressBg.sprite = CreateRectSprite();
            mProgressBg.transform.localScale = new Vector3(mProgressBarMaxWidth, 0.1f, 1f);  // 加高加宽

            // 创建进度条填充
            var progressFillGo = new GameObject("ProgressFill");
            progressFillGo.transform.SetParent(transform);
            progressFillGo.transform.localPosition = new Vector3(-mProgressBarMaxWidth / 2f, -0.55f, 0);  // 与背景对齐
            mProgressFill = progressFillGo.AddComponent<SpriteRenderer>();
            mProgressFill.sortingOrder = 12;
            mProgressFill.color = new Color(0.3f, 0.8f, 0.3f, 1f);
            mProgressFill.sprite = CreateRectSprite();
            // 设置锚点为左侧
            mProgressFill.transform.localScale = new Vector3(0, 0.08f, 1f);  // 加高

            // 创建状态文本 (显示在图标上方)
            var statusGo = new GameObject("StatusText");
            statusGo.transform.SetParent(transform);
            statusGo.transform.localPosition = new Vector3(0, 0.55f, 0);  // 上移
            mStatusText = statusGo.AddComponent<TextMeshPro>();
            mStatusText.alignment = TextAlignmentOptions.Center;
            mStatusText.fontSize = 3f;  // 字号加大
            mStatusText.sortingOrder = 15;

            // 创建描述文本（默认隐藏）
            var descGo = new GameObject("DescriptionText");
            descGo.transform.SetParent(transform);
            descGo.transform.localPosition = new Vector3(0, -0.75f, 0);  // 下移到进度条下方
            mDescriptionText = descGo.AddComponent<TextMeshPro>();
            mDescriptionText.alignment = TextAlignmentOptions.Center;
            mDescriptionText.fontSize = 2f;  // 字号加大
            mDescriptionText.sortingOrder = 15;
            mDescriptionText.color = new Color(0.8f, 0.8f, 0.8f);
            descGo.SetActive(false);
        }

        /// <summary>
        /// 创建简单的矩形Sprite
        /// </summary>
        private Sprite CreateRectSprite()
        {
            // 创建1x1白色纹理
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        }

        /// <summary>
        /// 更新图标
        /// </summary>
        private void UpdateIcon()
        {
            if (mIconRenderer == null || mConfig == null) return;

            // 尝试加载图标
            if (!string.IsNullOrEmpty(mConfig.icon))
            {
                Sprite sprite = null;
                
                // 尝试多种路径格式
                string[] pathFormats = new string[]
                {
                    "prefabs/" + mConfig.icon,           // prefabs/plants/pasture_seed
                    mConfig.icon,                         // plants/pasture_seed
                    "Sprites/Farm/" + mConfig.icon,      // Sprites/Farm/plants/pasture_seed
                    "Sprites/" + mConfig.icon,           // Sprites/plants/pasture_seed
                };

                foreach (var path in pathFormats)
                {
                    sprite = ResourceManager.Instance?.Load<Sprite>(path);
                    if (sprite != null)
                    {
                        break;
                    }
                }
                
                if (sprite != null)
                {
                    mIconRenderer.sprite = sprite;
                    // 使用固定缩放 (90%填充格子)
                    mIconRenderer.transform.localScale = Vector3.one * mIconScale;
                    return;
                }
            }

            // 加载失败时使用默认显示（创建一个彩色方块）
            mIconRenderer.sprite = CreateDefaultSprite();
            // 使用固定缩放 (90%填充格子)
            mIconRenderer.transform.localScale = Vector3.one * mIconScale;
            
            // 根据阶段设置颜色
            int stageIndex = Mathf.Clamp(mPlant.CurrentStageIndex, 0, STAGE_COLORS.Length - 1);
            mIconRenderer.color = STAGE_COLORS[stageIndex];
            
            Debug.LogWarning($"[CropView] 无法加载作物图标: {mConfig.icon}，使用默认显示");
        }

        /// <summary>
        /// 创建默认图标Sprite
        /// </summary>
        private Sprite CreateDefaultSprite()
        {
            // 创建一个简单的圆形纹理
            int size = 32;
            var texture = new Texture2D(size, size);
            Color transparent = new Color(0, 0, 0, 0);
            Color white = Color.white;
            
            float center = size / 2f;
            float radius = size / 2f - 1;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist <= radius)
                    {
                        texture.SetPixel(x, y, white);
                    }
                    else
                    {
                        texture.SetPixel(x, y, transparent);
                    }
                }
            }
            
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
        }

        /// <summary>
        /// 更新状态文本
        /// </summary>
        private void UpdateStatusText()
        {
            if (mStatusText == null || mPlant == null) return;

            int stageIndex = Mathf.Clamp(mPlant.CurrentStageIndex, 0, STAGE_NAMES.Length - 1);
            mStatusText.text = STAGE_NAMES[stageIndex];
            mStatusText.color = STAGE_COLORS[stageIndex];
        }

        /// <summary>
        /// 更新进度条
        /// </summary>
        private void UpdateProgressBar()
        {
            if (mProgressFill == null || mPlant == null || mConfig == null) return;

            // 计算进度
            float progress = mConfig.need_maturity > 0 
                ? Mathf.Clamp01(mPlant.CurrentMaturity / mConfig.need_maturity) 
                : 0f;

            // 更新填充宽度
            float fillWidth = mProgressBarMaxWidth * progress;
            mProgressFill.transform.localScale = new Vector3(fillWidth, 0.08f, 1f);  // 加高
            
            // 更新填充位置（从左侧开始）
            mProgressFill.transform.localPosition = new Vector3(
                -mProgressBarMaxWidth / 2f + fillWidth / 2f,
                -0.55f,  // 移到图标下方
                0
            );

            // 更新颜色
            if (mPlant.IsMature)
            {
                mProgressFill.color = new Color(1f, 0.85f, 0.2f, 1f); // 金色
                if (mProgressBg != null)
                {
                    mProgressBg.gameObject.SetActive(false);
                }
            }
            else
            {
                mProgressFill.color = new Color(0.3f, 0.8f, 0.3f, 1f); // 绿色
                if (mProgressBg != null)
                {
                    mProgressBg.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// 更新描述（预留给LLM系统）
        /// </summary>
        private void UpdateDescription()
        {
            // 预留：后续由 LLM 系统调用 SetDescription 填充
            // 默认不显示
            if (mDescriptionText != null)
            {
                mDescriptionText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 作物阶段变化回调
        /// </summary>
        private void OnPlantStageChanged(PlantEntity plant)
        {
            UpdateDisplay();
        }

        #endregion

        #region 生命周期

        private void OnDestroy()
        {
            // 取消订阅事件
            if (mPlant != null)
            {
                mPlant.OnStageChanged -= OnPlantStageChanged;
            }
        }

        #endregion
    }
}
