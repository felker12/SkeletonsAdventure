using SkeletonsAdventure.Controls;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.GameMenu
{
    internal class ExitScreenMenu : TabbedMenu
    {
        //Create the menus
        public BaseMenu SaveMenu { get; set; }
        public BaseMenu Settings { get; private set; }
        public PlayerInfoMenu PlayerMenu { get; set; }
        public EvolutionMenu EvolutionMenu { get; set; }
        public QuestMenu QuestMenu { get; set; }
        public LearnSkillsMenu SkillsMenu { get; set; }
        public ActionBarMenu ActionBarMenu { get; set; }

        //Create labels
        public LinkLabel SaveGameLabel { get; set; }
        public LinkLabel ReturnToGameLabel { get; set; }
        public LinkLabel ReturnToMenuLabel { get; set; } 

        public ExitScreenMenu(int Width, int Height) : base(Width, Height)
        {
            Initialize();
        }

        private void Initialize()
        {
            Visible = false;
            Title = "ExitScreenMenu";

            CreateTabbedMenu();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void HandleInput(PlayerIndex playerIndex)
        {
            base.HandleInput(playerIndex);
        }

        private void CreateTabbedMenu()
        {
            //Create the child menus for the tabbed menu
            CreateSaveMenu();
            CreateSettingsMenu();
            CreatePlayerMenu();
            CreateQuestsMenu();
            CreateEvolutionMenu();
            CreateSkillsMenu();
            CreateActionBarMenu();

            //Add the menus to the tab bar
            AddMenu(SaveMenu);
            AddMenu(Settings);
            AddMenu(PlayerMenu);
            AddMenu(QuestMenu);
            AddMenu(EvolutionMenu);
            AddMenu(SkillsMenu);
            AddMenu(ActionBarMenu);

            TabBar.SetActiveTab(SaveMenu); //Set the active tab
        }

        public override void MenuOpened()
        {
            TabBar.ActiveMenu?.MenuOpened(); //Call MenuOpened on the active menu to update it
            base.MenuOpened(); //Call the base MenuOpened method to handle any additional logic

            //Update the Evolution menu visibility based on whether the player can evolve
            EvolutionMenu.Player = World.Player;
            EvolutionMenu.Visible = World.Player.CanEvolve; //should only be visible if the player can evolve

            //Find the Evolution tab and update its visibility
            foreach (var tabMenu in TabBar.TabMenus)
            {
                if (tabMenu.Value == EvolutionMenu)
                {
                    tabMenu.Key.Visible = World.Player.CanEvolve;

                    //If the Evolution tab is not visible and is currently active, switch to the first visible tab
                    if (TabBar.ActiveMenu == EvolutionMenu)
                        TabBar.SetActiveTab(TabBar.GetFirstVisibleMenu());

                    break;
                }
            }
        }

        private void CreateSaveMenu()
        {
            SaveMenu = new()
            {
                Visible = true,
                Title = "Save",
            };
            SaveMenu.SetBackgroundColor(Color.MidnightBlue);

            //Add controls to the Menu
            SaveGameLabel = new(GameManager.Arial20)
            {
                Text = "Press to save the game",
                TabStop = true,
            };
            SaveGameLabel.Position = new(Game1.ScreenWidth / 2 - SaveGameLabel.SpriteFont.MeasureString(SaveGameLabel.Text).X / 2, 300);

            ReturnToGameLabel = new(GameManager.Arial20)
            {
                Text = "Press to return to the game",
                TabStop = true,
            };
            ReturnToGameLabel.Position = new(Game1.ScreenWidth / 2 - ReturnToGameLabel.SpriteFont.MeasureString(ReturnToGameLabel.Text).X / 2, 350);

            ReturnToMenuLabel = new(GameManager.Arial20)
            {
                Text = "Press to return to menu screen",
                TabStop = true,
            };
            ReturnToMenuLabel.Position = new(Game1.ScreenWidth / 2 - ReturnToMenuLabel.SpriteFont.MeasureString(ReturnToMenuLabel.Text).X / 2,
                ReturnToGameLabel.Position.Y + ReturnToGameLabel.SpriteFont.MeasureString(ReturnToGameLabel.Text).Y + 20);


            SaveMenu.ControlManager.Add(SaveGameLabel);
            SaveMenu.ControlManager.Add(ReturnToGameLabel);
            SaveMenu.ControlManager.Add(ReturnToMenuLabel);

            SaveMenu.ControlManager.NextControl(); //Initialize the selected control and set the selected control to have focus
        }

        private void CreateSettingsMenu()
        {
            Settings = new()
            {
                Visible = true,
                Title = "Settings",
            };
            Settings.SetBackgroundColor(Color.MidnightBlue);
        }

        private void CreatePlayerMenu()
        {
            PlayerMenu = new PlayerInfoMenu()
            {
                Visible = true,
                Title = "Player",
            };
            PlayerMenu.SetBackgroundColor(Color.MidnightBlue);
        }

        private void CreateQuestsMenu()
        {
            QuestMenu = new()
            {
                Visible = true,
                Title = "Quests",
            };
            QuestMenu.SetBackgroundColor(Color.MidnightBlue);
        }

        private void CreateEvolutionMenu()
        {
            EvolutionMenu = new()
            {
                Visible = true,
                Title = "Evolution",
            };
            EvolutionMenu.SetBackgroundColor(Color.MidnightBlue);
        }

        private void CreateSkillsMenu()
        {
            SkillsMenu = new()
            {
                Visible = true,
                Title = "Skills",
            };
            SkillsMenu.SetBackgroundColor(Color.MidnightBlue);
        }

        private void CreateActionBarMenu()
        {
            ActionBarMenu = new()
            {
                Visible = true,
                Title = "Action Bar",
            };
            ActionBarMenu.SetBackgroundColor(Color.MidnightBlue);
        }
    }
}
