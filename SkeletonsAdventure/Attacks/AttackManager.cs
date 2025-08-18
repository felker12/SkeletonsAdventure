using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkeletonsAdventure.Entities;

namespace SkeletonsAdventure.Attacks
{
    internal class AttackManager(Entity entity)
    {
        public Entity SourceEntity { get; set; } = entity;
        public List<BasicAttack> Attacks { get; private set; } = [];
        public TimeSpan LastAttackTime { get; set; } = new(0, 0, 0, 0);

        private bool _attacked = false;

        public List<BasicAttack> ToRemove = [];

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var attack in Attacks)
                attack.Draw(spriteBatch);
        }

        public void Update(GameTime gameTime)
        {
            ToRemove = [];
            foreach (var attack in Attacks)
            {
                attack.Update(gameTime);
                if (attack.AttackTimedOut())
                {
                    attack.AttackVisible = false;
                    ToRemove.Add(attack);
                }
            }

            foreach (var atk in ToRemove)
            {
                RemoveAttack(atk);
            }

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

        public static void ClearOldAttacks(List<Entity> entities)
        {
            List<BasicAttack> oldAttacks = [];

            foreach (var entity in entities)
            {
                foreach (var atk in entity.AttacksHitBy)
                {
                    if (atk.AttackTimedOut())
                        oldAttacks.Add(atk);
                }

                if(oldAttacks.Count > 0)
                {
                    foreach (var oldAttack in oldAttacks)
                    {
                        entity.AttacksHitBy.Remove(oldAttack);
                    }
                    oldAttacks.Clear();
                }
            }
        }
    }
}
