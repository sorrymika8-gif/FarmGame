using UnityEngine;
using System.Collections.Generic;
using FarmGame.Combat.Core;
using FarmGame.Combat.Data;
using FarmGame.Combat.Entity;
using FarmGame.Combat.Spawn;

namespace FarmGame.Combat.Player
{
    /// <summary>
    /// 玩家战斗控制器 - 处理战斗中的玩家输入和技能释放
    /// </summary>
    [RequireComponent(typeof(CharEntity))]
    public class PlayerCombatController : MonoBehaviour
    {
        #region 序列化字段

        [Header("技能槽配置")]
        [SerializeField]
        private int mMaxSkillSlots = 4;

        [Header("瞄准配置")]
        [SerializeField]
        private bool mUseMouseAim = true;

        [SerializeField]
        private float mAimAssistRange = 5f;

        [SerializeField]
        private float mAimAssistAngle = 30f;

        [Header("视觉反馈")]
        [SerializeField]
        private LineRenderer mAimLineRenderer;

        [SerializeField]
        private float mAimLineLength = 3f;

        #endregion

        #region 私有字段

        private CharEntity mCharEntity;
        private Camera mMainCamera;
        private List<SkillSlot> mSkillSlots;
        private int mCurrentSkillIndex;
        private Vector2 mAimDirection;
        private Vector2 mInputDirection;
        private bool mIsAiming;
        private float mLastSkillTime;

        #endregion

        #region 公共属性

        /// <summary>关联的角色实体</summary>
        public CharEntity CharEntity => mCharEntity;

        /// <summary>当前瞄准方向</summary>
        public Vector2 AimDirection => mAimDirection;

        /// <summary>当前选中的技能索引</summary>
        public int CurrentSkillIndex => mCurrentSkillIndex;

        /// <summary>技能槽列表</summary>
        public IReadOnlyList<SkillSlot> SkillSlots => mSkillSlots;

        #endregion

        #region 生命周期

        private void Awake()
        {
            mCharEntity = GetComponent<CharEntity>();
            mMainCamera = Camera.main;

            // 初始化技能槽
            mSkillSlots = new List<SkillSlot>(mMaxSkillSlots);
            for (int i = 0; i < mMaxSkillSlots; i++)
            {
                mSkillSlots.Add(new SkillSlot());
            }

            // 设置默认瞄准方向
            mAimDirection = Vector2.right;
        }

        private void Start()
        {
            // 装备默认技能（测试用）
            EquipDefaultSkills();
        }

        private void Update()
        {
            if (mCharEntity == null || !mCharEntity.IsAlive) return;

            // 处理输入
            HandleMoveInput();
            HandleAimInput();
            HandleSkillInput();
            HandleSkillSwitch();

            // 更新冷却
            UpdateCooldowns();

            // 更新瞄准线
            UpdateAimLine();
        }

        private void FixedUpdate()
        {
            // 应用移动
            ApplyMovement();
        }

        #endregion

        #region 输入处理

        private void HandleMoveInput()
        {
            // WASD / 方向键移动
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            mInputDirection = new Vector2(horizontal, vertical);

            if (mInputDirection.sqrMagnitude > 1f)
            {
                mInputDirection.Normalize();
            }
        }

        private void HandleAimInput()
        {
            if (mUseMouseAim && mMainCamera != null)
            {
                // 鼠标瞄准
                Vector3 mousePos = mMainCamera.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0f;

                Vector2 toMouse = mousePos - transform.position;

                if (toMouse.sqrMagnitude > 0.01f)
                {
                    mAimDirection = toMouse.normalized;
                }
            }
            else
            {
                // 手柄/键盘瞄准 - 使用右摇杆或移动方向
                float aimH = Input.GetAxis("RightStickHorizontal");
                float aimV = Input.GetAxis("RightStickVertical");

                if (Mathf.Abs(aimH) > 0.1f || Mathf.Abs(aimV) > 0.1f)
                {
                    mAimDirection = new Vector2(aimH, aimV).normalized;
                }
                else if (mInputDirection.sqrMagnitude > 0.01f)
                {
                    // 没有瞄准输入时使用移动方向
                    mAimDirection = mInputDirection.normalized;
                }
            }

            // 瞄准辅助
            if (mAimAssistRange > 0f)
            {
                ApplyAimAssist();
            }
        }

        private void HandleSkillInput()
        {
            // 左键 / 按钮A - 释放当前技能
            if (Input.GetMouseButton(0) || Input.GetButton("Fire1"))
            {
                TryCastSkill(mCurrentSkillIndex);
            }

            // 右键 / 按钮B - 释放技能2
            if (Input.GetMouseButton(1) || Input.GetButton("Fire2"))
            {
                TryCastSkill(1);
            }

            // Q / 按钮X - 释放技能3
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetButtonDown("Fire3"))
            {
                TryCastSkill(2);
            }

