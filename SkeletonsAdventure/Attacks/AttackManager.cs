using CppNet;
using SkeletonsAdventure.Entities;

namespace SkeletonsAdventure.Attacks
{
    public class AttackManager(Entity entity)
    {
        private readonly List<BasicAttack> _queuedAttack = [];

        public Entity SourceEntity { get; private set; } = entity;
        public List<BasicAttack> Attacks { get; private set; } = [];
        public TimeSpan LastAttackTime { get; private set; } = new(0, 0, 0, 0);
        public BasicAttack LastAttack { get; private set; } = null;

        /// <summary>
        /// Checks if the entity is on attack cooldown.
        /// </summary>
        /// <param name="gameTime">The current game time.</param>
        /// <returns>True if the entity is on attack cooldown, false otherwise.</returns>
        public bool AttackingIsOnCoolDown(GameTime gameTime) => (gameTime.TotalGameTime - LastAttackTime).TotalMilliseconds < SourceEntity.AttackCoolDownLength;

        public void Draw(SpriteBatch spriteBatch)
        {
            if(SourceEntity.IsDead)
                return;

            foreach (var attack in Attacks)
                attack.Draw(spriteBatch);
        }

        public void Update(GameTime gameTime)
        {
            //add any attacks queued by a multishot attack
            _queuedAttack.Clear();

            bool canMove = true;

            for (int i = Attacks.Count - 1; i >= 0; i--)
            {
                var attack = Attacks[i];

                attack.Update(gameTime);
                canMove &= GetSourceCanMove(attack); //if any attack prevents movement, the source cannot move

                if (attack is MultiShotAttack multiShotAttack)
                {
                    foreach (var atk in multiShotAttack.Shots)
                        _queuedAttack.Add(atk);

                    multiShotAttack.Shots.Clear();
                }
            }

            foreach (var attack in _queuedAttack)
                AddAttack(attack, gameTime);

            SourceEntity.CanMove = canMove;
            SourceEntity.CanAttack = canMove;

            //remove any attacks that have expired. Clear the attacks after the attack.Update()
            ClearExpiredAttacks();
        }

        private static bool GetSourceCanMove(BasicAttack attack)
        {
            //Prevent the source from moving during an attack with a build up
            if (attack.AttackPassedDelay())
                return true;
            //Prevent the source from moving during an attack that locks the source in place
            else if (attack.AttackTimedOut())
                return true;
            else
                return false;
        }

        public void AddAttack(BasicAttack atk, GameTime gameTime)
        {
            if (atk is ShootingAttack shootingAtk)
                shootingAtk.PathPoints.Clear();

            atk.InitialMotion = atk.Motion;

            Attacks.Add(atk);

            if(atk is not MultiShotAttack)
            {
                LastAttackTime = gameTime.TotalGameTime;
                LastAttack = atk;
            }
        }

        public void ClearAttacks()
        {
            Attacks.Clear();
            LastAttack = null;
        }

        public void ClearExpiredAttacks()
        {
            for (int i = Attacks.Count - 1; i >= 0; i--)
            {
                var attack = Attacks[i];

                if (attack.AttackTimedOut() || attack.Source.IsDead)
                {
                    attack.AttackVisible = false;
                    attack.Position = Vector2.Zero;

                    //remove by index
                    Attacks.RemoveAt(i);
                }
            }
        }

        public void CheckAttackHit(List<Entity> entities, GameTime gameTime)
        {
            //check to see if the attack hit an entity
            foreach (var attack in Attacks)
            {
                //if the attack isn't visible it can't hit anything so skip it
                if (attack.AttackVisible is false)
                    continue;

                foreach (var entity in entities)
                {
                    if (ValidTarget(attack, entity) is false)
                        continue;

                    if(IsWithinRange(attack, entity) is false)
                        continue;

                    if (attack.Hits(entity))
                        entity.GetHitByAttack(attack, gameTime);
                }
            }
        }

        private static bool IsWithinRange(BasicAttack attack, Entity target, float threshold = 10000f)
        {
            //only do the math if they are reasonably close.
            //10,000 is 100 pixels squared.
            return Vector2.DistanceSquared(attack.Position, target.Position) < threshold;
        }

        private bool ValidTarget(BasicAttack attack, Entity target)
        {
            if (target.IsDead)
                return false;//prevents attacking dead enemies
            if (SourceEntity == target) //makes sure the entity cannot attack itself
                return false;
            if (target is Enemy && SourceEntity is Enemy) //This line prevents enemies from attacking other enemies
                return false;
            if (target.AttacksHitBy.Contains(attack) is true) //prevents an attack from hitting an opponent multiple times
                return false;

            return true;
        }
    }
}