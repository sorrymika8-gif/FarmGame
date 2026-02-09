using System.Collections.Generic;
using UnityEngine;
using FarmGame.GameConfig.Generated;

namespace FarmGame.Farm
{
    /// <summary>
    /// Container for a collection of SoilEntities.
    /// Represents a specific farming area (e.g. Main Farm, NPC Backyard).
    /// </summary>
    public class FarmMapData
    {
        public string MapId { get; private set; }
        private Dictionary<Vector2Int, SoilEntity> mSoils = new Dictionary<Vector2Int, SoilEntity>();

        public FarmMapData(string mapId)
        {
            MapId = mapId;
        }

        public void AddSoil(SoilEntity soil)
        {
            mSoils[soil.GridPos] = soil;
        }

        public SoilEntity GetSoil(Vector2Int pos)
        {
            if (mSoils.TryGetValue(pos, out var soil)) return soil;
            return null;
        }

        public IEnumerable<SoilEntity> GetAllSoils() => mSoils.Values;
        
        public int Count => mSoils.Count;
    }
}
