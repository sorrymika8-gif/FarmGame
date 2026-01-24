using System;

namespace FarmGame.LLMCore.Brain
{
    /// <summary>
    /// 角色设定
    /// 定义角色的名称、性格、背景故事等
    /// </summary>
    [Serializable]
    public class CharacterSetting
    {
        public string Name;
        public string Persona; // 人设/性格
        public string Background; // 背景故事

        public CharacterSetting(string name, string persona, string background)
        {
            Name = name;
            Persona = persona;
            Background = background;
        }

        public string ToPrompt()
        {
            return $"Name: {Name}\nPersona: {Persona}\nBackground: {Background}";
        }
    }
}
