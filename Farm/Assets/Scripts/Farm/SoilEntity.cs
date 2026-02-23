using System;
using UnityEngine;
using FarmGame.Item;

namespace FarmGame.Farm
{
    /// <summary>
    /// 土地实体，代表世界中的一块土地
    /// 持有状态数据
    /// </summary>
    public class SoilEntity
    {
        #region 事件

        /// <summary>
        /// 土地状态变化事件（耕地状态或作物变化时触发）
        /// </summary>
        public event Action<SoilEntity> OnStateChanged;

        #endregion

        #region 属性

        /// <summary>
        /// 土地配置ID
        /// </summary>
        public int ConfigId { get; private set; }

        /// <summary>
        /// 网格坐标
        /// </summary>
        public Vector2Int GridPos { get; private set; }

        private bool mIsTilled = false;
        /// <summary>
        /// 是否已耕地
        /// </summary>
        public bool IsTilled
        {
            get => mIsTilled;
            set
            {
                if (mIsTilled != value)
                {
                    mIsTilled = value;
                    OnStateChanged?.Invoke(this);
                }
            }
        }

        private PlantEntity mPlant = null;
        /// <summary>
        /// 当前种植的作物
        /// </summary>
        public PlantEntity Plant
        {
            get => mPlant;
            set
            {
                if (mPlant != value)
                {
                    // 取消订阅旧作物事件
                    if (mPlant != null)
                    {
                        mPlant.OnStageChanged -= OnPlantStageChanged;
                    }
                    
                    mPlant = value;
                    
                    // 订阅新作物事件
                    if (mPlant != null)
                    {
                        mPlant.OnStageChanged += OnPlantStageChanged;
                    }
                    
                    OnStateChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// 是否有作物
        /// </summary>
        public bool HasPlant => mPlant != null;

        #endregion

        #region 构造函数

        public SoilEntity(int configId, int x, int y)
        {
            ConfigId = configId;
            GridPos = new Vector2Int(x, y);
        }

        #endregion

        #region 私有方法

        private void OnPlantStageChanged(PlantEntity plant)
        {
            // 作物生长阶段变化时，也触发土地状态变化
            OnStateChanged?.Invoke(this);
        }

        #endregion
    }
}
