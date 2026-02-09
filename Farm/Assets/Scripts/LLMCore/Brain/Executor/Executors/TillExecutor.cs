using UnityEngine;
using FarmGame.Farm;
using FarmGame.Game.NPC;

namespace FarmGame.LLMCore.Brain
{
    public class TillExecutor : ICommandExecutor
    {
        public string CommandType => CommandTypes.Till;

        public void Execute(ICommand command, DecisionContext context)
        {
            if (command is not TillCommand cmd) return;
            
            if (!context.Extra.TryGetValue("NPCEntity", out var entityObj) || entityObj is not NPCEntity npc)
            {
                Debug.LogError("[TillExecutor] Missing NPCEntity in context.");
                return;
            }

            // Distance Check
            var targetPos = new Vector2Int(cmd.X, cmd.Y);
            if (Vector3.Distance(npc.Position, new Vector3(targetPos.x, targetPos.y, 0)) > npc.InteractionDistance)
            {
                Debug.LogWarning($"[TillExecutor] Target too far: {targetPos}");
                return;
            }

            // Resolve Soil Entity
            var soil = FarmManager.Instance.GetSoil(targetPos);
            
            if (FarmManager.Instance.Till(soil))
            {
                npc.MemoryStore.GetOrCreatePartition("ShortTerm").Append($"我在 ({cmd.X}, {cmd.Y}) 开垦了土地。");
            }
            else
            {
                npc.MemoryStore.GetOrCreatePartition("ShortTerm").Append($"我在 ({cmd.X}, {cmd.Y}) 开垦失败。");
            }
        }
    }
}
