using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameUI;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.Engines
{
    public static class DamageEngine //TODO WIP
    {
        private readonly static Random rnd = new();

        public static int CalculateDamage(Entity attacker, Entity target)
        {
           return CalculateDamage(attacker.Attack, attacker.WeaponAttack, target.Defence, target.ArmourDefence);
        }

        public static int CalculateDamage(int attack, int weaponAttack, int defence, int armourDefence)
        {
            int dif, dmg = 0;
            //add the 1 because the random.Next() will only provide a number less than damage
            dif = (attack + weaponAttack) - (defence + armourDefence) + 1;
            if (dif > 1)
            {
                dmg = rnd.Next(dif);
                if (dmg == 0)
                    dmg = rnd.Next(dif); //reroll once if damage is 0
                if (dmg == 0)
                    dmg = 1; //minimum damage is 1
            }
            return dmg;

        }

        public static void GetHitByAttack(Entity target, BasicAttack attack, GameTime gameTime)
        {
            target.AttacksHitBy.Add(attack);
            target.LastTimeAttacked = gameTime.TotalGameTime;
            target.PositionLastAttackedFrom = attack.Source.Center;

            int dmg = (int)(CalculateDamage(attack.Source, target) * attack.DamageModifier);
            DamagePopUp damagePopUp = new(dmg.ToString(), target.Center);

            if (target is Enemy)
                damagePopUp.Color = Color.Cyan;

            World.CurrentLevel.DamagePopUpManager.Add(damagePopUp);
            //TODO add logic for critical hits and color the attack orange if it is a critical

            target.Health -= dmg;

            if (target.Health < 1)
            {
                target.EntityDiedByAttack(attack);

                if(attack.Source is Player player)
                {
                    player.GainXp(target.XP);
                    player.KillCounter.RecordKill(target.GetType().Name);
                }
            }
        }
    }
}
