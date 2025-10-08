using SkeletonsAdventure.Controls;
using SkeletonsAdventure.GameWorld;
using Microsoft.Xna.Framework.Input;
using SkeletonsAdventure.ItemClasses;
using SkeletonsAdventure.Entities;
using RpgLibrary.WorldClasses;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.GameMenu;
using SkeletonsAdventure.GameUI;
using SkeletonsAdventure.Quests;

namespace SkeletonsAdventure.States
{
    enum BoxSource { Game, Panel }
    internal class GameScreen : State
    {
        private MouseState _mouseState, _lastMouseState;
        private GameButton equip, unequip, pickUp, drop, consume;
        private GameItem itemUnderMouse = null;

        Vector2 TransformedMousePosition => Vector2.Transform(new(_mouseState.X, _mouseState.Y), Matrix.Invert(Camera.Transformation));
        Rectangle TransformedMouseRectangle => new((int)TransformedMousePosition.X, (int)TransformedMousePosition.Y, 1, 1);
        Rectangle MouseRec => new(_mouseState.X, _mouseState.Y, 1, 1);

        private BoxSource CurrentSource { get; set; }
        public Camera Camera { get; set; }
        public World World { get; private set; }
        public Player Player { get; set; }
        public List<BaseMenu> Menus { get; set; } = [];
        public GameButtonBox GameItemPopUpBox { get; private set; }
        public BackpackMenu BackpackMenu { get; set; }
        private StatusBar HealthBar { get; set; }
        private StatusBar ManaBar { get; set; }
        private StatusBar XPProgress { get; set; }
        public Label QuestToDisplay { get; set; } = new()
        {
            Text = "No Quest",
        };
        public MessageBox MessageBox { get; private set; } = new();

        //private FPSCounter FPS { get; set; } = new();

        public GameScreen(Game1 game) : base(game)
        {
            Initialize();
            MessageBox.AddMessage("Welcome!");
        }

        public GameScreen(Game1 game, WorldData worldData) : base(game)
        {
            Initialize();
            World.LoadWorldDataIntoLevels(worldData);
        }

        public void Initialize()
        {
            World = new(Content, GraphicsDevice);
            Camera = World.CurrentLevel.Camera;
            Player = World.CurrentLevel.Player;

            CreatePopUpBox();

            BackpackMenu = new()
            {
                Visible = true,
            };

            Menus = [BackpackMenu];//TODO add more menus here when made
            CreateStatusBars();

            QuestToDisplay.Position = new(10, XPProgress.Position.Y + XPProgress.Height + 10);
            ControlManager.Add(QuestToDisplay);
            MessageBox.Position = new(4, Game1.ScreenHeight - MessageBox.Height - 4);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            GraphicsDevice.Clear(Color.DarkCyan);

            World.Draw(spriteBatch);

            foreach (BaseMenu menu in Menus)
                menu.Draw(spriteBatch);

            spriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null,
                null,
                null,
                Camera.Transformation);

            if (CurrentSource == BoxSource.Game && GameItemPopUpBox.Visible)
            {
                GameItemPopUpBox.Draw(spriteBatch);
            }

            spriteBatch.End();

            spriteBatch.Begin();

            if (CurrentSource == BoxSource.Panel && GameItemPopUpBox.Visible)
            {
                GameItemPopUpBox.Draw(spriteBatch);
            }

            HealthBar.Draw(spriteBatch);
            ManaBar.Draw(spriteBatch);
            XPProgress.Draw(spriteBatch);

            ControlManager.Draw(spriteBatch);
            MessageBox.Draw(spriteBatch);

            //FPS.Draw(spriteBatch, GameManager.InfoFont, new(10,10), Color.MonoGameOrange);

            spriteBatch.End();
        }

        public override void PostUpdate(GameTime gameTime) { }

        public override void Update(GameTime gameTime)
        {
            CheckUnderMouse(gameTime);
            World.Update(gameTime);

            int lvlXP = GameManager.GetLevelXPAtLevel(Player.Level);
            int nextLvlXP = GameManager.GetLevelXPAtLevel(Player.Level + 1);
            int playerXPToLevel = nextLvlXP - lvlXP;
            int playerXPSinceLastLevel = Player.TotalXP - lvlXP;

            HealthBar.Update(Player.Health, Player.MaxHealth, HealthBar.Position);
            ManaBar.Update(Player.Mana, Player.MaxMana, ManaBar.Position);
            XPProgress.Update(playerXPSinceLastLevel, playerXPToLevel, XPProgress.Position);

            UpdateQuestToDisplayLabel();
            ControlManager.Update(gameTime, PlayerIndexInControl);

            BackpackMenu.Update(World.CurrentLevel.EntityManager.Player.Backpack.Items);

            foreach (BaseMenu menu in Menus)
                menu.Update(gameTime);

            LoadMessagesFromWorld();
            MessageBox.Update();
            //FPS.Update(gameTime);
        }

