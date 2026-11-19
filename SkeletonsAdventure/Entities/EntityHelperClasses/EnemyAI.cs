using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Entities.PlayerClasses;

namespace SkeletonsAdventure.Entities.EntityHelperClasses
{
    internal static class EnemyAI
    {
        public static void CheckIfEnemyDetectPlayer(GameTime time, List<Entity> entities, Player player, EnemyNavigation navigation = null)
        {
            foreach (var entity in entities)
            {
                if (entity is not Enemy enemy)
                    continue;
                if (enemy.IsDead || enemy.Health <= 0 || player is null || player.IsDead || player.Health <= 0)
                {
                    enemy.Motion = Vector2.Zero;
                    continue;
                }
                bool seesPlayer = enemy.DetectionArea.Intersects(player.Rectangle) &&
                    (navigation?.HasLineOfSight(enemy.Center, player.Center) ?? true);
                if (seesPlayer)
                {
                    enemy.LastKnownPlayerPosition = player.Position;
                    enemy.LastSawPlayerAt = time.TotalGameTime;
                    enemy.Behavior = EnemyBehavior.Pursuing;
                    FaceTarget(enemy, player.Center);
                    enemy.BasicAttack.Offset();
                    var attackPosition = enemy.Position + enemy.BasicAttack.AttackOffset;
                    var reach = new Rectangle((int)attackPosition.X, (int)attackPosition.Y,
                        enemy.BasicAttack.Width, enemy.BasicAttack.Height);
                    bool inReach = enemy.Rectangle.Intersects(player.Rectangle) || reach.Intersects(player.Rectangle);
                    enemy.Motion = inReach || !enemy.CanMove ? Vector2.Zero : MoveTo(player.Position);
                    if (inReach)
                        enemy.AutoAttack(player, time);
                    continue;
                }
                bool recentHit = enemy.LastTimeAttacked != TimeSpan.Zero &&
                    time.TotalGameTime - enemy.LastTimeAttacked <= TimeSpan.FromSeconds(6);
                Vector2? investigation = recentHit && !enemy.CheckedLastAtackArea
                    ? enemy.PositionLastAttackedFrom - new Vector2(enemy.Width, enemy.Height) / 2
                    : enemy.LastKnownPlayerPosition.HasValue && time.TotalGameTime - enemy.LastSawPlayerAt <= TimeSpan.FromSeconds(6)
                        ? enemy.LastKnownPlayerPosition : null;
                if (investigation is Vector2 target)
                {
                    enemy.Behavior = EnemyBehavior.Investigating;
                    if (Vector2.DistanceSquared(enemy.Position, target) <= 1)
                    {
                        enemy.CheckedLastAtackArea = true;
                        enemy.LastKnownPlayerPosition = null;
                        Patrol();
                    }
                    else
                    {
                        FaceTarget(enemy, target + new Vector2(enemy.Width, enemy.Height) / 2);
                        enemy.Motion = enemy.CanMove ? MoveTo(target) : Vector2.Zero;
                    }
                }
                else
                {
                    enemy.LastKnownPlayerPosition = null;
                    Patrol();
                }

                Vector2 MoveTo(Vector2 target) => navigation?.DirectionTo(enemy, target, time)
                    ?? EnemyNavigation.Direction(enemy, target, time);
                void Patrol()
                {
                    enemy.Behavior = EnemyBehavior.Patrolling;
                    if (enemy.CanMove)
                        enemy.IdleBehavior(time);
                    else
                        enemy.Motion = Vector2.Zero;
                }
            }
        }

        private static void FaceTarget(Enemy source, Vector2 target)
        {
            var delta = target - source.Center;
            if (delta == Vector2.Zero)
                return;
            source.CurrentAnimation = Math.Abs(delta.X) >= Math.Abs(delta.Y)
                ? delta.X > 0 ? AnimationKey.Right : AnimationKey.Left
                : delta.Y > 0 ? AnimationKey.Down : AnimationKey.Up;
        }
    }
}