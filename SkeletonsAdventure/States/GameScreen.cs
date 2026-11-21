using SkeletonsAdventure.Controls;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.Content;
using Microsoft.Xna.Framework.Input;
using SkeletonsAdventure.ItemClasses;
using RpgLibrary.WorldClasses;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.GameMenu;
using SkeletonsAdventure.GameUI;
using SkeletonsAdventure.Quests;
using SkeletonsAdventure.Entities.PlayerClasses;
using MessageBox = SkeletonsAdventure.GameUI.MessageBox;

namespace SkeletonsAdventure.States
{
    enum BoxSource { Game, Panel }
    internal class GameScreen : State
    {
        private const float ManualPickupReach = 48f;
        private MouseState _mouseState, _lastMouseState;
        private GameButton equip, unequip, pickUp, drop, consume;
        private GameItem itemUnderMouse = null;
        private Point _itemPopupAnchor;

        private Vector2 TransformedMousePosition => Vector2.Transform(new(_mouseState.X, _mouseState.Y), Matrix.Invert(Camera.Transformation));
        private Rectangle TransformedMouseRectangle => new((int)TransformedMousePosition.X, (int)TransformedMousePosition.Y, 1, 1);
        private Rectangle MouseRec => new(_mouseState.X, _mouseState.Y, 1, 1);

        private BoxSource CurrentSource { get; set; }
        public Camera Camera { get; set; }
        public World World { get; private set; }
        internal bool IsTestGame { get; }
        public Player Player { get; set; }
        public List<BaseMenu> Menus { get; set; } = [];
        public GameButtonBox GameItemPopUpBox { get; private set; }
        public BackpackMenu BackpackMenu { get; set; }
        private StatusBar HealthBar { get; set; }
        private StatusBar ManaBar { get; set; }
        private StatusBar XPProgress { get; set; }
        public Label QuestToDisplay { get; private set; }
        public MessageBox MessageBox { get; private set; }
        private ActionBar ActionBar { get; set; }

        //private FPSCounter FPS { get; set; } = new();

        public GameScreen(Game1 game, GameManager assets, bool testGame = false) : base(game, assets)
        {
            if (testGame && !Game1.IfDev)
                throw new InvalidOperationException("Test games require a development build.");
            IsTestGame = testGame;
            Initialize(useTestLoadout: testGame);
            MessageBox.AddMessage("Welcome!");
        }

        public GameScreen(Game1 game, GameManager assets, WorldData worldData, Texture2D playerTexture, bool testGame = false)
            : base(game, assets)
        {
            IsTestGame = testGame;
            Initialize();
            SaveManager.LoadGame(worldData, playerTexture);
        }

        private void Initialize(bool useTestLoadout = false)
        {
            QuestToDisplay = new(Assets.FontsLibrary.Arial12, "No Quest");
            MessageBox = new(Assets.FontsLibrary.Arial12);
            var player = new Player(Assets.DefaultEntityAssets, Assets.TexturesLibrary.ArmoredSkeletonTexture,
                Assets.AttackTemplates, GameManager.GetItemByName(DataAssetCatalog.Coins));
            if (useTestLoadout)
                player.EnableTestLoadout();
            World = new(Game.GameContent, GraphicsDevice, player, Assets.FontsLibrary.Arial12,
                Assets.FontsLibrary.Arial14, Assets.LeverAnimation);
            Camera = World.CurrentLevel.Camera;
            Player = World.CurrentLevel.Player;

            CreatePopUpBox();

            BackpackMenu = new(Assets.TexturesLibrary.BackpackBackground)
            {
                Visible = true,
            };

            Menus = [BackpackMenu];//TODO add more menus here when made

            CreateStatusBars();
            CreateActionBar();

            QuestToDisplay.Position = new(10, XPProgress.Position.Y + XPProgress.Height + 10);
            ControlManager.Add(QuestToDisplay);
            MessageBox.Position = new(4, Game1.ScreenHeight - MessageBox.Height - 4 - (Game1.ScreenHeight - ActionBar.Position.Y));
            MessageBox.Visible = false;
        }

