using SkeletonsAdventure.Controls;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.GameMenu
{
    internal class PlayerInfoMenu : BaseMenu
    {
        public Player Player { get; private set; }

        //Controls for displaying player information can be added here
        Label LevelLbl, AttributePointsLbl, BaseAttackLbl, BaseDefenceLbl,
            BaseHealthLbl, BaseManaLbl, AttackLbl, DefenceLbl,
            HealthLbl, ManaLbl, AttackToAddLabel,
            DefenceToAddLabel, HealthToAddLabel, ManaToAddLabel;

        Button ApplyBtn, CancelBtn, IncreaseAttackBtn, IncreaseDefenceBtn, IncreaseHealthBtn, IncreaseManaBtn;

        private readonly List<Button> _increaseFiveButtons = [];

        private int currentAttributePoints,
            attackToAdd = 0, defenceToAdd = 0, healthToAdd = 0, manaToAdd = 0;

        public PlayerInfoMenu(Texture2D texture, Texture2D buttonTexture, SpriteFont font, Color color)
            : base(texture, color)
        {
            CreateControls(buttonTexture, font);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void MenuOpened()
        {
            UpdateWithPlayer(World.Player);
            ResetAttributePoints();
            CheckApplyAndCancelShouldBeEnabled();
        }

        public void UpdateWithPlayer(Player player)
        {
            Player = player;
            currentAttributePoints = player.AttributePoints;

            UpdateLabelText(player);
            SetPositions(); 
            CheckEnoughAttributePoints();
            CheckApplyAndCancelShouldBeEnabled();
        }

        public void ResetAttributePoints()
        {
            attackToAdd = 0;
            defenceToAdd = 0;
            healthToAdd = 0;
            manaToAdd = 0;
            SetToAddLabelsText();
            CheckEnoughAttributePoints();
        }

        private void UpdateLabelText(Player player)
        {
            //Update the labels with the player's information
            LevelLbl.Text = $"Level: {player.Level}";
            AttributePointsLbl.Text = $"Total Attribute Points: {player.AttributePoints}";
            BaseAttackLbl.Text = $"Base Attack: {player.BaseAttack}";
            BaseDefenceLbl.Text = $"Base Defence: {player.BaseDefence}";
            BaseHealthLbl.Text = $"Base Health: {player.BaseHealth}";
            BaseManaLbl.Text = $"Base Mana: {player.BaseMana}";

            AttackLbl.Text = $"Total Attack: {player.Attack}, Base Attack: {player.BaseAttack}, " +
                $"Level: {player.bonusAttackFromLevel}, " +
                $"Attributes: {player.bonusAttackFromAttributePoints}, " +
                $"Gear: {player.EquippedItems.EquippedItemsAttackBonus()}," +
                $"Evolution: {player.bonusAttackFromEvolution}";
            DefenceLbl.Text = $"Total Defence: {player.Defence}, Base Defence: {player.BaseDefence}, " +
                $"Level: {player.bonusDefenceFromLevel}, " +
                $"Attributes: {player.bonusDefenceFromAttributePoints}, " +
                $"Gear: {player.EquippedItems.EquippedItemsDefenceBonus()}, " +
                $"Evolution: {player.bonusDefenceFromEvolution}";
            HealthLbl.Text = $"Total Health: {player.MaxHealth}, Base Health: {player.BaseHealth}, " +
                $"Level: {player.bonusHealthFromLevel}, " +
                $"Attributes: {player.bonusHealthFromAttributePoints}, " +
                $"Gear: {player.EquippedItems.EquippedItemsHealthBonus()}, " +
                $"Evolution: {player.bonusHealthFromEvolution}";
            ManaLbl.Text = $"Mana: {player.MaxMana}, Base Mana: {player.BaseMana}, " +
                $"Level: {player.bonusManaFromLevel}, " +
                $"Attributes: {player.bonusManaFromAttributePoints}, " +
                $"Gear: {player.EquippedItems.EquippedItemsManaBonus()}, " +
                $"Evolution: {player.bonusManaFromEvolution}";

            SetToAddLabelsText();
        }

        private void SetToAddLabelsText()
        {
            AttackToAddLabel.Text = attackToAdd.ToString();
            DefenceToAddLabel.Text = defenceToAdd.ToString();
            HealthToAddLabel.Text = healthToAdd.ToString();
            ManaToAddLabel.Text = manaToAdd.ToString();
            AttributePointsLbl.Text = "Total Attribute Points: " + currentAttributePoints.ToString();
        }

        private void CheckEnoughAttributePoints()
        {
            IncreaseAttackBtn.Enabled = currentAttributePoints >= 1;
            IncreaseDefenceBtn.Enabled = currentAttributePoints >= 1;
            IncreaseHealthBtn.Enabled = currentAttributePoints >= 1;
            IncreaseManaBtn.Enabled = currentAttributePoints >= 1;
            foreach (Button button in _increaseFiveButtons)
                button.Enabled = currentAttributePoints >= 5;
        }
        public override void Arrange(Rectangle bounds)
        {
            base.Arrange(bounds);
            if (Player is not null)
                SetPositions();
        }

        private void SetPositions()
        {
            int labelSpace = 10;

            //get the longest label width to position the buttons correctly
            List<Label> labels = [BaseAttackLbl, BaseDefenceLbl, BaseHealthLbl, BaseManaLbl];
            int longestLbl = GetLongestLabel(labels);

            //Position the controls
            LevelLbl.Position = new(Position.X + 26, Position.Y + 26);
            AttributePointsLbl.Position = LevelLbl.Position +
                new Vector2(0, LevelLbl.SpriteFont.LineSpacing + labelSpace);
            BaseAttackLbl.Position = AttributePointsLbl.Position +
                new Vector2(0, AttributePointsLbl.SpriteFont.LineSpacing + labelSpace);
            BaseDefenceLbl.Position = BaseAttackLbl.Position +
                new Vector2(0, BaseAttackLbl.SpriteFont.LineSpacing + labelSpace);
            BaseHealthLbl.Position = BaseDefenceLbl.Position +
                new Vector2(0, BaseDefenceLbl.SpriteFont.LineSpacing + labelSpace);
            BaseManaLbl.Position = BaseHealthLbl.Position +
                new Vector2(0, BaseHealthLbl.SpriteFont.LineSpacing + labelSpace);

            ApplyBtn.Position = BaseManaLbl.Position +
                new Vector2(0, BaseManaLbl.SpriteFont.LineSpacing + 20);
            CancelBtn.Position = ApplyBtn.Position +
                new Vector2(ApplyBtn.Width + labelSpace, 0);

            AttackLbl.Position = new Vector2(BaseManaLbl.Position.X,
                CancelBtn.Position.Y + CancelBtn.SpriteFont.LineSpacing + CancelBtn.Height + labelSpace);
            DefenceLbl.Position = AttackLbl.Position +
                new Vector2(0, AttackLbl.SpriteFont.LineSpacing + labelSpace);
            HealthLbl.Position = DefenceLbl.Position +
                new Vector2(0, DefenceLbl.SpriteFont.LineSpacing + labelSpace);
            ManaLbl.Position = HealthLbl.Position +
                new Vector2(0, HealthLbl.SpriteFont.LineSpacing + labelSpace);

            IncreaseAttackBtn.Position = new(LevelLbl.Position.X + labelSpace * 2 + longestLbl, BaseAttackLbl.Position.Y);
            IncreaseDefenceBtn.Position = new(LevelLbl.Position.X + labelSpace * 2 + longestLbl, BaseDefenceLbl.Position.Y);
            IncreaseHealthBtn.Position = new(LevelLbl.Position.X + labelSpace * 2 + longestLbl, BaseHealthLbl.Position.Y);
            IncreaseManaBtn.Position = new(LevelLbl.Position.X + labelSpace * 2 + longestLbl, BaseManaLbl.Position.Y);

            Button[] increaseOneButtons = [IncreaseAttackBtn, IncreaseDefenceBtn, IncreaseHealthBtn, IncreaseManaBtn];
            for (int i = 0; i < increaseOneButtons.Length; i++)
            {
                _increaseFiveButtons[i].Position = increaseOneButtons[i].Position +
                    new Vector2(increaseOneButtons[i].Width + labelSpace, 0);
            }

            AttackToAddLabel.Position = _increaseFiveButtons[0].Position +
                new Vector2(IncreaseAttackBtn.Width + labelSpace, 0);
            DefenceToAddLabel.Position = _increaseFiveButtons[1].Position +
                new Vector2(IncreaseDefenceBtn.Width + labelSpace, 0);
            HealthToAddLabel.Position = _increaseFiveButtons[2].Position +
                new Vector2(IncreaseHealthBtn.Width + labelSpace, 0);
            ManaToAddLabel.Position = _increaseFiveButtons[3].Position +
                new Vector2(IncreaseManaBtn.Width + labelSpace, 0);
        }

        private static int GetLongestLabel(List<Label> labels)
        {
            int longest = 0;

            foreach(Label lbl in labels)
            {
                if(lbl.SpriteFont.MeasureString(lbl.Text).X > longest)
                {
                    longest = (int)lbl.SpriteFont.MeasureString(lbl.Text).X;
                }
            }

            return longest;
        }

        private void CreateControls(Texture2D buttonTexture, SpriteFont font)
        {
            LevelLbl = new(font);
            AttributePointsLbl = new(font);
            BaseAttackLbl = new(font);
            BaseDefenceLbl = new(font);
            BaseHealthLbl = new(font);
            BaseManaLbl = new(font);
            AttackLbl = new(font);
            DefenceLbl = new(font);
            HealthLbl = new(font);
            ManaLbl = new(font);
            AttackToAddLabel = new(font);
            DefenceToAddLabel = new(font);
            HealthToAddLabel = new(font);
            ManaToAddLabel = new(font);

            //Initialize the buttons
            ApplyBtn = new(buttonTexture, font)
            {
                Text = "Apply",
                Width = 105,
                Height = 34,
            };
            CancelBtn = new(buttonTexture, font)
            { 
                Text = "Cancel",
                Width = 105,
                Height = 34,
            };
            IncreaseAttackBtn = new(buttonTexture, font)
            {
                Text = "+1",
            };
            IncreaseAttackBtn.Height = 24;
            IncreaseAttackBtn.Width = 42;

            IncreaseDefenceBtn = new(buttonTexture, font)
            {
                Text = "+1",
            };
            IncreaseDefenceBtn.Height = 24;
            IncreaseDefenceBtn.Width = 42;

            IncreaseHealthBtn = new(buttonTexture, font)
            {
                Text = "+1",
            };
            IncreaseHealthBtn.Height = 24;
            IncreaseHealthBtn.Width = 42;

            IncreaseManaBtn = new(buttonTexture, font)
            {
                Text = "+1",
            };
            IncreaseManaBtn.Height = 24;
            IncreaseManaBtn.Width = 42;

            Action[] increaseFiveActions =
            [
                () => AllocatePoints(ref attackToAdd, 5),
                () => AllocatePoints(ref defenceToAdd, 5),
                () => AllocatePoints(ref healthToAdd, 5),
                () => AllocatePoints(ref manaToAdd, 5)
            ];
            foreach (Action allocate in increaseFiveActions)
            {
                var button = new Button(buttonTexture, font) { Text = "+5", Width = 42, Height = 24 };
                button.Click += (sender, e) => allocate();
                _increaseFiveButtons.Add(button);
                ControlManager.Add(button);
            }

            //Add the event handlers
            ApplyBtn.Click += ApplyBtn_Click;
            CancelBtn.Click += CancelBtn_Click;
            IncreaseAttackBtn.Click += IncreaseAttackBtn_Click;
            IncreaseDefenceBtn.Click += IncreaseDefenceBtn_Click;
            IncreaseHealthBtn.Click += IncreaseHealthBtn_Click;
            IncreaseManaBtn.Click += IncreaseManaBtn_Click;

            //Add the labels to the control manager
            ControlManager.Add(LevelLbl);
            ControlManager.Add(AttributePointsLbl);
            ControlManager.Add(BaseAttackLbl);
            ControlManager.Add(BaseDefenceLbl);
            ControlManager.Add(BaseHealthLbl);
            ControlManager.Add(BaseManaLbl);
            ControlManager.Add(AttackLbl);
            ControlManager.Add(DefenceLbl);
            ControlManager.Add(HealthLbl);
            ControlManager.Add(ManaLbl);
            ControlManager.Add(AttackToAddLabel);
            ControlManager.Add(DefenceToAddLabel);
            ControlManager.Add(HealthToAddLabel);
            ControlManager.Add(ManaToAddLabel);

            //Add the buttons to the control manager
            ControlManager.Add(ApplyBtn);
            ControlManager.Add(CancelBtn);
            ControlManager.Add(IncreaseAttackBtn);
            ControlManager.Add(IncreaseDefenceBtn);
            ControlManager.Add(IncreaseHealthBtn);
            ControlManager.Add(IncreaseManaBtn);
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            ResetAttributePoints();
            currentAttributePoints = Player.AttributePoints;
            CheckEnoughAttributePoints();
            UpdateLabelText(Player);
            CheckApplyAndCancelShouldBeEnabled();
        }

        private void ApplyBtn_Click(object sender, EventArgs e)
        {
            Player.ApplyAttributePoints(attackToAdd, defenceToAdd, healthToAdd, manaToAdd);
            currentAttributePoints = Player.AttributePoints;
            UpdateLabelText(Player); 
            ResetAttributePoints();
            CheckApplyAndCancelShouldBeEnabled();
        }

        private void IncreaseManaBtn_Click(object sender, EventArgs e) => AllocatePoints(ref manaToAdd, 1);

        private void IncreaseHealthBtn_Click(object sender, EventArgs e) => AllocatePoints(ref healthToAdd, 1);

        private void IncreaseDefenceBtn_Click(object sender, EventArgs e) => AllocatePoints(ref defenceToAdd, 1);

        private void IncreaseAttackBtn_Click(object sender, EventArgs e) => AllocatePoints(ref attackToAdd, 1);

        private void AllocatePoints(ref int pendingPoints, int amount)
        {
            if (currentAttributePoints < amount)
                return;

            pendingPoints += amount;
            currentAttributePoints -= amount;
            SetToAddLabelsText();
            CheckEnoughAttributePoints();
            CheckApplyAndCancelShouldBeEnabled();
        }
        public void CheckApplyAndCancelShouldBeEnabled()
        {
            if ((defenceToAdd + attackToAdd + healthToAdd + manaToAdd) > 0)
            {
                ApplyBtn.Enabled = true;
                CancelBtn.Enabled = true;
            }
            else
            {
                ApplyBtn.Enabled = false;
                CancelBtn.Enabled = false;
            }
        }
    }
}
