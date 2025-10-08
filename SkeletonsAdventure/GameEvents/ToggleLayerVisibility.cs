
using MonoGame.Extended.Collisions.Layers;
using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkeletonsAdventure.GameEvents
{
    internal class ToggleLayerVisibility : GameEvent
    {
        public TiledMapTileLayer Layer { get; set; } = null;
        public new bool IsComplete = false;

        public ToggleLayerVisibility() { }

        public ToggleLayerVisibility(TiledMapTileLayer layer)
        {
            Layer = layer ?? throw new ArgumentNullException(nameof(layer));
            Duration = 0; // Instantaneous effect
        }

        public override void Update(GameTime gameTime)
        {
            if (Layer is not null && IsComplete is false)
            {
                Layer.IsVisible = !Layer.IsVisible;

                // Mark the event as processed to prevent repeated toggling
                IsComplete = true;
            }
        }
    }
}
