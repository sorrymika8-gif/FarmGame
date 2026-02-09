using UnityEngine;

namespace FarmGame.LLMCore.Brain
{
    public class TillCommand : ICommand
    {
        public string CommandType => CommandTypes.Till;
        public int X;
        public int Y;
    }
}
