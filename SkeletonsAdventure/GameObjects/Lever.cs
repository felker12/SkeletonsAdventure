using MonoGame.Extended.Tilemaps;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Entities.PlayerClasses;
using SkeletonsAdventure.GameEvents;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.GameObjects
{
    internal class Lever : InteractableObject
    {
        public string LeverPurpose { get; set; } = string.Empty;
        public string GameEventName { get; set; } = string.Empty;
        public Vector2 LeverPosition { get; set; } = new(); //in the event the lever event needs to be drawn at a different location than the interactable objects position

        private readonly TiledAnimation _animation;

        public Lever(TilemapObject obj, SpriteFont font, TiledAnimation animation) : base(obj, font)
        {
            _animation = animation ?? throw new ArgumentNullException(nameof(animation));
            Initialize(obj);
        }

        private void Initialize(TilemapObject obj)
        {
            LeverPosition = obj.Position;
        }

        public override void Interact(GameTime gameTime, Player player)
        {
            // Check for cooldown
            if(CooldownTime > 0 && 
                LastInteractedTime + TimeSpan.FromMilliseconds(CooldownTime) < gameTime.TotalGameTime is false)
                return;

            HandleLeverActivation();

            LastInteractedTime = gameTime.TotalGameTime;
        }

        private void HandleLeverActivation()
        {
            if (GameEventName == "PlayLeverAnimation")
            {
                AnimatedTileEvent animatedTileEvent = new(_animation.Clone(), LeverPosition);

                World.CurrentLevel.AddGameEvent(animatedTileEvent);

                if (LeverPurpose == "ConditionalLayerToggle")
                    ToggleConditionalLayer(animatedTileEvent);
            }
        }

        private static void ToggleConditionalLayer(AnimatedTileEvent animatedTileEvent)
        {
            ToggleLayerVisibility toggle = new(World.CurrentLevel.ConditionalLayer);
            TimeDelayedGameEvent delayedGameEvent = new(toggle, animatedTileEvent.Duration * .75f);

            //delay hiding the static lever until the animated lever is mostly done
            World.AddGameEventToCurrentLevel(delayedGameEvent);

            //hide the static lever, draw the animated 1, then show the static lever again
            ToggleLayerVisibility toggle2 = new(World.CurrentLevel.InteractableObjectLayerTiles);
            World.AddGameEventToCurrentLevel(toggle2);
            TimeDelayedGameEvent delayedGameEvent2 = new(new ToggleLayerVisibility(World.CurrentLevel.InteractableObjectLayerTiles), animatedTileEvent.Duration);
            World.AddGameEventToCurrentLevel(delayedGameEvent2);
        }
    }
}
