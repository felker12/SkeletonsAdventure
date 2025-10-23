using SkeletonsAdventure.Entities;

namespace SkeletonsAdventure.Engines
{
    internal class DamageEngine() //TODO WIP
    {
        private readonly static Random rnd = new();
        public static int CalculateDamage(Entity attacker, Entity target)
        {
            int dif, dmg = 0;
            //add the 1 because the random.Next() will only provide a number less than damage
            dif = (attacker.Attack + attacker.WeaponAttack) - (target.Defence + target.ArmourDefence) + 1; 

            if (dif > 0)
            {
                dmg = rnd.Next(dif);

                if (dmg == 0)
                    dmg = rnd.Next(dif); //reroll once if damage is 0

                if (dmg == 0)
                    dmg = 1; //minimum damage is 1
            }

            return dmg;
        }
    }
}
