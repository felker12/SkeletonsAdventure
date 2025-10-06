using System.Xml.Linq;

namespace SkeletonsAdventure.TileEngine
{
    internal readonly struct XDoc
    {
        public readonly XDocument Doc { get; private init; }
        public readonly XElement Map { get; private init; }
        public readonly int TileWidth { get; private init; }
        public readonly int TileHeight { get; private init; }

        public XDoc(string tmxPath)
        {
            this.Doc = XDocument.Load(tmxPath);
            this.Map = Doc.Element("map") ?? throw new InvalidOperationException("TMX file must have a <map> element.");
            this.TileWidth = int.Parse(Map.Attribute("tilewidth").Value);
            this.TileHeight = int.Parse(Map.Attribute("tileheight").Value);
        }
    }
}
