using UnityEngine;
using FarmGame.Farm;
using FarmGame.Game.NPC;

namespace FarmGame.LLMCore.Brain
{
    public class HarvestExecutor : ICommandExecutor
    {
        public string CommandType => CommandTypes.Harvest;

        public void Execute(ICommand command, DecisionContext context)
        {
            if (command is not HarvestCommand cmd) return;
            
            if (!context.Extra.TryGetValue("NPCEntity", out var entityObj) || entityObj is not NPCEntity npc)
            {
                Debug.LogError("[HarvestExecutor] Missing NPCEntity in context.");
                return;
            }

            var targetPos = new Vector2Int(cmd.X, cmd.Y);
            if (Vector3.Distance(npc.Position, new Vector3(targetPos.x, targetPos.y, 0)) > npc.InteractionDistance)
            {
                Debug.LogWarning($"[HarvestExecutor] Target too far: {targetPos}");
                return;
            }

            // Resolve Soil Entity
            var soil = FarmManager.Instance.GetSoil(targetPos);

            if (FarmManager.Instance.Harvest(soil, npc.Inventory))
            {
                npc.MemoryStore.GetOrCreatePartition("ShortTerm").Append($"我在 ({cmd.X}, {cmd.Y}) 收获了作物。");
            }
            else
            {
                npc.MemoryStore.GetOrCreatePartition("ShortTerm").Append($"我在 ({cmd.X}, {cmd.Y}) 收获失败。");
            }
        }
    }
}
