using RpgLibrary.EntityClasses;
using SkeletonsAdventure.HelperClasses;
using SkeletonsAdventure.Quests;
using System.Linq;

namespace SkeletonsAdventure.Entities.NPCs
{
    internal class NPC : AnimatedSprite, ICloneableGameClass<NPC>
    {
        List<Quest> Quests { get; set; } = [];

        public NPC(Texture2D texture, SpriteFont infoFont) : base(texture, infoFont)
        {
        }

        public NPC(NPC npc) : base(npc.Texture, npc.Info.SpriteFont)
        {
            Quests = [.. npc.Quests.Select(q => new Quest(q))]; //Create a deep copy of the quests
        }

        public NPC(NPCData data, Texture2D texture, SpriteFont infoFont) : base(texture, infoFont)
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