        public override void HandleInput(PlayerIndex playerIndex)
        {
            if (InputHandler.KeyReleased(Keys.I))
            {
                BackpackMenu.ToggleVisibility();
            }

            if (InputHandler.KeyReleased(Keys.V)) //TODO
            {
                HealthBar.ToggleVisibility();
                ManaBar.ToggleVisibility();
                XPProgress.ToggleVisibility();
            }

            if (InputHandler.KeyReleased(Keys.Q))
            {
                //TODO
                //System.Diagnostics.Debug.WriteLine(World.CurrentLevel.EntityManager.Entities.Count);
            }

            if (InputHandler.KeyReleased(Keys.C))
            {
                MessageBox.ToggleVisibility();
            }

            if (_mouseState.LeftButton == ButtonState.Released && _lastMouseState.LeftButton == ButtonState.Pressed)
            {
                // Only try to pick up items if they're in the game world (not in backpack)
                if (itemUnderMouse != null &&
                    !BackpackMenu.Items.Contains(itemUnderMouse) && // Check if item is NOT in backpack
                    TransformedMouseRectangle.Intersects(itemUnderMouse.ItemRectangle))
                {
                    PickUp_Click(null, new EventArgs());
                }
            }

            World.HandleInput(playerIndex);

            if (GameItemPopUpBox.Visible)
                GameItemPopUpBox.HandleInput(playerIndex);

            MessageBox.HandleInput();
        }

        private void UpdateQuestToDisplayLabel()
        {
            Quest quest = Player.GetActiveQuestByName(Player.DisplayQuestName);

            if(quest is null)
            {
                QuestToDisplay.Visible = false;
                QuestToDisplay.Text = string.Empty;
            }
            else
            {
                QuestToDisplay.Visible = true;
                QuestToDisplay.Text = quest.QuestDetails;
            }
        }

        private void LoadMessagesFromWorld()
        {
            MessageBox.AddMessages(World.MessagesToAdd);
            World.MessagesToAdd.Clear();
        }

        private void CheckUnderMouse(GameTime gameTime)
        {
            _lastMouseState = _mouseState;
            _mouseState = Mouse.GetState();

            itemUnderMouse = null;

            if (BackpackMenu.Visible)
            {
                foreach (GameItem item in BackpackMenu.Items)
                {
                    Intersects(MouseRec, item.ItemRectangle, item, BoxSource.Panel);
                    SetItemUnderMouse(item, MouseRec);
                }
            }

            foreach (GameItem item in World.CurrentLevel.EntityManager.DroppedLootManager.Items)
            {
                Intersects(TransformedMouseRectangle, item.ItemRectangle, item, BoxSource.Game);
                SetItemUnderMouse(item, TransformedMouseRectangle);
            }

            //hide popupbox when no more items are under mouse
            if (itemUnderMouse == null)
                GameItemPopUpBox.Visible = false;

            //TODO
            //if (_mouseState.LeftButton == ButtonState.Released && _lastMouseState.LeftButton == ButtonState.Pressed)
            //{
            //    // Only try to pick up items if they're in the game world (not in backpack)
            //    if (itemUnderMouse != null &&
            //        !BackpackMenu.Items.Contains(itemUnderMouse) && // Check if item is NOT in backpack
            //        TransformedMouseRectangle.Intersects(itemUnderMouse.ItemRectangle))
            //    {
            //        PickUp_Click(null, new EventArgs());
            //    }
            //}

            if (GameItemPopUpBox.Visible)
            {
                foreach (GameButton button in GameItemPopUpBox.Buttons)
                    button.Visible = false;

                if (CurrentSource == BoxSource.Game)
                    CheckMouseIntersectsInGame(gameTime, TransformedMouseRectangle);
                else if (CurrentSource == BoxSource.Panel)
                    CheckMouseIntersectsInPanel(gameTime, MouseRec);

                consume.Text = $"Consume {itemUnderMouse.Name}";
                drop.Text = $"Drop {itemUnderMouse.Name}";
                equip.Text = $"Equip {itemUnderMouse.Name}";
                unequip.Text = $"Unequip {itemUnderMouse.Name}";
                pickUp.Text = $"Pick Up {itemUnderMouse.Name}";
            }
        }

        private void SetItemUnderMouse(GameItem item, Rectangle rectangle)
        {
            if (GameItemPopUpBox.Visible)
            {
                Rectangle rec = new((int)GameItemPopUpBox.Position.X, (int)GameItemPopUpBox.Position.Y, 1, 1);
                if (rec.Intersects(item.ItemRectangle))
                    itemUnderMouse = item;
            }
            else if (rectangle.Intersects(item.ItemRectangle))
                itemUnderMouse = item;
        }

