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
            Info.Text = "Press R to Interact";
            if (!base.CheckPlayerNear(player))
                return false;

            if (!HasInteractionTask(player) && CheckAvailableQuestsToStart(player) == 0)
                Info.Text = CheckAllCompleted(player) ? "All Quests Completed" : GetLockedQuestText(player);

            return true;
        }

        private string GetLockedQuestText(Player player)
        {
            List<string> details = [];
            foreach (var quest in Quests.Where(quest => quest.CanOffer(player)))
            {
                details.Add(quest.Name);
                if (quest.Requirements is { } requirements && !requirements.CheckRequirements(player))
                    details.Add(requirements.MissingRequirementsText(player));

                string missingQuests = string.Join(", ", quest.RequiredQuestNames
                    .Where(name => player.GetCompletedQuestByName(name) is null));
                if (missingQuests.Length > 0)
                    details.Add($"Complete {missingQuests} first");
            }

            return details.Count > 0 ? string.Join("\n", details) : "No Available Quests to Start";
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
