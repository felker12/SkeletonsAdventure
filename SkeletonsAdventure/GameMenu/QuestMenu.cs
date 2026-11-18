using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.Controls;
using SkeletonsAdventure.Quests;
using SkeletonsAdventure.GameUI;
using SkeletonsAdventure.Entities.PlayerClasses;

namespace SkeletonsAdventure.GameMenu
{
    internal class QuestMenu : BaseMenu
    {
        //TODO this menu is used to display the player's quests
        public Player Player { get; set; } = World.Player;

        private List<Quest> Quests = [];
        private Label QuestsLbl;
        private ToggleButton CompletedQuestsToggle, StartedQuestsToggle;
        private SelectionControlBox QuestSelectionControlBox;
        private TextBox SelectedQuestTextBox;
        private readonly SpriteFont _font;

        public QuestMenu(Texture2D texture, Texture2D buttonTexture, Texture2D textBoxTexture,
            SpriteFont font, SpriteFont headingFont, Color color)
            : base(texture, color)
        {
            _font = font ?? throw new ArgumentNullException(nameof(font));
            CreateControls(buttonTexture, textBoxTexture, headingFont);
            SetPositions();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime); 
            QuestSelectionControlBox.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            spriteBatch.Begin();
            QuestSelectionControlBox.Draw(spriteBatch);
            spriteBatch.End();
        }

        public override void HandleInput(PlayerIndex playerIndex)
        {
            QuestSelectionControlBox.HandleInput(playerIndex);
        }

        public void UpdateWithPlayer(Player player)
        {
            Player = player;
        }

        public override void MenuOpened()
        {
            UpdateWithPlayer(World.Player); 
            SetPositions(); 

            if(QuestSelectionControlBox.ControlsCount == 0)
            {
                QuestSelectionControlBox.Clear(); //clear the ControlBox to be safe
                LoadQuests();
            }
            else
                QuestSelectionControlBox_ActiveSelectionChanged(this, null); //this updates the text box with any changes that may have happened to the active quest
        }

        private void CreateControls(Texture2D buttonTexture, Texture2D textBoxTexture, SpriteFont headingFont)
        {

            //Create the labels
            QuestsLbl = new(headingFont)
            {
                Text = "Quest log",
                TextColor = MenuStyle.Ivory,
            };

            //Create the Toggles for the quests
            CompletedQuestsToggle = new(buttonTexture, _font)
            {
                Text = "Hide Completed",
                ToggledText = "Show Completed",
                ToggledColor = MenuStyle.Selection,
            };
            CompletedQuestsToggle.Height = 34;
            CompletedQuestsToggle.Width = 175;

            StartedQuestsToggle = new(buttonTexture, _font)
            {
                Text = "Hide not Started",
                ToggledText = "Show not Started",
                ToggledColor = MenuStyle.Selection,
                Height = CompletedQuestsToggle.Height,
                Width = CompletedQuestsToggle.Width
            };

            //add the event handlers for the toggles
            CompletedQuestsToggle.Click += CompletedQuestsToggle_Click;
            StartedQuestsToggle.Click += StartedQuestsToggle_Click;

            //Add the controls to the control manager
            ControlManager.Add(QuestsLbl);

            ControlManager.Add(CompletedQuestsToggle);
            ControlManager.Add(StartedQuestsToggle);

            QuestSelectionControlBox = new(textBoxTexture)
            {
                Visible = true,
                DrawOutline = true,
                Color = MenuStyle.Surface,
                OutlineColor = MenuStyle.Border,
            };

            QuestSelectionControlBox.ActiveSelectionChanged += QuestSelectionControlBox_ActiveSelectionChanged;

            SelectedQuestTextBox = new(textBoxTexture, _font)
            {
                Visible = true,
                BackgroundColor = MenuStyle.Surface,
                BackgroundOutlineColor = MenuStyle.Border,
                TextColor = MenuStyle.Ivory,
            };
            ControlManager.Add(SelectedQuestTextBox);
        }


        public override void Arrange(Rectangle bounds)
        {
            base.Arrange(bounds);
            SetPositions();
        }

        private void SetPositions()
        {
            int labelSpace = 10;

            QuestsLbl.Position = new(Position.X + 26, Position.Y + 26);

            CompletedQuestsToggle.Position = new(QuestsLbl.Position.X, QuestsLbl.Position.Y + QuestsLbl.SpriteFont.LineSpacing + labelSpace);
            StartedQuestsToggle.Position = new(CompletedQuestsToggle.Position.X, CompletedQuestsToggle.Position.Y + CompletedQuestsToggle.Height + labelSpace);

            QuestSelectionControlBox.Position = new(CompletedQuestsToggle.Position.X + CompletedQuestsToggle.Width + labelSpace, QuestsLbl.Position.Y);
            QuestSelectionControlBox.Width = (int)(Width * .6);
            QuestSelectionControlBox.Height = (int)(Height * .4);

            SelectedQuestTextBox.Position = new(QuestSelectionControlBox.Position.X, QuestSelectionControlBox.Position.Y + QuestSelectionControlBox.Height + labelSpace);
            SelectedQuestTextBox.Width = QuestSelectionControlBox.Width;
            SelectedQuestTextBox.Height = (int)(Height * .3);
        }

        private void LoadQuests()
        {
            Quests = [.. GameManager.QuestsClone.Values];
            List<string> questStrings = Quests.ConvertAll(q => $"{q.Name}");

            foreach (Quest quest in Quests)
            {
                if (CompletedQuestsToggle.Toggled)
                {
                    if (quest.CheckCompletedQuest(Player)) // Hide completed quests
                        continue;
                }
                if (StartedQuestsToggle.Toggled)
                {
                    if (quest.CheckActiveQuest(Player) is false) // Only show started quests
                        continue;
                }

                QuestSelectionControlBox.AddSelectionControl(new(_font, quest.Name) { TextColor = MenuStyle.Ivory, HoverColor = MenuStyle.Brass, HasFocusColor = MenuStyle.Brass });
            }
        }

        private Quest GetQuestByName(string name)
        {
            foreach(Quest quest in Quests)
            {
                if (quest.Name == name)
                    return quest;
            }

            return null;
        }

        private void StartedQuestsToggle_Click(object sender, EventArgs e)
        {
            QuestSelectionControlBox.Clear();
            LoadQuests();
        }

        private void CompletedQuestsToggle_Click(object sender, EventArgs e)
        {
            QuestSelectionControlBox.Clear();
            LoadQuests();
        }

        private void QuestSelectionControlBox_ActiveSelectionChanged(object sender, EventArgs e)
        {
            if (QuestSelectionControlBox.ActiveSelectionControl != null)
            {
                string questName = QuestSelectionControlBox.ActiveSelectionControl.Text;
                Quest quest;

                //set the quest = to the quest info from the player if the quest is active or completed. If not set it to the quest from the quest list
                if (Player.GetActiveQuestByName(questName) != null)
                    quest = Player.GetActiveQuestByName(questName);
                else if (Player.GetCompletedQuestByName(questName) != null)
                    quest = Player.GetCompletedQuestByName(questName);
                else
                    quest = GetQuestByName(questName);

                //Set the text box text to be the info of the quest
                if (quest != null)
                    SelectedQuestTextBox.Text = quest.QuestInfo;
            }
            else
                SelectedQuestTextBox.Text = string.Empty; //Set the text boxes text to be empty if there is no quest to display
        }
    }
}
