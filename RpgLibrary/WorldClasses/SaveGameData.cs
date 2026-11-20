using RpgLibrary.AttackData;
using RpgLibrary.MenuClasses;
using RpgLibrary.SettingsClasses;

namespace RpgLibrary.WorldClasses
{
    // One snapshot keeps the world, UI and player configuration in the same generation.
    public class SaveGameData
    {
        public int FormatVersion { get; set; } = 1;
        [Microsoft.Xna.Framework.Content.ContentSerializer(Optional = true)]
        public bool IsTestGame { get; set; }
        public WorldData World { get; set; } = new();
        public MenuManagerData Menus { get; set; } = new();
        public TabbedMenuData PauseMenu { get; set; } = new();
        public List<string> Messages { get; set; } = new();
        public KeyBindingsManagerData Bindings { get; set; } = new();
        public LearnedAttackManagerData LearnedAttacks { get; set; } = new();
    }
}
