using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

namespace RpgLibrary.DataClasses
{
    public static class XnaSerializer
    {
        // Keep relative references as stored in the XML, with no relocation root.
        private const string NoReferenceRelocation = null!;

        public static void Serialize<T>(string filename, T data)
        {
            XmlWriterSettings settings = new()
            {
                Indent = true
            };

            using XmlWriter writer = XmlWriter.Create(filename, settings);

            IntermediateSerializer.Serialize<T>(writer, data, NoReferenceRelocation);
        }

        public static T Deserialize<T>(string filename)
        {
            T data;
            using (FileStream stream = new(filename, FileMode.Open))
            {
                using XmlReader reader = XmlReader.Create(stream);

                data = IntermediateSerializer.Deserialize<T>(reader, NoReferenceRelocation);
            }
            return data;
        }
    }
}
