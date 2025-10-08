using MonoGame.Extended.Tiled;
using RpgLibrary.GameObjectClasses;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.Quests;
using System.Linq;

namespace SkeletonsAdventure.GameObjects
{
    internal class QuestNode : InteractableObject
    {
        public List<Quest> Quests { get; set; } = [];

        public Quest LowestRequirementQuest
        {
            get
            {
                if (Quests.Count == 0)
                    return null;
                return Quests.OrderBy(q => q.Requirements.GetTotalRequirements()).FirstOrDefault();
            }
        }

        public Quest DisplayedQuest
        {
            get
            {
                if (Quests.Count == 0)
                    return null;
                return Quests.FirstOrDefault(q => !q.IsCompleted && q.Active);
            }
        }

        public QuestNode(TiledMapObject obj) : base(obj)
        {

        }

        public QuestNode(QuestNode node) : base(node)
        {

        }

        public QuestNode(QuestNodeData data) : base(data)
        {

        }

        public override void Update(GameTime gameTime, Player player)
        {
            if (CheckPlayerNear(player) && Active && CheckAvailableQuestsToStart(player) > 0)
                HandleInput(gameTime, player);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override QuestNode Clone()
        {
            return new(this);
        }

        public override bool CheckPlayerNear(Player player)
        {
            if (Rectangle.Intersects(player.Rectangle))
            {
                Info.Visible = true;
                Info.Text = $"Press R to Interact" +
                    $"\nTotal Quests: {Quests.Count}" +
                    $"\nActive Quests: {GetTotalActiveQuests(player)}" +
                    $"\nCompleted Quests: {GetTotalCompletedQuests(player)}" +
                    $"\nAll Quests Completed: {CheckAllCompleted(player)}";

                if (CheckAvailableQuestsToStart(player) == 0)
                    Info.Text = "No Available Quests to Start";

                if (CheckAllCompleted(player))
                {
                    Info.Text = "All Quests Completed";
                    Active = false;
                }
            }
            else
            {
                Info.Visible = false;
                Info.Text = "Press R to Interact";
            }

            return Info.Visible;
        }

        public override void Interact(GameTime gameTime, Player player)
        {
            foreach (var quest in Quests)
            {
                if (quest.PlayerRequirementsMet(player) is false)
                    continue; // Skip to the nexrt quest if the requirements aren't met

                if (quest.CheckCompletedQuest(player))
                    continue; // Skip to the next quest if this one is already completed

                if (quest.CheckActiveQuest(player))
                    continue; // Skip to the next quest if this one is already active

                //add the quest to the player's active quests if all the requirements are met
                Quest q = quest.Clone();
                q.StartQuest();
                player.AddActiveQuest(q);
            }
        }

        public int GetTotalActiveQuests(Player player)
        {
            int totalActiveQuests = 0;

            foreach (var quest in Quests)
                foreach(var playersQuest in player.ActiveQuests)
                    if (quest.Name == playersQuest.Name)
                        totalActiveQuests++;

            return totalActiveQuests;
        }

        public int GetTotalCompletedQuests(Player player)
        {
            int totalActiveQuests = 0;

            foreach (var quest in Quests)
                foreach (var playersQuest in player.CompletedQuests)
                    if (quest.Name == playersQuest.Name)
                        totalActiveQuests++;

            return totalActiveQuests;
        }

        public bool CheckAllCompleted(Player player)
        {
            return Quests.Count == GetTotalCompletedQuests(player);
        }

        public int CheckAvailableQuestsToStart(Player player)
        {
            return Quests.Count - GetTotalCompletedQuests(player) - GetTotalActiveQuests(player);
        }
    }
}
