using SkeletonsAdventure.Entities;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.Controls;
using SkeletonsAdventure.Quests;
using SkeletonsAdventure.GameUI;

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

        public QuestMenu() : base()
        {
            Initialize();
        }

        public QuestMenu(int width, int height) : base(width, height)
        {
            Initialize();
        }

        private void Initialize()
        {
            CreateControls();
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

        private void CreateControls()
        {
            //Create the labels
            QuestsLbl = new(GameManager.Arial16)
            {
                Text = "Quests",
            };

            //Create the Toggles for the quests
            CompletedQuestsToggle = new()
            {
                Text = "Hide Completed",
                ToggledText = "Show completed",
            };
            CompletedQuestsToggle.Height = (int)CompletedQuestsToggle.SpriteFont.MeasureString(CompletedQuestsToggle.Text).Y + 4;
            CompletedQuestsToggle.Width = (int)CompletedQuestsToggle.SpriteFont.MeasureString(CompletedQuestsToggle.Text).X + 10;

            StartedQuestsToggle = new()
            {
                Text = "Hide not Started",
                ToggledText = "Show not Started",
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

            QuestSelectionControlBox = new()
            {
                Visible = true,
                Texture = GameManager.TextBoxTexture,
                DrawOutline = true,
            };

            QuestSelectionControlBox.ActiveSelectionChanged += QuestSelectionControlBox_ActiveSelectionChanged;

            SelectedQuestTextBox = new()
            {
                Visible = true,
                Background = GameManager.TextBoxTexture,
            };
            ControlManager.Add(SelectedQuestTextBox);
        }


        private void SetPositions()
        {
            int labelSpace = 10;

            QuestsLbl.Position = new(Position.X + 15, Position.Y + 15);

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

                QuestSelectionControlBox.AddSelectionControl(new(quest.Name){TextColor = Color.Black});
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
