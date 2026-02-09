using System;

namespace FarmGame.GameConfig
{
    [Serializable]
    public class ItemConfig
    {
        public int id;
        public string name;
        public string description;
        public int max_stack;
        // Type: 1=Seed, 2=Product, 3=Tool
        public int type; 
        // If type==Seed, this links to PlantConfig.class_id
        public int function_args; 
    }
}
