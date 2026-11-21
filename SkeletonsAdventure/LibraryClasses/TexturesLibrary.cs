
using SkeletonsAdventure.Content;

namespace SkeletonsAdventure.LibraryClasses
{
    internal class TexturesLibrary
    {
        private readonly ContentManager content;
        private readonly Dictionary<string, string> texturePaths;

        //Entity Textures
        public Texture2D SkeletonTexture { get; private set; }
        public Texture2D ArmoredSkeletonTexture { get; private set; }

        //Attack Textures
        public Texture2D AttackAreaTexture { get; private set; }
        public Texture2D SkeletonAttackTexture { get; private set; }
        public Texture2D FireBallTexture { get; private set; }
        public Texture2D FireBallTexture2 { get; private set; }
        public Texture2D IcePillarTexture { get; private set; }
        public Texture2D IcePillarSpriteSheetTexture { get; private set; }
        public Texture2D IceBulletTexture { get; private set; }
        public Texture2D IceBulletsTexture { get; private set; }
        public Texture2D WaterBallSpriteSheetTexture { get; private set; }
        public Texture2D FireWallTexture { get; private set; }
        public Texture2D BlueFireWallTexture { get; private set; }
        public Texture2D FireWallSpriteSheetTexture { get; private set; }
        public Texture2D BlueFireWallSpriteSheetTexture { get; private set; }
        public Texture2D TriangleAttackTexture { get; private set; }
        public Texture2D SpinningTriangleAttackTexture { get; private set; }

        //UI Textures
        public Texture2D ButtonBoxTexture { get; private set; }
        public Texture2D DefaultButtonTexture { get; private set; }
        public Texture2D GameMenuTexture { get; set; }
        public Texture2D BackpackBackground { get; set; }
        public Texture2D StatusBarTexture { get; set; }
        public Texture2D ButtonTexture { get; set; }
        public Texture2D TextBoxTexture { get; set; }
        public Texture2D TitleBackdrop { get; private set; }

        public GraphicsDevice GraphicsDevice { get; set; }

        public TexturesLibrary(ContentManager content, GraphicsDevice graphicsDevice)
        {
            this.content = content;
            texturePaths = content.Load<Dictionary<string, string>>(TextureCatalog.AssetName);
            GraphicsDevice = graphicsDevice;

            //Entity Textures
            LoadEntityTextures(content);

            //Attack Textures
            LoadAttackTextures(content, graphicsDevice);

            //UI Textures
            LoadUITextures(content, graphicsDevice);

        }

        private void LoadUITextures(ContentManager content, GraphicsDevice graphicsDevice)
        {
            ButtonBoxTexture = new(graphicsDevice, 1, 1);
            ButtonBoxTexture.SetData([new Color(83, 105, 140, 230)]);

            DefaultButtonTexture = new(graphicsDevice, 1, 1);
            DefaultButtonTexture.SetData([new Color(83, 105, 140, 230)]);

            GameMenuTexture = new(graphicsDevice, 1, 1);
            GameMenuTexture.SetData([new Color(171, 144, 91, 250)]);

            BackpackBackground = content.Load<Texture2D>(@"TiledFiles/BackpackBackground");

            StatusBarTexture = CreateTextureFromColor(Color.White);

            ButtonTexture = content.Load<Texture2D>("Controls/Button");
            TitleBackdrop = content.Load<Texture2D>("FrontEnd/DungeonBackdrop");

            TextBoxTexture = CreateTextureFromColor(new Color(210, 210, 210, 220));
        }

        private void LoadAttackTextures(ContentManager content, GraphicsDevice graphicsDevice)
        {
            FireBallTexture = content.Load<Texture2D>(@"AttackSprites/FireBall_01");
            FireBallTexture2 = content.Load<Texture2D>(@"AttackSprites/FireBallSpriteSheet");
            IcePillarTexture = content.Load<Texture2D>(@"AttackSprites/IcePillar");
            IcePillarSpriteSheetTexture = content.Load<Texture2D>(@"AttackSprites/IcePillarSpriteSheet");
            IceBulletTexture = content.Load<Texture2D>(@"AttackSprites/IceBullet");
            IceBulletsTexture = content.Load<Texture2D>(@"AttackSprites/IceBullets");
            WaterBallSpriteSheetTexture = content.Load<Texture2D>(@"AttackSprites/WaterBallSpriteSheet");
            FireWallTexture = content.Load<Texture2D>(@"AttackSprites/FireWall_Red");
            BlueFireWallTexture = content.Load<Texture2D>(@"AttackSprites/FireWall_Blue");
            FireWallSpriteSheetTexture = content.Load<Texture2D>(@"AttackSprites/FireWaveSpriteSheet");
            BlueFireWallSpriteSheetTexture = content.Load<Texture2D>(@"AttackSprites/FireWaveSpriteSheetBlue");
            TriangleAttackTexture = content.Load<Texture2D>(@"AttackSprites/TriangleAttack");
            SpinningTriangleAttackTexture = content.Load<Texture2D>(@"AttackSprites/SpinningTriangleAttack");

            AttackAreaTexture = new(graphicsDevice, 1, 1);
            AttackAreaTexture.SetData([new Color(153, 29, 20, 250)]);
        }

        private void LoadEntityTextures(ContentManager content)
        {
            SkeletonTexture = GetTexture(TextureCatalog.SkeletonBody);
            SkeletonAttackTexture = content.Load<Texture2D>(@"Player/SkeletonAttackSprites");
            ArmoredSkeletonTexture = content.Load<Texture2D>(@"Player/ArmoredSkeletonSpriteSheet");
        }

        public Texture2D GetTexture(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || !texturePaths.TryGetValue(name, out string path))
                throw new ContentLoadException($"Unknown texture name '{name}' in '{TextureCatalog.AssetName}'.");
            return content.Load<Texture2D>(path);
        }

        public Texture2D CreateTextureFromColor(Color color)
        {
            Texture2D texture = new(GraphicsDevice, 1, 1);
            texture.SetData([color]);

            return texture;
        }
    }
}
