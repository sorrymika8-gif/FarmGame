using UnityEngine;
using FarmGame.Farm;
using FarmGame.Game.NPC;

namespace FarmGame.LLMCore.Brain
{
    public class PlantExecutor : ICommandExecutor
    {
        public string CommandType => CommandTypes.Plant;

        public void Execute(ICommand command, DecisionContext context)
        {
            if (command is not PlantCommand cmd) return;
            
            if (!context.Extra.TryGetValue("NPCEntity", out var entityObj) || entityObj is not NPCEntity npc)
            {
                Debug.LogError("[PlantExecutor] Missing NPCEntity in context.");
                return;
            }

            var targetPos = new Vector2Int(cmd.X, cmd.Y);
            if (Vector3.Distance(npc.Position, new Vector3(targetPos.x, targetPos.y, 0)) > npc.InteractionDistance)
            {
                Debug.LogWarning($"[PlantExecutor] Target too far: {targetPos}");
                return;
            }

            // Resolve Soil Entity
            var soil = FarmManager.Instance.GetSoil(targetPos);

            if (FarmManager.Instance.Plant(soil, cmd.ItemId, npc.Inventory))
            {
                npc.MemoryStore.GetOrCreatePartition("ShortTerm").Append($"我在 ({cmd.X}, {cmd.Y}) 种下了 {cmd.ItemId}。");
            }
            else
            {
                npc.MemoryStore.GetOrCreatePartition("ShortTerm").Append($"我在 ({cmd.X}, {cmd.Y}) 种植失败。");
            }
        }
    }
}
