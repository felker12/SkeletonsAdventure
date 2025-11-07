using RpgLibrary.AttackData;
using SkeletonsAdventure.Entities;

namespace SkeletonsAdventure.Attacks
{
    internal class PopUpAttack : BasicAttack
    {
        public PopUpAttack(AttackData attackData, Texture2D texture, Entity source = null) : base(attackData, texture, source)
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
