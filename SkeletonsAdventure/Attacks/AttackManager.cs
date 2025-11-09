using SkeletonsAdventure.Entities;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace SkeletonsAdventure.Attacks
{
    internal class AttackManager(Entity entity)
    {
        public Entity SourceEntity { get; set; } = entity;
        public List<BasicAttack> Attacks { get; private set; } = [];
        public TimeSpan LastAttackTime { get; set; } = new(0, 0, 0, 0);
        public BasicAttack LastAttack { get; set; } = null;

        private bool _attacked = false;

        public void Draw(SpriteBatch spriteBatch)
        {
            if(SourceEntity.IsDead)
                return;

            foreach (var attack in Attacks)
                attack.Draw(spriteBatch);
        }

        public void Update(GameTime gameTime)
        {
            if (_attacked)
            {
                LastAttackTime = gameTime.TotalGameTime;
                _attacked = false;
            }

            List<Entity> SourcesThatCantMove = [];

            foreach (var attack in Attacks)
            {
                attack.Update(gameTime);

                //if any attack is causing the source to not be able to move make sure a later attack isn't overriding that
                if(SourcesThatCantMove.Contains(attack.Source) is false && SourceEntity.CanMove is false)
                    SourcesThatCantMove.Add(attack.Source);
            }

            //make sure any source that can't move because of any attack can't move and isn't being allowed to move by another attack overriding it
            foreach (var entity in SourcesThatCantMove)
                entity.CanMove = false;

            //Add any attacks queued by a multishot attack
            List<BasicAttack> toRemove = [];
            foreach(var attack in Attacks)
            {
                if(attack is MultiShotAttack multiShotAttack)
                {
                    foreach(var atk in multiShotAttack.Shots)
                        toRemove.Add(atk); 

                    multiShotAttack.Shots.Clear();
                }
            }

            foreach(var attack in toRemove)
                AddAttack(attack, gameTime);
        }

        public void AddAttack(BasicAttack atk, GameTime gameTime)
        {
            if (atk is ShootingAttack shootingAtk)
                shootingAtk.PathPoints = [];

            atk.InitialMotion = atk.Motion;

            Attacks.Add(atk);
            LastAttackTime = gameTime.TotalGameTime;
            LastAttack = atk;
            _attacked = true;
        }

        public void RemoveAttack(BasicAttack atk)
        {
            atk.Position = Vector2.Zero;
            atk.AttackVisible = true;
            Attacks.Remove(atk);
        }

        public void ClearAttacks()
        {
            Attacks.Clear();
            LastAttack = null;
        }

        public void ClearExpiredAttacks()
        {
            List<BasicAttack> toRemove = [];

            foreach (var attack in Attacks)
            {
                if (attack.AttackTimedOut() || attack.Source.IsDead)
                {
                    attack.AttackVisible = false;
                    toRemove.Add(attack);
                }
            }

            foreach (var atk in toRemove)
                RemoveAttack(atk);
        }

        public void CheckAttackHit(List<Entity> entities, GameTime gameTime)
        {
            //check to see if the attack hit an entity
            foreach (var attack in Attacks)
            {
                if(attack.AttackVisible)
                {
                    foreach (var entity in entities)
                    {
                        if (entity.IsDead)
                            continue;//prevents attacking dead enemies
                        if (entity.AttacksHitBy.Contains(attack) is true) //prevents an attack from hitting an opponent multiple times
                            continue;
                        if (SourceEntity == entity) //makes sure the entity cannot attack itself
                            continue;
                        if (entity is Enemy && SourceEntity is Enemy) //This line prevents enemies from attacking other enemies
                            continue;
                        if (entity.Rectangle.Intersects(attack.DamageHitBox))
                            entity.GetHitByAttack(attack, gameTime);
                    }
                }
            }
        }
    }
}
