using MonoGame.Extended.Tiled;
using RpgLibrary.GameObjectClasses;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.GameObjects
{
    internal class Lever : InteractableObject
    {
        public string LeverPurpose { get; set; } = string.Empty;

        public Lever(TiledMapObject obj) : base(obj) { }

        public Lever(InteractableObject obj) : base(obj) { }

        public Lever(InteractableObjectData obj) : base(obj) { }

        public override void Interact(Player player)
        {
            //base.Interact(player);

            //Info.Visible = false;
            //Active = !Active;
            //Visible = !Visible;


            if(LeverPurpose == "ConditionalLayerToggle")
            {
                World.CurrentLevel.ConditionalLayer.IsVisible = !World.CurrentLevel.ConditionalLayer.IsVisible;
            }
        }
    }
}