        private void CreateActionBar()
        {
            ActionBar = new(
                Player.KeybindingsManager.Keybindings,
                Assets.FontsLibrary.Arial10,
                Assets.TexturesLibrary.CreateTextureFromColor(Color.White),
                slotSize: 36
            );

            float x = (Game1.ScreenWidth - ActionBar.TotalWidth) / 2f;
            float y = Game1.ScreenHeight - ActionBar.SlotSize - 10;
            ActionBar.Position = new(x, y);
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
            MessageBox.Draw(spriteBatch); //TODO

            ActionBar.Draw(spriteBatch);

            if (Camera.IsDetached)
            {
                const string hint = "Free camera  |  Arrows to pan  |  Wheel to zoom  |  Scroll Lock to follow";
                var font = Assets.FontsLibrary.Arial12;
                var position = new Vector2(GraphicsDevice.Viewport.Width - font.MeasureString(hint).X - 12, 12);
                spriteBatch.DrawString(font, hint, position + Vector2.One, Color.Black);
                spriteBatch.DrawString(font, hint, position, Color.White);
            }

            //FPS.Draw(spriteBatch, GameManager.InfoFont, new(10,10), Color.MonoGameOrange);

            spriteBatch.End();
        }

        public override void PostUpdate(GameTime gameTime) { }

        public override void Update(GameTime gameTime)
        {
            if (Game1.IfDev)
                UpdateFreeCamera(gameTime.ElapsedGameTime);

            CheckUnderMouse(gameTime);

            World.Update(gameTime, IsTestGame);

            UpdatePlayerStatusBars();

            UpdateQuestToDisplayLabel();
            ControlManager.Update(gameTime, PlayerIndexInControl);

            BackpackMenu.Update(World.CurrentLevel.EntityManager.Player.Backpack.Items);

            foreach (BaseMenu menu in Menus)
                menu.Update(gameTime);

            LoadMessagesFromWorld();
            MessageBox.Update();
            //FPS.Update(gameTime);
        }

        private void UpdateFreeCamera(TimeSpan elapsed)
        {
            if (InputHandler.KeyPressed(Keys.Scroll))
                Camera.ToggleDetached(Player.Position);
            if (!Camera.IsDetached)
                return;

            var direction = new Vector2(
                (InputHandler.KeyDown(Keys.Right) ? 1 : 0) - (InputHandler.KeyDown(Keys.Left) ? 1 : 0),
                (InputHandler.KeyDown(Keys.Down) ? 1 : 0) - (InputHandler.KeyDown(Keys.Up) ? 1 : 0));
            var mouse = InputHandler.MouseState;
            float wheelNotches = 0;
            if (!MessageBox.Visible || !MessageBox.Rectangle.Contains(mouse.Position))
                wheelNotches = (mouse.ScrollWheelValue - InputHandler.LastMouseState.ScrollWheelValue) / 120f;

            Camera.MoveDetached(direction, wheelNotches, elapsed);
        }

        internal void UpdatePlayerStatusBars()
        {
            var (Current, Required, AtMaximumLevel) = GameManager.XPTable.GetProgress(Player.TotalXP);
            HealthBar.Update(Player.Health, Player.MaxHealth, HealthBar.Position);
            ManaBar.Update(Player.Mana, Player.MaxMana, ManaBar.Position);
            XPProgress.Update(Current, Required, XPProgress.Position,
                AtMaximumLevel ? "MAX LEVEL" : null);
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
                if (itemUnderMouse != null && TransformedMouseRectangle.Intersects(itemUnderMouse.ItemRectangle))
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
                    SetItemUnderMouse(item, MouseRec, BoxSource.Panel);
                }
            }

            foreach (GameItem item in World.CurrentLevel.EntityManager.DroppedLootManager.Items)
            {
                Intersects(TransformedMouseRectangle, item.ItemRectangle, item, BoxSource.Game);
                SetItemUnderMouse(item, TransformedMouseRectangle, BoxSource.Game);
            }

            //hide popupbox when no more items are under mouse
            if (itemUnderMouse == null)
                GameItemPopUpBox.Visible = false;