            // E / 按钮Y - 释放技能4
            if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Jump"))
            {
                TryCastSkill(3);
            }
        }

        private void HandleSkillSwitch()
        {
            // 数字键 1-4 切换当前技能
            for (int i = 0; i < Mathf.Min(4, mMaxSkillSlots); i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    mCurrentSkillIndex = i;
                    Debug.Log($"[PlayerCombat] 切换到技能槽 {i + 1}");
                }
            }

            // 滚轮切换
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0.1f)
            {
                mCurrentSkillIndex = (mCurrentSkillIndex + 1) % mMaxSkillSlots;
            }
            else if (scroll < -0.1f)
            {
                mCurrentSkillIndex = (mCurrentSkillIndex - 1 + mMaxSkillSlots) % mMaxSkillSlots;
            }
        }

        #endregion

        #region 移动

        private void ApplyMovement()
        {
            if (mInputDirection.sqrMagnitude > 0.01f)
            {
                mCharEntity.SetMoveDirection(mInputDirection);
            }
            else
            {
                mCharEntity.StopMoving();
            }
        }

        #endregion

        #region 瞄准

        private void ApplyAimAssist()
        {
            // 查找瞄准方向附近的敌人
            var colliders = Physics2D.OverlapCircleAll(
                transform.position,
                mAimAssistRange
            );

            CharEntity bestTarget = null;
            float bestAngle = mAimAssistAngle;

            foreach (var col in colliders)
            {
                var target = col.GetComponent<CharEntity>();
                if (target == null) continue;
                if (target.EntityType != EntityType.Enemy) continue;
                if (!target.IsAlive) continue;

                Vector2 toTarget = (target.Position - transform.position).normalized;
                float angle = Vector2.Angle(mAimDirection, toTarget);

                if (angle < bestAngle)
                {
                    bestAngle = angle;
                    bestTarget = target;
                }
            }

            // 如果找到合适的目标，微调瞄准方向
            if (bestTarget != null)
            {
                Vector2 toTarget = (bestTarget.Position - transform.position).normalized;
                // 插值调整瞄准方向
                mAimDirection = Vector2.Lerp(mAimDirection, toTarget, 0.3f).normalized;
            }
        }

        private void UpdateAimLine()
        {
            if (mAimLineRenderer == null) return;

            mAimLineRenderer.enabled = true;
            mAimLineRenderer.positionCount = 2;
            mAimLineRenderer.SetPosition(0, transform.position);
            mAimLineRenderer.SetPosition(1, transform.position + (Vector3)(mAimDirection * mAimLineLength));
        }

        #endregion

        #region 技能释放

        private void TryCastSkill(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= mSkillSlots.Count) return;

            var slot = mSkillSlots[slotIndex];
            if (slot.IsEmpty) return;

            // 检查冷却
            if (slot.RemainingCooldown > 0f) return;

            // 检查技能间隔
            if (Time.time - mLastSkillTime < CombatConfig.MIN_SKILL_INTERVAL) return;

            // 检查沉默
            if (mCharEntity.Stats.IsSilenced) return;

            // 释放技能
            CastSkill(slot);
        }

        private void CastSkill(SkillSlot slot)
        {
            if (slot.Skill == null) return;

            // 克隆技能数据（避免修改原始数据）
            var skillData = slot.Skill.Clone();

            // 计算生成位置（角色前方一小段距离）
            Vector3 spawnPos = transform.position + (Vector3)(mAimDirection * 0.5f);

            // 计算旋转（2D中使用Z轴旋转）
            float angle = Mathf.Atan2(mAimDirection.y, mAimDirection.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            // 创建生成请求
            var request = new SpawnRequest(
                skillData,
                spawnPos,
                rotation,
                0f,
                mCharEntity
            );

            // 入队
            SpawnQueue.Instance.Enqueue(request);

            // 设置冷却
            slot.StartCooldown();
            mLastSkillTime = Time.time;

            Debug.Log($"[PlayerCombat] 释放技能: {skillData.displayName ?? "未命名技能"}");
        }

        #endregion

        #region 冷却更新

        private void UpdateCooldowns()
        {
            foreach (var slot in mSkillSlots)
            {
                slot.UpdateCooldown(Time.deltaTime);
            }
        }

        #endregion

        #region 技能管理

        /// <summary>
        /// 装备技能到指定槽位
        /// </summary>
        /// <param name="slotIndex">槽位索引</param>
        /// <param name="skill">技能数据</param>
        public void EquipSkill(int slotIndex, SkillAtomData skill)
        {
            if (slotIndex < 0 || slotIndex >= mSkillSlots.Count) return;

            mSkillSlots[slotIndex].SetSkill(skill);
            Debug.Log($"[PlayerCombat] 装备技能到槽位 {slotIndex + 1}: {skill?.displayName ?? "空"}");
        }

        /// <summary>
        /// 卸载指定槽位的技能
        /// </summary>
        /// <param name="slotIndex">槽位索引</param>
        public void UnequipSkill(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= mSkillSlots.Count) return;

            mSkillSlots[slotIndex].Clear();
        }

        /// <summary>
        /// 获取指定槽位的技能
        /// </summary>
        /// <param name="slotIndex">槽位索引</param>
        /// <returns>技能数据</returns>
        public SkillAtomData GetSkill(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= mSkillSlots.Count) return null;
            return mSkillSlots[slotIndex].Skill;
        }

        /// <summary>
        /// 装备默认技能（测试用）
        /// </summary>
        private void EquipDefaultSkills()
        {
            // 技能1：基础射击
            EquipSkill(0, new SkillAtomData
            {
                displayName = "基础射击",
                directHP = -15f,
                projectileSpeed = 12f,
                cooldown = 0.3f,
                pierce = 0,
                shape = ShapeType.Point,
                target = TargetType.SingleEnemy,
                trigger = TriggerType.Immediate
            });

            // 技能2：追踪弹
            EquipSkill(1, new SkillAtomData
            {
                displayName = "追踪弹",
                directHP = -10f,
                projectileSpeed = 8f,
                tracking = 60f,
                cooldown = 1f,
                shape = ShapeType.Point,
                target = TargetType.SingleEnemy,
                trigger = TriggerType.Immediate
            });

            // 技能3：散射
            EquipSkill(2, new SkillAtomData
            {
                displayName = "散射",
                directHP = -8f,
                projectileSpeed = 10f,
                split = 3,
                cooldown = 1.5f,
                shape = ShapeType.Point,
                target = TargetType.SingleEnemy,
                trigger = TriggerType.Immediate
            });

            // 技能4：AOE爆炸
            EquipSkill(3, new SkillAtomData
            {
                displayName = "爆炸弹",
                directHP = -20f,
                projectileSpeed = 6f,
                aoeRadius = 3f,
                cooldown = 3f,
                shape = ShapeType.Circle,
                target = TargetType.Area,
                trigger = TriggerType.Immediate
            });
        }

        #endregion

        #region 编辑器辅助

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // 绘制瞄准辅助范围
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, mAimAssistRange);

            // 绘制瞄准方向
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)(mAimDirection * mAimLineLength));

            // 绘制瞄准辅助扇形
            if (Application.isPlaying)
            {
                Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
                Vector3 leftBound = Quaternion.Euler(0, 0, mAimAssistAngle) * (Vector3)mAimDirection;
                Vector3 rightBound = Quaternion.Euler(0, 0, -mAimAssistAngle) * (Vector3)mAimDirection;
                Gizmos.DrawLine(transform.position, transform.position + leftBound * mAimAssistRange);
                Gizmos.DrawLine(transform.position, transform.position + rightBound * mAimAssistRange);
            }
        }
