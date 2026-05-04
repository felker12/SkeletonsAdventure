using RpgLibrary.AttackData;
using RpgLibrary.DataClasses;
using RpgLibrary.EntityClasses;
using RpgLibrary.MenuClasses;
using RpgLibrary.SettingsClasses;
using RpgLibrary.WorldClasses;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameMenu;
using SkeletonsAdventure.GameWorld;
using System.IO;

namespace SkeletonsAdventure.Engines
{
    internal class SaveEngine
    {
        public SaveEngine()
        {

        }

        public static void SaveGame(Game1 game, ExitScreenMenu exitScreen)
        {
            string savePath = GameManager.PathsLibrary.SavePath;
            Player player = World.Player;

            try
            {
                if (Directory.Exists(savePath) == false)
                    Directory.CreateDirectory(savePath);

                MenuManagerData GameScreenMenuData = new();
                foreach (BaseMenu baseMenu in game.GameScreen.Menus)
                {
                    if (baseMenu is TabbedMenu tabbedMenu)
                        GameScreenMenuData.Menus.Add(tabbedMenu.GetTabbedMenuData());
                    else if (baseMenu is not null)
                        GameScreenMenuData.Menus.Add(baseMenu.GetMenuData());
                }

                //save the data
                XnaSerializer.Serialize<WorldData>(savePath + @"\World.xml", World.GetWorldData());
                XnaSerializer.Serialize<MenuManagerData>(savePath + @"\GameScreenMenuData.xml", GameScreenMenuData);
                XnaSerializer.Serialize<TabbedMenuData>(savePath + @"\ExitScreenData.xml", exitScreen.GetTabbedMenuData());
                XnaSerializer.Serialize<List<String>>(Path.Combine(savePath, "MessageBox.xml"), game.GameScreen.MessageBox.Messages);
                XnaSerializer.Serialize<KeyBindingsManagerData>(savePath + @"\Keybindings.xml", player.KeybindingsManager.ToData());
                XnaSerializer.Serialize<LearnedAttackManagerData>(savePath + @"\LearnedAttackManager.Xml", player.LearnedAttackManager.ToData());
            }
            catch (Exception ex)
            {
                //TODO handle exception
                Debug.WriteLine(ex);
                return;
            }
        }
    }
}