            if (GameItemPopUpBox.Visible)
            {
                foreach (GameButton button in GameItemPopUpBox.Buttons)
                    button.Visible = false;

                if (CurrentSource == BoxSource.Game)
                    CheckMouseIntersectsInGame(gameTime, TransformedMouseRectangle);
                else if (CurrentSource == BoxSource.Panel)
                    CheckMouseIntersectsInPanel(gameTime, MouseRec);
            }
        }

        private void SetItemUnderMouse(GameItem item, Rectangle rectangle, BoxSource source)
        {
            if (GameItemPopUpBox.Visible)
            {
                if (source == CurrentSource && item.ItemRectangle.Contains(_itemPopupAnchor))
                    itemUnderMouse = item;
            }
            else if (rectangle.Intersects(item.ItemRectangle))
                itemUnderMouse = item;
        }

        private void CheckMouseIntersectsInPanel(GameTime gameTime, Rectangle mouseRec)
        {
            var viewport = GraphicsDevice.Viewport.Bounds;
            if (itemUnderMouse is EquipableItem equipment)
            {
                if (equipment.Equipped)
                {
                    unequip.Visible = true;
                    SetActionLabel(unequip, "Unequip");
                }
                else
                {
                    equip.Visible = true;
                    if (Player.EquippedItems.CheckRequirements(equipment))
                        SetActionLabel(equip, "Equip");
                    else
                        equip.SetText("Requirements not met");
                }
            }
            else if (itemUnderMouse is Consumable)
            {
                consume.Visible = true;
                SetActionLabel(consume, "Consume");
            }

            drop.Visible = true;
            SetActionLabel(drop, "Drop");
            GameItemPopUpBox.Position = _itemPopupAnchor.ToVector2();
            GameItemPopUpBox.Update(gameTime, false, Matrix.Identity, viewport);

            if (mouseRec.Intersects(GameItemPopUpBox.Rectangle) == false)
            {
                GameItemPopUpBox.Visible = false;
            }
            else
            {
                foreach (var item in BackpackMenu.Items)
                    item.ToolTip.Visible = false;
            }

            void SetActionLabel(GameButton button, string action)
            {
                string text = $"{action} {itemUnderMouse.Name}";
                float availableWidth = viewport.Width - GameItemPopUpBox.ControlOffset.X * 2;
                button.SetText(button.SpriteFont.MeasureString(text).X <= availableWidth ? text : action);
            }
        }

        private void CheckMouseIntersectsInGame(GameTime gameTime, Rectangle transformedMouseRectangle)
        {
            pickUp.Visible = true;
            pickUp.SetText(IsWithinPickupReach(itemUnderMouse)
                ? $"Pick Up {itemUnderMouse.Name}"
                : "Move closer to pick up");

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
                    _itemPopupAnchor = mouseRec.Location;
                    GameItemPopUpBox.Position = _itemPopupAnchor.ToVector2();
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
            Texture2D texture = Assets.TexturesLibrary.DefaultButtonTexture;
            SpriteFont font = Assets.FontsLibrary.Arial10;

            GameItemPopUpBox = new(Vector2.Zero, Assets.TexturesLibrary.ButtonBoxTexture, 100, 100)
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
            Texture2D statusBarTexture = Assets.TexturesLibrary.StatusBarTexture;
            SpriteFont font = Assets.FontsLibrary.Arial10;
            HealthBar = new(statusBarTexture, font)
            {
                Width = 200,
                Height = 18,
                BorderColor = Color.Black,
                BorderWidth = 2,
                BarColor = Color.Firebrick,
                TextVisible = true,
                Transparency = 0.5f,
            };

            ManaBar = new(statusBarTexture, font)
            {
                Width = HealthBar.Width,
                Height = HealthBar.Height,
                BorderColor = Color.Black,
                BarColor = Color.Blue,
                BorderWidth = 2,
                TextVisible = true,
                Transparency = HealthBar.Transparency,
            };

            XPProgress = new(statusBarTexture, font)
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
            if (itemUnderMouse != null && Player.Backpack.Items.Contains(itemUnderMouse))
            {
                Player.EquippedItems.TryUnequipItem(itemUnderMouse);
                World.CurrentLevel.EntityManager.DroppedLootManager.Add(itemUnderMouse, Player.Position - new Vector2(60, 40));
                Player.Backpack.Remove(itemUnderMouse);
            }
        }

        private void PickUp_Click(object sender, EventArgs e)
        {
            if (TryPickUpGroundItem(itemUnderMouse))
            {
                itemUnderMouse = null;
                GameItemPopUpBox.Visible = false;
            }
        }

        internal bool TryPickUpGroundItem(GameItem item) =>
            item is not null && IsWithinPickupReach(item) &&
            World.CurrentLevel.EntityManager.DroppedLootManager.TryPickUp(item, Player);

        private bool IsWithinPickupReach(GameItem item)
        {
            var itemCenter = item.Position + new Vector2(GameItem.Width, GameItem.Height) / 2;
            return IsTestGame || Vector2.DistanceSquared(Player.Center, itemCenter) <= ManualPickupReach * ManualPickupReach;
        }

        private void Unequip_Click(object sender, EventArgs e)
        {
            Player.EquippedItems.TryUnequipItem(itemUnderMouse);
        }

        private void Equip_Click(object sender, EventArgs e)
        {
            if (itemUnderMouse is not EquipableItem equipableItem)
                return;

            if(Player.EquippedItems.CheckRequirements(itemUnderMouse) is false)
            {
                World.MessagesToAdd.Add(equipableItem.LevelRequirements.MissingRequirementsText(Player));

                return;
            }

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
