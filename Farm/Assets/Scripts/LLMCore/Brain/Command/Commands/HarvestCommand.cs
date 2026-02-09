using UnityEngine;

namespace FarmGame.LLMCore.Brain
{
    public class HarvestCommand : ICommand
    {
        public string CommandType => CommandTypes.Harvest;
        public int X;
        public int Y;
    }
}
