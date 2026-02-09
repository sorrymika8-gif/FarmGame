using UnityEngine;
using FarmGame.Item;

namespace FarmGame.Farm
{
    /// <summary>
    /// The Entity representing a piece of soil in the world.
    /// Holds state data.
    /// </summary>
    public class SoilEntity
    {
        public int ConfigId { get; private set; } // Soil Config Id
        public Vector2Int GridPos { get; private set; }
        public bool IsTilled { get; set; } = false;
        public PlantEntity Plant { get; set; } = null;

        public SoilEntity(int configId, int x, int y)
        {
            ConfigId = configId;
            GridPos = new Vector2Int(x, y);
        }

        public bool HasPlant => Plant != null;
    }
}
