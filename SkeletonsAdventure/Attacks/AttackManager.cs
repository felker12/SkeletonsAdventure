using CppNet;
using SkeletonsAdventure.Entities;

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
            {
                attack.Draw(spriteBatch);

                /*spriteBatch.Draw(attack.Texture, attack.Source.Position, attack.IconRectangle, attack.SpriteColor,
                    0, attack.Source.Position, attack.Scale, SpriteEffects.None, 1);*/

                attack.DrawIcon(spriteBatch, attack.Source.Position);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (_attacked)
            {
                LastAttackTime = gameTime.TotalGameTime;
                _attacked = false;
            }
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

        public void ClearExpiredAttacks(GameTime gameTime)
        {
            List<BasicAttack> toRemove = [];

            foreach (var attack in Attacks)
            {
                attack.Update(gameTime);
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
