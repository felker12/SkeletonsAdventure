using Microsoft.Xna.Framework.Input;
using RpgLibrary.SettingsClasses;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.Attacks
{
    internal class PlayerKeybindingsManager
    {
        public Player Player { get; init; }
        public Dictionary<Keys, BasicAttack> Keybindings { get; private set; } = [];
        public List<Keys> PossibleKeybindings { get; init; } = 
            [Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, 
            Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.D0];

        public PlayerKeybindingsManager(Player player)
        {
            Player = player;

            foreach (Keys key in PossibleKeybindings)
                SetKeybinding(key, null);
        }

        public void CheckInput(GameTime gameTime)
        {
            foreach (var keyBinding in Keybindings)
            {
                if (InputHandler.KeyReleased(keyBinding.Key))
                {
                    Player.PerformAttack(gameTime, keyBinding.Value);
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (var attack in Keybindings.Values)
            {
                if (attack is null)
                    continue;

                attack.UpdateCooldown(gameTime);
            }
        }

        public void SetKeybinding(Keys key, BasicAttack attack)
        {
            if (PossibleKeybindings.Contains(key) is false)
            {
                throw new ArgumentException($"Key {key} is not a valid keybinding option.");
            }

            attack?.Source = Player;
            Keybindings[key] = attack;
        }

        public void SetKeybinding(Dictionary<Keys, BasicAttack> keybindings)
        {
            foreach(var binding in keybindings)
                SetKeybinding(binding.Key, binding.Value);
        }

        public void SetKeybinding(Dictionary<Keys, string> keybindings)
        {
            foreach (var binding in keybindings)
                SetKeybinding(binding.Key, GameManager.GetAttackByName(binding.Value));
        }

        public void Clear() 
        { 
            Keybindings.Clear();
        }

        public KeyBindingsManagerData ToData()
        {
            Dictionary<Keys, string> bindings = [];

            foreach(var binding in Keybindings)
            {
                bindings.Add(binding.Key, binding.Value?.Name ?? string.Empty);
            }

            return new(bindings);
        }

        public override string ToString()
        {
            string toString = string.Empty;

            foreach(var binding in Keybindings)
                toString += binding.ToString();

            return toString;
        }
    }
}
