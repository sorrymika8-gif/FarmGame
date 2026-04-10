using System.Collections.Generic;
using UnityEngine;
using FarmGame.Combat.Entity;
using FarmGame.Combat.Data;
using FarmGame.Combat.LLM;

namespace FarmGame.Combat.Test
{
    /// <summary>
    /// 战斗测试运行器
    /// 用于验证弹道行为和技能效果
    /// </summary>
    public class CombatTestRunner : MonoBehaviour
    {
        #region 序列化字段

        [Header("预制体引用")]
        [SerializeField]
        private GameObject mSkillEntityPrefab;

        [SerializeField]
        private GameObject mCharEntityPrefab;

        [Header("测试配置")]
        [SerializeField]
        private string mCurrentSkillType = "basic";

        [SerializeField]
        private float mFireInterval = 1f;

        [SerializeField]
        private bool mAutoFire = false;

        [Header("生成位置")]
        [SerializeField]
        private Vector3 mPlayerSpawnPos = new Vector3(-5f, 0f, 0f);

        [SerializeField]
        private Vector3 mEnemySpawnPos = new Vector3(5f, 0f, 0f);

        [SerializeField]
        private float mPlayerMoveSpeed = 5f;

        #endregion

        #region 私有字段

        private CharEntity mPlayer;
        private CharEntity mEnemy;
        private float mFireTimer;
        private List<SkillEntity> mActiveSkills = new List<SkillEntity>();

        // 可用的技能类型列表
        private static readonly string[] SkillTypes = new string[]
        {
            "basic",
            "tracking",
            "split",
            "bounce",
            "aoe",
            "dot",
            "combo"
        };

        private int mCurrentSkillIndex = 0;

        #endregion

        #region 生命周期

        private void Awake()
        {
            Debug.Log("[CombatTestRunner] Awake 开始");
        }

        private void Start()
        {
            Debug.Log("[CombatTestRunner] Start 开始");
            
            // 强制创建可见的测试对象（不依赖预制体）
            ForceCreateVisibleEntities();
            
            Debug.Log("[CombatTestRunner] 战斗测试场景已初始化");
            Debug.Log("[CombatTestRunner] 按键说明:");
            Debug.Log("  WASD/方向键 - 移动玩家");
            Debug.Log("  Space - 发射当前技能");
            Debug.Log("  1-7   - 切换技能类型 (basic/tracking/split/bounce/aoe/dot/combo)");
            Debug.Log("  R     - 重置场景");
            Debug.Log("  T     - 切换自动发射");
        }
        
        /// <summary>
        /// 强制创建可见的测试实体（不依赖预制体和 CharEntity 组件）
        /// </summary>
        private void ForceCreateVisibleEntities()
        {
            Debug.Log("[CombatTestRunner] 创建测试实体...");
            
            // 创建玩家（蓝色圆形）
            var playerGO = CreateSimpleCircle("Player", mPlayerSpawnPos, Color.cyan, 1f);
            mPlayer = playerGO.AddComponent<CharEntity>();
            
            // 创建敌人（红色圆形）
            var enemyGO = CreateSimpleCircle("Enemy", mEnemySpawnPos, Color.red, 1f);
            mEnemy = enemyGO.AddComponent<CharEntity>();
            
            Debug.Log($"[CombatTestRunner] 玩家位置: {mPlayerSpawnPos}, 敌人位置: {mEnemySpawnPos}");
        }
        
        /// <summary>
        /// 创建简单的彩色圆形对象
        /// </summary>
        private GameObject CreateSimpleCircle(string name, Vector3 position, Color color, float radius)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            
            // 创建 SpriteRenderer 并设置 Sprite
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite(64, Color.white);
            sr.color = color;
            sr.sortingOrder = 10;
            
            // 添加物理组件
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.drag = 5f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = radius * 0.5f;
            
            Debug.Log($"[CombatTestRunner] 创建了 {name} 在位置 {position}");
            return go;
        }

        private void Update()
        {
            HandleInput();

            if (mAutoFire)
            {
                mFireTimer += Time.deltaTime;
                if (mFireTimer >= mFireInterval)
                {
                    mFireTimer = 0f;
                    FireSkill();
                }
            }

            // 清理已销毁的技能实体
            mActiveSkills.RemoveAll(s => s == null);
        }

        #endregion

        #region 输入处理

        private void HandleInput()
        {
            // 玩家移动 (WASD 或方向键)
            HandlePlayerMovement();
            
            // 发射技能
            if (Input.GetKeyDown(KeyCode.Space))
            {
                FireSkill();
            }

            // 切换技能类型 (1-7)
            for (int i = 0; i < SkillTypes.Length && i < 7; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    mCurrentSkillIndex = i;
                    mCurrentSkillType = SkillTypes[i];
                    Debug.Log($"[CombatTestRunner] 切换到技能: {mCurrentSkillType}");
                }
            }

