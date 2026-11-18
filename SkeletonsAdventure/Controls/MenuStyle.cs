namespace SkeletonsAdventure.Controls
{
    internal static class MenuStyle
    {
        public static readonly Color Canvas = new(7, 18, 18);
        public static readonly Color Panel = new(10, 28, 27);
        public static readonly Color Surface = new(16, 39, 37);
        public static readonly Color Border = new(69, 98, 91);
        public static readonly Color Brass = new(194, 151, 95);
        public static readonly Color Ivory = new(252, 233, 201);
        public static readonly Color MutedText = new(185, 204, 194);
        public static readonly Color Selection = new(64, 46, 30);

        public static void Apply(Button button)
        {
            button.BackgroundColor = Surface;
            button.HoverColor = Selection;
            button.DisabledColor = new(43, 50, 47);
            button.TextColor = Ivory;
        }
    }
}
