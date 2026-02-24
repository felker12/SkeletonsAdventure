using Microsoft.Xna.Framework.Input;
using RpgLibrary.SettingsClasses;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.Entities.PlayerClasses
{
    public class KeybindingsManager
    {
        public Player Player { get; init; }
        public Dictionary<Keys, BasicAttack> Keybindings { get; private set; } = [];

        //default possible keybindings are 0..9 (Keys.D0..Keys.D9) but this can be changed in the GameManager.PossibleKeybindings list
        public List<Keys> PossibleKeybindings = GameManager.PossibleKeybindings;

        public KeybindingsManager(Player player)
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

        public bool SetKeybinding(Keys key, BasicAttack attack)
        {
            if (PossibleKeybindings.Contains(key) is false)
            {
                //throw new ArgumentException($"Key {key} is not a valid keybinding option.");
                Debug.WriteLine($"Key {key} is not a valid keybinding option.");
                return false;
            }

            attack?.Source = Player;
            Keybindings[key] = attack;

            return true;
        }

        public void SetKeybinding(Dictionary<Keys, BasicAttack> keybindings)
        {
            foreach(var binding in keybindings)
                SetKeybinding(binding.Key, binding.Value);
        }

        public void RemoveKeybinding(Keys key)
        {
            if (Keybindings.ContainsKey(key))
                Keybindings[key] = null;
        }

        public void Clear() 
        { 
            Keybindings.Clear();
        }

        public KeyBindingsManagerData ToData()
        {
            Dictionary<Keys, string> bindings = [];

            foreach(var binding in Keybindings)
                bindings.Add(binding.Key, binding.Value?.Name ?? string.Empty);

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