        private void CheckMouseIntersectsInPanel(GameTime gameTime, Rectangle mouseRec)
        {
            if (itemUnderMouse != null)
            {
                if (itemUnderMouse is EquipableItem equipable == true)
                {
                    if (equipable.Equipped == true)
                        unequip.Visible = true;
                    else if (equipable.Equipped == false)
                        equip.Visible = true;
                }
                else if (itemUnderMouse is Consumable)
                {
                    consume.Visible = true;
                }
            }

            drop.Visible = true;

            GameItemPopUpBox.Update(gameTime, false, Camera.Transformation);

            if (mouseRec.Intersects(GameItemPopUpBox.Rectangle) == false)
            {
                GameItemPopUpBox.Visible = false;
            }
        }

        private void CheckMouseIntersectsInGame(GameTime gameTime, Rectangle transformedMouseRectangle)
        {
            pickUp.Visible = true;

            GameItemPopUpBox.Update(gameTime, true, Camera.Transformation);

            if (transformedMouseRectangle.Intersects(GameItemPopUpBox.Rectangle) == false)
                GameItemPopUpBox.Visible = false;
        }

        private bool Intersects(Rectangle mouseRec, Rectangle itemRec, GameItem item, BoxSource source)
        {
            bool intersects = false;
            if (mouseRec.Intersects(itemRec))
            {
                item.ToolTip.Visible = true;
                if (_mouseState.RightButton == ButtonState.Released && _lastMouseState.RightButton == ButtonState.Pressed)
                {
                    CurrentSource = source;
                    Vector2 mousePos = new(mouseRec.X, mouseRec.Y);

                    GameItemPopUpBox.Position = mousePos;
                    GameItemPopUpBox.Visible = true;
                    intersects = true;
                }
            }
            else
                item.ToolTip.Visible = false;

            return intersects;
        }

        public void CreatePopUpBox()
        {
            Texture2D texture = GameManager.DefaultButtonTexture;
            SpriteFont font = GameManager.Arial10;

            GameItemPopUpBox = new(Vector2.Zero, GameManager.ButtonBoxTexture, 100, 100)
            {
                Visible = false,
            };

            equip = new(texture, font);
            unequip = new(texture, font);
            pickUp = new(texture, font);
            consume = new(texture, font);
            drop = new(texture, font);

            equip.Click += Equip_Click;
            unequip.Click += Unequip_Click;
            pickUp.Click += PickUp_Click;
            consume.Click += Consume_Click;
            drop.Click += Drop_Click;

            GameItemPopUpBox.AddButton(equip, "Equip Item");
            GameItemPopUpBox.AddButton(unequip, "Unequip Item");
            GameItemPopUpBox.AddButton(pickUp, "Pick Up Item");
            GameItemPopUpBox.AddButton(consume, "Consume Item");
            GameItemPopUpBox.AddButton(drop, "Drop Item");
        }

        private void CreateStatusBars()
        {
            HealthBar = new()
            {
                Width = 200,
                Height = 18,
                BorderColor = Color.Black,
                BorderWidth = 2,
                BarColor = Color.Firebrick,
                TextVisible = true,
                Transparency = 0.5f,
            };

            ManaBar = new()
            {
                Width = HealthBar.Width,
                Height = HealthBar.Height,
                BorderColor = Color.Black,
                BarColor = Color.Blue,
                BorderWidth = 2,
                TextVisible = true,
                Transparency = HealthBar.Transparency,
            };

            XPProgress = new()
            {
                Width = HealthBar.Width,
                Height = HealthBar.Height,
                BorderColor = Color.Black,
                BarColor = Color.DarkSlateGray,
                BorderWidth = 2,
                TextVisible = true,
                Transparency = HealthBar.Transparency,
            };

            //Position the status bars
            HealthBar.Position = new(4, 20);
            ManaBar.Position = HealthBar.Position + new Vector2(0, HealthBar.Height + HealthBar.BorderWidth);
            XPProgress.Position = ManaBar.Position + new Vector2(0, ManaBar.Height + ManaBar.BorderWidth);
        }


        private void Consume_Click(object sender, EventArgs e)
        {
            if (itemUnderMouse != null && itemUnderMouse is Consumable)
                Player.ConsumeItem(itemUnderMouse);
        }

        private void Drop_Click(object sender, EventArgs e)
        {
            if (itemUnderMouse != null)
            {
                Player.EquippedItems.TryUnequipItem(itemUnderMouse);
                World.CurrentLevel.EntityManager.DroppedLootManager.Add(itemUnderMouse, Player.Position - new Vector2(60, 40));
                Player.Backpack.Remove(itemUnderMouse);
            }
        }

        private void PickUp_Click(object sender, EventArgs e)
        {
            if (itemUnderMouse != null && Player.Backpack.Add(itemUnderMouse) == true)
                World.CurrentLevel.EntityManager.DroppedLootManager.Remove(itemUnderMouse);
        }

        private void Unequip_Click(object sender, EventArgs e)
        {
            Player.EquippedItems.TryUnequipItem(itemUnderMouse);
        }

        private void Equip_Click(object sender, EventArgs e)
        {
            Player.EquippedItems.TryEquipItem(itemUnderMouse);
        }

        public override void StateChangeToHandler()
        {
        }

        public override void StateChangeFromHandler()
        {
        }
    }
}
