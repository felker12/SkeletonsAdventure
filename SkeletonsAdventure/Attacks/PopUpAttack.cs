using RpgLibrary.AttackData;
using SkeletonsAdventure.Entities;

namespace SkeletonsAdventure.Attacks
{
    public class PopUpAttack : BasicAttack
    {
        public PopUpAttack(AttackData attackData, Texture2D texture, SpriteFont infoFont, Entity source = null) : base(attackData, texture, infoFont, source)
        {
        }

        protected PopUpAttack(BasicAttack attack) : base(attack)
        {
        }

        public virtual void ResetAttack()
        {
            //is handled in classes that inherit from this
        }
    }
}
