using SkeletonsAdventure.Entities;

namespace SkeletonsAdventure.Attacks
{
    public class AttackManager(Entity entity)
    {
        private bool _attacked = false;
        List<BasicAttack> _queuedAttack = []; 
        List<Entity> _sourcesThatCantMove = [];

        public Entity SourceEntity { get; set; } = entity;
        public List<BasicAttack> Attacks { get; private set; } = [];
        public TimeSpan LastAttackTime { get; set; } = new(0, 0, 0, 0);
        public BasicAttack LastAttack { get; set; } = null;

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
            if (_attacked)
            {
                _attacked = false;
            }

            _sourcesThatCantMove = [];

            foreach (var attack in Attacks)
            {
                attack.Update(gameTime);

                //if any attack is causing the source to not be able to move make sure a later attack isn't overriding that
                if(_sourcesThatCantMove.Contains(attack.Source) is false && SourceEntity.CanMove is false)
                    _sourcesThatCantMove.Add(attack.Source);
            }

            //make sure any source that can't move because of any attack can't move and isn't being allowed to move by another attack overriding it
            foreach (var entity in _sourcesThatCantMove)
                entity.CanMove = false;

            //add any attacks queued by a multishot attack
            _queuedAttack = [];
            foreach(var attack in Attacks)
            {
                if(attack is MultiShotAttack multiShotAttack)
                {
                    foreach(var atk in multiShotAttack.Shots)
                        _queuedAttack.Add(atk); 

                    multiShotAttack.Shots.Clear();
                }
            }

            foreach(var attack in _queuedAttack)
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
                        if(attack.Hits(entity))
                            entity.GetHitByAttack(attack, gameTime);
                    }
                }
            }
        }
    }
}
