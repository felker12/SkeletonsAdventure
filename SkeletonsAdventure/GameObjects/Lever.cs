using MonoGame.Extended.Tiled;
using RpgLibrary.GameObjectClasses;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.GameEvents;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.GameObjects
{
    internal class Lever : InteractableObject
    {
        public string LeverPurpose { get; set; } = string.Empty;
        public string GameEventName { get; set; } = string.Empty;
        public float CooldownTime { get; set; } = 1000; //Time in milliseconds
        public TimeSpan LastInteractedTime { get; set; } = new();

        public Lever(TiledMapObject obj) : base(obj) { }

        public Lever(InteractableObject obj) : base(obj) { }

        public Lever(InteractableObjectData obj) : base(obj) { }

        public override void Interact(GameTime gameTime, Player player)
        {
            //base.Interact(player);

            //Info.Visible = false;
            //Active = !Active;
            //Visible = !Visible;

            if (LastInteractedTime + TimeSpan.FromMilliseconds(CooldownTime) < gameTime.TotalGameTime is false)
            {
                return;
            }

            TileAnimationFrame frame1 = new() { TileId = 0, Duration = 200f };
            TileAnimationFrame frame2 = new() { TileId = 30, Duration = 200f };
            TileAnimationFrame frame3 = new() { TileId = 60, Duration = 200f };
            TileAnimationFrame frame4 = new() { TileId = 90, Duration = 200f };
            TileAnimationFrame frame5 = new() { TileId = 120, Duration = 200f };

            TileAnimation animation = new()
            {
                Frames = [frame1, frame2, frame3, frame4, frame5],
            };

            TiledAnimation tiledAnimation = new("LeverPull", GameManager.DoorLeverAndChestAnimationTexture, animation, 16, 16);


            AnimatedTileEvent animatedTileEvent = new()
            {
                TiledAnimation = tiledAnimation,
                Position = Position,
                Duration = tiledAnimation.Animation.TotalDuration
            };


            if (GameEventName == "PlayLeverAnimation")
            {
                World.CurrentLevel.AddGameEvent(animatedTileEvent);

                if (LeverPurpose == "ConditionalLayerToggle")
                {
                    ToggleLayerVisibility toggle = new(World.CurrentLevel.ConditionalLayer);
                    TimeDelayedGameEvent delayedGameEvent = new(toggle, animatedTileEvent.Duration * .75f);

                    World.CurrentLevel.AddGameEvent(delayedGameEvent);
                }
            }

            if (string.IsNullOrEmpty(GameEventName))
            {
                GameEvent gameEvent = new()
                {
                    Duration = 600, //600 milliseconds = 0.6 seconds
                };

                World.CurrentLevel.AddGameEvent(gameEvent);
            }

            LastInteractedTime = gameTime.TotalGameTime;
        }
    }
}
