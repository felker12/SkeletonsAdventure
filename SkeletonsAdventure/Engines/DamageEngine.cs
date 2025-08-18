using SkeletonsAdventure.Entities;

namespace SkeletonsAdventure.Engines
{
    internal class DamageEngine() //TODO WIP
    {
        private readonly static Random rnd = new();
        public static int CalculateDamage(Entity attacker, Entity target)
        {
            int dmg, num = 0;
            //add the 1 because the random.Next() will only provide a number less than damage
            dmg = (attacker.Attack + attacker.weaponAttack) - (target.Defence + target.armourDefence) + 1; 

            if (dmg > 0)
            {
                num = rnd.Next(dmg);
                if (num == 0)
                {
                    num = rnd.Next(dmg);
                }
            }

            return num;
        }
    }
}
