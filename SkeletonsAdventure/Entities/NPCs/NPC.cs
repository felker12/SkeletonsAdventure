using RpgLibrary.EntityClasses;
using SkeletonsAdventure.HelperClasses;
using SkeletonsAdventure.Quests;
using System.Linq;

namespace SkeletonsAdventure.Entities.NPCs
{
    internal class NPC : AnimatedSprite, ICloneableGameClass<NPC>
    {
        List<Quest> Quests { get; set; } = [];

        public NPC() : base()
        {
        }

        public NPC(NPC npc) : base()
        {
            Quests = [.. npc.Quests.Select(q => new Quest(q))]; //Create a deep copy of the quests
        }

        public NPC(NPCData data) : base()
        {
            foreach (var questData in data.QuestDatas)
                Quests.Add(new Quest(questData));
        }

        public override NPC Clone() 
        { 
            return new NPC(this);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