            // 重置场景
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetScene();
            }

            // 切换自动发射（改为 T 键，避免与移动冲突）
            if (Input.GetKeyDown(KeyCode.T))
            {
                mAutoFire = !mAutoFire;
                Debug.Log($"[CombatTestRunner] 自动发射: {(mAutoFire ? "开启" : "关闭")}");
            }
        }

        private void HandlePlayerMovement()
        {
            if (mPlayer == null) return;

            // 获取输入
            float horizontal = Input.GetAxisRaw("Horizontal"); // A/D 或 左/右方向键
            float vertical = Input.GetAxisRaw("Vertical");     // W/S 或 上/下方向键

            if (Mathf.Approximately(horizontal, 0f) && Mathf.Approximately(vertical, 0f))
                return;

            // 计算移动方向并归一化
            Vector2 moveDirection = new Vector2(horizontal, vertical).normalized;

            // 移动玩家
            Vector3 movement = new Vector3(moveDirection.x, moveDirection.y, 0f) * mPlayerMoveSpeed * Time.deltaTime;
            mPlayer.transform.position += movement;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 发射当前选中的技能
        /// </summary>
        public void FireSkill()
        {
            if (mPlayer == null)
            {
                Debug.LogWarning("[CombatTestRunner] Player 未初始化");
                return;
            }

            // 创建测试技能数据
            var skillData = LLMBridge.CreateTestSkill(mCurrentSkillType);
            Debug.Log($"[CombatTestRunner] 发射技能: {skillData.displayName}");

            // 计算方向
            Vector2 direction = (mEnemy != null) 
                ? ((Vector2)(mEnemy.Position - mPlayer.Position)).normalized 
                : Vector2.right;

            // 尝试使用预制体，如果没有就创建简单投射物
            if (mSkillEntityPrefab != null)
            {
                var skillGO = Instantiate(mSkillEntityPrefab, mPlayer.Position, Quaternion.identity);
                var skillEntity = skillGO.GetComponent<SkillEntity>();
                if (skillEntity != null)
                {
                    skillEntity.Init(skillData, mPlayer);
                    skillEntity.SetDirection(direction);
                    mActiveSkills.Add(skillEntity);
                }
            }
            else
            {
                // 创建简单的测试投射物
                CreateSimpleProjectile(skillData, mPlayer.Position, direction);
            }
        }
        
        /// <summary>
        /// 创建简单的测试投射物（不依赖 SkillEntity 预制体）
        /// </summary>
        private void CreateSimpleProjectile(SkillAtomData skillData, Vector3 startPos, Vector2 direction)
        {
            var go = new GameObject($"Projectile_{skillData.displayName}");
            go.transform.position = startPos;
            
            // 添加 SpriteRenderer
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite(16, Color.white);
            sr.color = Color.yellow;
            sr.sortingOrder = 20;
            
            // 添加 Rigidbody2D
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            // 添加触发器碰撞体
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.25f;
            col.isTrigger = true;
            
            // 添加 SkillEntity 组件
            var skillEntity = go.AddComponent<SkillEntity>();
            skillEntity.Init(skillData, mPlayer);
            skillEntity.SetDirection(direction);
            
            mActiveSkills.Add(skillEntity);
            Debug.Log($"[CombatTestRunner] 创建测试投射物: {skillData.displayName}, 方向: {direction}");
        }

        /// <summary>
        /// 发射指定类型的技能
        /// </summary>
        /// <param name="skillType">技能类型名</param>
        public void FireSkill(string skillType)
        {
            mCurrentSkillType = skillType;
            FireSkill();
        }

        /// <summary>
        /// 重置测试场景
        /// </summary>
        public void ResetScene()
        {
            // 销毁所有活跃的技能实体
            foreach (var skill in mActiveSkills)
            {
                if (skill != null)
                {
                    Destroy(skill.gameObject);
                }
            }
            mActiveSkills.Clear();

            // 重置角色位置和状态
            if (mPlayer != null)
            {
                mPlayer.transform.position = mPlayerSpawnPos;
                mPlayer.Initialize(EntityType.Player);
            }

            if (mEnemy != null)
            {
                mEnemy.transform.position = mEnemySpawnPos;
                mEnemy.Initialize(EntityType.Enemy);
            }

            Debug.Log("[CombatTestRunner] 场景已重置");
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 创建圆形占位符 Sprite
        /// </summary>
        private Sprite CreateCircleSprite(int size, Color color)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;

            float radius = size / 2f;
            Vector2 center = new Vector2(radius, radius);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    if (distance <= radius - 1)
                    {
                        texture.SetPixel(x, y, color);
                    }
                    else if (distance <= radius)
                    {
                        // 边缘抗锯齿
                        float alpha = 1f - (distance - (radius - 1));
                        texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size // PPU
            );
        }

        #endregion

        #region 编辑器辅助

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // 绘制生成位置
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(mPlayerSpawnPos, 0.5f);
            UnityEditor.Handles.Label(mPlayerSpawnPos + Vector3.up, "Player Spawn");

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(mEnemySpawnPos, 0.5f);
            UnityEditor.Handles.Label(mEnemySpawnPos + Vector3.up, "Enemy Spawn");

            // 绘制战斗区域边界
            Gizmos.color = Color.yellow;
            float halfWidth = Core.CombatConfig.BATTLE_AREA_WIDTH / 2f;
            float halfHeight = Core.CombatConfig.BATTLE_AREA_HEIGHT / 2f;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(halfWidth * 2, halfHeight * 2, 0));
        }
#endif

        #endregion
    }
}
