using UnityEngine;

namespace FarmGame.LLMCore.Brain
{
    public class PlantCommand : ICommand
    {
        public string CommandType => CommandTypes.Plant;
        public int X;
        public int Y;
        public int ItemId;
    }
}
