using System;
using FarmGame.Item;
using FarmGame.GameConfig;
using FarmGame.GameConfig.Generated;
using FarmGame.Core.LLMDescription;
using UnityEngine;

namespace FarmGame.Farm
{
    /// <summary>
    /// 作物运行时实体
    /// 继承自 ItemEntity，因为作物是世界中的特殊物品
    /// 实现 IDescribable 接口以支持 LLM 生成描述
    /// </summary>
    public class PlantEntity : ItemEntity, IDescribable
    {
        #region 事件

        /// <summary>
        /// 生长阶段变化事件
        /// </summary>
        public event Action<PlantEntity> OnStageChanged;

        #endregion

        #region 属性

        /// <summary>
        /// 当前成熟度（累积值）
        /// </summary>
        [Describable("CurrentMaturity", "当前成熟度")]
        public float CurrentMaturity { get; private set; }

        /// <summary>
        /// 当前生长阶段索引
        /// </summary>
        [Describable("StageIndex", "生长阶段索引")]
        public int CurrentStageIndex { get; private set; }

        /// <summary>
        /// 是否已成熟可收获
        /// </summary>
        [Describable("IsHarvestable", "是否可收获")]
        public bool IsMature { get; private set; }

        /// <summary>
        /// 作物配置数据（种子配置）
        /// </summary>
        public SeedConfig PlantData => ConfigManager.Instance.GetConfig<SeedConfig>(ConfigId);

        /// <summary>
        /// 成熟进度百分比
        /// </summary>
        [Describable("MaturityPercent", "成熟进度百分比")]
        public float MaturityPercent
        {
            get
            {
                if (PlantData == null || PlantData.need_maturity <= 0) return 0;
                return (CurrentMaturity / PlantData.need_maturity) * 100f;
            }
        }

        /// <summary>
        /// 生长阶段名称
        /// </summary>
        [Describable("StageName", "生长阶段名称")]
        public string StageName
        {
            get
            {
                return CurrentStageIndex switch
                {
                    0 => "幼苗期",
                    1 => "生长期",
                    2 => "成熟期",
                    _ => "未知"
                };
            }
        }

        /// <summary>
        /// 作物名称（用于描述）
        /// </summary>
        [Describable("Name", "作物名称")]
        public string Name => PlantData?.name ?? "未知作物";

        #endregion

        #region IDescribable 实现

        /// <summary>
        /// 描述类型标识
        /// </summary>
        public string DescriptionType => "Crop";

        /// <summary>
        /// 获取显示名称
        /// </summary>
        public string GetDisplayName() => Name;

        /// <summary>
        /// 获取缓存键
        /// </summary>
        public string GetCacheKey() => $"crop_{ConfigId}_stage_{CurrentStageIndex}";

        #endregion

        #region 构造函数

        public PlantEntity(int configId) : base(configId, 1)
        {
            CurrentMaturity = 0;
            CurrentStageIndex = 0;
            IsMature = false;
            UpdateStage();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 应用生长周期
        /// </summary>
        /// <param name="delta">要增加的成熟度</param>
        public void Grow(float delta)
        {
            if (IsMature || PlantData == null) return;

            CurrentMaturity += delta;

            if (CurrentMaturity >= PlantData.need_maturity)
            {
                CurrentMaturity = PlantData.need_maturity;
                IsMature = true;
            }

            UpdateStage();
        }

        #endregion

        #region 私有方法

        private void UpdateStage()
        {
            if (PlantData == null || PlantData.maturity_stage == null) return;

            int oldStage = CurrentStageIndex;
            int newStage = 0;

            for (int i = 0; i < PlantData.maturity_stage.Length; i++)
            {
                if (CurrentMaturity >= PlantData.maturity_stage[i])
                {
                    newStage = i + 1;
                }
                else
                {
                    break;
                }
            }

            CurrentStageIndex = newStage;

            // 阶段变化时触发事件
            if (oldStage != newStage)
            {
                OnStageChanged?.Invoke(this);
            }
        }

        #endregion
    }
}
