
using Microsoft.Xna.Framework.Input;
using SkeletonsAdventure.Entities;

namespace SkeletonsAdventure.Attacks
{
    internal class PlayerKeybindingsManager(Player player)
    {
        public Player Player { get; init; } = player;
        public Dictionary<Keys, BasicAttack> Keybindings { get; private set; } = [];

        public List<Keys> PossibleKeybindings = 
            [Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, 
            Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.D0];

        public void SetKeybinding(Keys key, BasicAttack attack)
        {
            if (PossibleKeybindings.Contains(key) is false)
            {
                throw new ArgumentException($"Key {key} is not a valid keybinding option.");
            }

            Keybindings[key] = attack;
        }

        public void Clear() 
        { 
            Keybindings.Clear();
        }
    }
}
