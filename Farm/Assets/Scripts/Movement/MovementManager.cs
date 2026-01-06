using UnityEngine;
using QFramework;
using System.Collections.Generic;

namespace FarmGame.Movement
{
    /// <summary>
    /// 移动管理器
    /// 负责管理所有可移动实体，提供全局移动控制接口
    /// </summary>
    public class MovementManager : MonoSingleton<MovementManager>
    {
        #region 私有字段

        private bool mIsInitialized;
        private HashSet<Movable> mMovables = new HashSet<Movable>();

        #endregion

        #region 公共属性

        /// <summary>
        /// 是否启用全局移动（可用于暂停游戏时禁用所有移动）
        /// </summary>
        public bool GlobalMovementEnabled { get; set; } = true;

        /// <summary>
        /// 当前注册的可移动实体数量
        /// </summary>
        public int MovableCount => mMovables.Count;

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化移动管理器
        /// </summary>
        public void Initialize()
        {
            if (mIsInitialized) return;

            mMovables.Clear();

            mIsInitialized = true;
            Debug.Log("[MovementManager] Initialized");
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private void Dispose()
        {
            if (!mIsInitialized) return;

            mMovables.Clear();
            mIsInitialized = false;
        }

        #endregion

        #region 注册/注销

        /// <summary>
        /// 注册可移动实体
        /// </summary>
        /// <param name="movable">可移动组件</param>
        public void Register(Movable movable)
        {
            if (movable == null) return;

            if (mMovables.Add(movable))
            {
                Debug.Log($"[MovementManager] Registered: {movable.gameObject.name}");
            }
        }

        /// <summary>
        /// 注销可移动实体
        /// </summary>
        /// <param name="movable">可移动组件</param>
        public void Unregister(Movable movable)
        {
            if (movable == null) return;

            if (mMovables.Remove(movable))
            {
                Debug.Log($"[MovementManager] Unregistered: {movable.gameObject.name}");
            }
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 停止所有实体移动
        /// </summary>
        public void StopAll()
        {
            foreach (var movable in mMovables)
            {
                if (movable != null)
                {
                    movable.StopMovement();
                }
            }
        }

        /// <summary>
        /// 获取所有正在移动的实体
        /// </summary>
        /// <returns>正在移动的实体列表</returns>
        public List<Movable> GetMovingEntities()
        {
            List<Movable> moving = new List<Movable>();
            foreach (var movable in mMovables)
            {
                if (movable != null && movable.IsMoving)
                {
                    moving.Add(movable);
                }
            }
            return moving;
        }

        /// <summary>
        /// 检查是否有任何实体正在移动
        /// </summary>
        /// <returns>是否有实体正在移动</returns>
        public bool IsAnyMoving()
        {
            foreach (var movable in mMovables)
            {
                if (movable != null && movable.IsMoving)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 验证是否已初始化
        /// </summary>
        private bool ValidateInitialized()
        {
            if (!mIsInitialized)
            {
                Debug.LogError("[MovementManager] Not initialized. Call Initialize() first.");
                return false;
            }
            return true;
        }

        #endregion
    }
}
