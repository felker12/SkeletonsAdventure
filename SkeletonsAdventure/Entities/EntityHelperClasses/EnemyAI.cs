using SkeletonsAdventure.Entities.PlayerClasses;

namespace SkeletonsAdventure.Entities.EntityHelperClasses
{
    internal static class EnemyAI
    {
        // Checks if any enemy in the list detects the player and handles their behavior accordingly
        public static void CheckIfEnemyDetectPlayer(GameTime gameTime, List<Entity> entities, Player player)
        {
            foreach (Entity entity in entities)
            {
                if (entity is not Enemy enemy || enemy.IsDead)
                    continue;

                HandleEnemyDetection(gameTime, enemy, player);
            }
        }

        // Checks if the enemy detects the player and moves towards them if so
        private static bool CheckForPlayer(Enemy enemy, Player player)
        {
            bool playerInRange = false;

            if (enemy.DetectionArea.Intersects(player.Rectangle))
                playerInRange = true;

            enemy.Motion = Vector2.Zero; //Stops any motion caused by another method

            //if the enemy detects the player then move towerds the player
            if (!enemy.Rectangle.Intersects(player.Rectangle))
                enemy.PathToPoint(player.Position);
            else
                enemy.FaceTarget(player);

            return playerInRange;
        }

        // Handles the enemy's behavior based on player detection and recent attacks
        private static void HandleEnemyDetection(GameTime gameTime, Enemy enemy, Player player)
        {
            //if the player is close then go to the player
            if (CheckForPlayer(enemy, player))
                enemy.AutoAttack(player, gameTime);

            //if the enemy has attacked by an out of sight enemy then move to the point the enemy was at  
            else if (enemy.LastTimeAttacked.TotalMilliseconds != 0
                && gameTime.TotalGameTime.TotalMilliseconds - enemy.LastTimeAttacked.TotalMilliseconds < 6000
                && enemy.CheckedLastAtackArea == false)
            {
                InvestigateLastAttackLocation(gameTime, enemy, player);
            }
            else //if no player in sight do something
                enemy.IdleBehavior(gameTime);
        }

        // Investigates the last known attack location of the enemy
        private static void InvestigateLastAttackLocation(GameTime gameTime, Enemy enemy, Player player)
        {
            //if the enemy detects player on way then mark area checked so the enemy doesn't keep moving towards that point
            if (CheckForPlayer(enemy, player))
                enemy.CheckedLastAtackArea = true;

            //move to the point the enemy was attacked from
            if (enemy.Rectangle.Intersects(new((int)enemy.PositionLastAttackedFrom.X, (int)enemy.PositionLastAttackedFrom.Y, 1, 1)))
            {
                enemy.CheckedLastAtackArea = true;
                if (CheckForPlayer(enemy, player))
                    enemy.AutoAttack(player, gameTime);
                else
                    enemy.IdleBehavior(gameTime); //investigation over
            }
            else
                enemy.PathToPoint(enemy.PositionLastAttackedFrom);
        }
    }
}
