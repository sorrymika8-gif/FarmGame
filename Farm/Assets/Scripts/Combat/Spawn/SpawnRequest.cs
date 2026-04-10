using UnityEngine;
using FarmGame.Combat.Data;
using FarmGame.Combat.Entity;

namespace FarmGame.Combat.Spawn
{
    /// <summary>
    /// 生成请求 - 描述一个待生成的技能实体
    /// </summary>
    public struct SpawnRequest
    {
        /// <summary>技能原子数据</summary>
        public SkillAtomData Data;

        /// <summary>生成位置</summary>
        public Vector3 Position;

        /// <summary>生成旋转</summary>
        public Quaternion Rotation;

        /// <summary>计划生成时间（Time.time + delay）</summary>
        public float ScheduledTime;

        /// <summary>技能所有者</summary>
        public CharEntity Owner;

        /// <summary>
        /// 创建生成请求
        /// </summary>
        /// <param name="data">技能数据</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <param name="delay">延迟秒数（0 = 立即）</param>
        /// <param name="owner">所有者</param>
        public SpawnRequest(
            SkillAtomData data,
            Vector3 position,
            Quaternion rotation,
            float delay = 0f,
            CharEntity owner = null)
        {
            Data = data;
            Position = position;
            Rotation = rotation;
            ScheduledTime = Time.time + delay;
            Owner = owner;
        }

        /// <summary>
        /// 是否已到达计划生成时间
        /// </summary>
        public bool IsReady => Time.time >= ScheduledTime;

        /// <summary>
        /// 距离计划生成时间还有多久
        /// </summary>
        public float TimeRemaining => Mathf.Max(0f, ScheduledTime - Time.time);
    }
}