#endif

        #endregion
    }

    /// <summary>
    /// 技能槽 - 管理单个技能及其冷却
    /// </summary>
    [System.Serializable]
    public class SkillSlot
    {
        private SkillAtomData mSkill;
        private float mRemainingCooldown;

        /// <summary>装备的技能</summary>
        public SkillAtomData Skill => mSkill;

        /// <summary>剩余冷却时间</summary>
        public float RemainingCooldown => mRemainingCooldown;

        /// <summary>冷却进度（0-1）</summary>
        public float CooldownProgress => mSkill != null && mSkill.cooldown > 0
            ? 1f - mRemainingCooldown / mSkill.cooldown
            : 1f;

        /// <summary>是否为空槽</summary>
        public bool IsEmpty => mSkill == null;

        /// <summary>是否可用（非空且冷却完成）</summary>
        public bool IsReady => mSkill != null && mRemainingCooldown <= 0f;

        /// <summary>设置技能</summary>
        public void SetSkill(SkillAtomData skill)
        {
            mSkill = skill;
            mRemainingCooldown = 0f;
        }

        /// <summary>清空技能槽</summary>
        public void Clear()
        {
            mSkill = null;
            mRemainingCooldown = 0f;
        }

        /// <summary>开始冷却</summary>
        public void StartCooldown()
        {
            if (mSkill != null)
            {
                mRemainingCooldown = mSkill.cooldown;
            }
        }

        /// <summary>更新冷却</summary>
        public void UpdateCooldown(float deltaTime)
        {
            if (mRemainingCooldown > 0f)
            {
                mRemainingCooldown = Mathf.Max(0f, mRemainingCooldown - deltaTime);
            }
        }

        /// <summary>重置冷却</summary>
        public void ResetCooldown()
        {
            mRemainingCooldown = 0f;
        }
    }
}
