using System.IO;

namespace SkeletonsAdventure.LibraryClasses
{
    internal class PathsLibrary
    {
        public string GamePath { get; private set; }
        public string SavePath { get; private set; }

        public PathsLibrary() 
        {
            // Resolve development saves from the executable, independent of the launch directory.
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "SkeletonsAdventure", "SkeletonsAdventure.csproj")))
                directory = directory.Parent;
            GamePath = directory is null ? AppContext.BaseDirectory : Path.Combine(directory.FullName, "SkeletonsAdventure");
            SavePath = directory is null
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SkeletonsAdventure", "SaveFiles")
                : Path.Combine(directory.FullName, "SaveFiles");
        }
    }
}
