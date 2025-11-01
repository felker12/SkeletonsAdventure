using RpgLibrary.AttackData;
using SkeletonsAdventure.Entities;

namespace SkeletonsAdventure.Attacks
{
    internal class PopUpAttack : BasicAttack
    {
        public PopUpAttack(BasicAttack attack) : base(attack)
        {
        }

        public PopUpAttack(AttackData attackData, Texture2D texture, Entity source) : base(attackData, texture, source)
        {
        }

        public virtual void ResetAttack()
        {
            //is handled in classes that inherit from this
        }
    }
}
