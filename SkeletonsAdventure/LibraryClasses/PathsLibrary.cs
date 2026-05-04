using System.IO;

namespace SkeletonsAdventure.LibraryClasses
{
    internal class PathsLibrary
    {
        public string GamePath { get; private set; }
        public string SavePath { get; private set; }

        public PathsLibrary() 
        {
            GamePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName; //Project Directory
            SavePath = Path.GetFullPath(Path.Combine(GamePath, @"..\SaveFiles")); //Directory of the saved files
        }
    }
}
