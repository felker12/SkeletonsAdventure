
namespace SkeletonsAdventure.LibraryClasses
{
    internal class FontsLibrary
    {
        //SpriteFonts
        public SpriteFont Arial10 { get; private set; }
        public SpriteFont Arial12 { get; private set; }
        public SpriteFont Arial14 { get; private set; }
        public SpriteFont Arial16 { get; private set; }
        public SpriteFont Arial18 { get; private set; }
        public SpriteFont Arial20 { get; private set; }

        public FontsLibrary(ContentManager content)
        {
            LoadArialFonts(content);
        }

        private void LoadArialFonts(ContentManager content)
        {
            Arial10 = content.Load<SpriteFont>("Fonts/Arial10");
            Arial12 = content.Load<SpriteFont>("Fonts/Arial12");
            Arial14 = content.Load<SpriteFont>("Fonts/Arial14");
            Arial16 = content.Load<SpriteFont>("Fonts/Arial16");
            Arial18 = content.Load<SpriteFont>("Fonts/Arial18");
            Arial20 = content.Load<SpriteFont>("Fonts/Arial20");
        }
    }
}
