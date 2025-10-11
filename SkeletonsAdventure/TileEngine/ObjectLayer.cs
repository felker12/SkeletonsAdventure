using SkeletonsAdventure.ShapeEngine;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.TileEngine
{
    internal class ObjectLayer : Layer
    {
        public List<MapObject> MapObjects { get; set; } = [];

        private readonly Texture2D texture = GameManager.CreateTextureFromColor(Color.White);
        private Color color = Color.White;

        public ObjectLayer() : base()
        {
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // draw objects for debugging (must be set to visible)

            if (!Visible || MapObjects.Count == 0)
                return;

            foreach (var mapObject in MapObjects)
            {
                if (!mapObject.Visible)
                    continue;

                OutlineDrawer.DrawPolygonOutline(spriteBatch, texture, mapObject.PolygonPoints, color, 1f, true);
            }
        }

        public override string ToString()
        {
            return $"ObjectLayer ID: {ID}, Name: {Name}, Width: {Width}, Height: {Height}, Objects Count: {MapObjects.Count}, Visible: {Visible}";
        }
    }
}
