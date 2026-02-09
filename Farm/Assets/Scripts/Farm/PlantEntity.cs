using FarmGame.Item;
using FarmGame.GameConfig;
using FarmGame.GameConfig.Generated;
using UnityEngine;

namespace FarmGame.Farm
{
    /// <summary>
    /// Runtime instance of a planted crop.
    /// Inherits from ItemEntity because plants are specialized items in the world.
    /// </summary>
    public class PlantEntity : ItemEntity
    {
        // Current growth progress (accumulated maturity)
        public float CurrentMaturity { get; private set; }
        
        // Current visual stage index
        public int CurrentStageIndex { get; private set; }
        
        // Is fully mature and ready to harvest
        public bool IsMature { get; private set; }

        public PlantConfig PlantData => ConfigManager.Instance.GetConfig<PlantConfig>(ConfigId);

        public PlantEntity(int configId) : base(configId, 1)
        {
            CurrentMaturity = 0;
            CurrentStageIndex = 0;
            IsMature = false;
            UpdateStage();
        }

        /// <summary>
        /// Apply growth tick.
        /// </summary>
        /// <param name="delta">Maturity to add</param>
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

        private void UpdateStage()
        {
            if (PlantData == null || PlantData.maturity_stage == null) return;
            
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
        }
    }
}
