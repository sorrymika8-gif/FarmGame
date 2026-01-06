using UnityEngine;

namespace FarmGame.Player
{
    /// <summary>
    /// 玩家数据类
    /// 存储玩家的基础属性数据
    /// </summary>
    public class PlayerData
    {
        #region 属性

        /// <summary>
        /// 是否为新玩家（首次进入游戏）
        /// </summary>
        public bool IsNewPlayer { get; set; }

        /// <summary>
        /// 玩家位置
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// 玩家朝向（单位向量）
        /// </summary>
        public Vector3 FacingDirection { get; set; }

        /// <summary>
        /// 移动速度
        /// </summary>
        public float MoveSpeed { get; set; }

        #endregion

        #region 构造函数

        /// <summary>
        /// 创建玩家数据实例（使用默认值）
        /// </summary>
        public PlayerData()
        {
            // 硬编码默认值，后续接入配置系统
            IsNewPlayer = true;
            Position = Vector3.zero;
            FacingDirection = Vector3.down; // 默认朝下
            MoveSpeed = 5f; // 默认移动速度
        }

        #endregion
    }
}
