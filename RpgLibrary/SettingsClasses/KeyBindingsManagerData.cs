using Microsoft.Xna.Framework.Input;
using System.Text;

namespace RpgLibrary.SettingsClasses
{
    public class KeyBindingsManagerData
    {
        public Dictionary<Keys, string> Keybindings { get;  set; } = new();

        public KeyBindingsManagerData() { }

        public KeyBindingsManagerData(Dictionary<Keys, string> keybindings)
        {
            Keybindings = keybindings;
        }

        public KeyBindingsManagerData Clone()
        {
            return new(Keybindings);
        }

        public override string ToString()
        {
            StringBuilder sb = new();

            sb.AppendLine("KeyBindingsManagerData: ");

            foreach (var obj in Keybindings)
                sb.AppendLine(obj.ToString() + ", ");

            return sb.ToString();
        }

        public string DebugString()
        {
            string output = string.Empty;

            foreach(var kvp in Keybindings)
                output += $"Key: {kvp.Key}, Attack Name: {kvp.Value}\n";

            return output;
        }
    }
}
