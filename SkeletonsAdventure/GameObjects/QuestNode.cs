using MonoGame.Extended.Tilemaps;
using RpgLibrary.GameObjectClasses;
using SkeletonsAdventure.Entities.PlayerClasses;
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

        public QuestNode(TilemapObject obj, SpriteFont font) : base(obj, font)
        {

        }

        public QuestNode(QuestNode node) : base(node)
        {
            Quests = node.Quests.Select(quest => quest.Clone()).ToList();
        }

        public QuestNode(QuestNodeData data, SpriteFont font) : base(data, font)
        {

        }

        public override void Update(GameTime gameTime, Player player)
        {
            if (!Visible || !Active || player.IsDead || player.Health <= 0)
            {
                Info.Visible = false;
                return;
            }
            if (!CheckPlayerNear(player))
                return;
            if (LevelRequirements is not null && !LevelRequirements.CheckRequirements(player))
                Info.Text = LevelRequirements.MissingRequirementsText(player);
            else if (KillRequirements is not null && !KillRequirements.CheckRequirements(player))
                Info.Text = KillRequirements.MissingRequirementsText(player);
            else if (HasInteractionTask(player) || CheckAvailableQuestsToStart(player) > 0)
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

                if (!HasInteractionTask(player) && CheckAvailableQuestsToStart(player) == 0)
                    Info.Text = "No Available Quests to Start";

                if (!HasInteractionTask(player) && CheckAllCompleted(player))
                {
                    Info.Text = "All Quests Completed";
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
            if (!CanInteract(gameTime, player))
                return;

            bool interacted = ProgressInteractionTasks(player);
            foreach (var quest in Quests)
                interacted |= player.TryStartQuest(quest);
            if (interacted)
                LastInteractedTime = gameTime.TotalGameTime;
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
            return Quests.Count > 0 && Quests.All(quest => quest.CheckCompletedQuest(player));
        }

        public int CheckAvailableQuestsToStart(Player player)
        {
            return Quests.Count(quest => quest.CanStart(player));
        }
    }
}
