using System.IO;

namespace SkeletonsAdventure.LibraryClasses
{
    internal class XPTable
    {
        public List<int> PlayerLevelXPs { get; private set; } = [];

        public XPTable(string savePath) 
        {
            PlayerLevelXPs = CreatePlayerLevelXPs(savePath);
        }

        //Get the level the player is at given the XP
        public int GetPlayerLevelAtXP(int XP)
        {
            int level = 0;

            foreach (var levelXP in PlayerLevelXPs)
                if (XP > levelXP)
                    level = PlayerLevelXPs.IndexOf(levelXP);

            return level;
        }

        //Get the XP needed for the level
        public int GetLevelXPAtLevel(int level)
        {
            return PlayerLevelXPs[level];
        }

        public static List<int> CreatePlayerLevelXPs(string savePath) //TODO adjust the xp as needed
        {
            string levelsSavePath = Path.Combine(savePath, "PlayerLevels.txt");
            List<int> playerLevelXPs = [];

            if (File.Exists(levelsSavePath)) //If the file exists load the data
            {
                List<string> lines = [.. File.ReadAllLines(levelsSavePath)];

                foreach (var line in lines)
                {
                    string[] parts = line.Split(',');
                    if (int.TryParse(parts[1], out int xp))
                    {
                        playerLevelXPs.Add(xp);
                    }
                }
            }
            else
            {
                //File.CreateText(levelsSavePath).Close(); //create the file if it doesn't exist
                string levels = string.Empty;

                for (int i = 0; i < 101; i++)
                {
                    if (i == 0)
                        playerLevelXPs.Add(0);
                    else
                        playerLevelXPs.Add((int)Math.Pow(i + 1, 2) * 20);

                    levels += $"{i},{playerLevelXPs[i]}" + Environment.NewLine;
                }

                File.WriteAllText(levelsSavePath, levels);
            }

            return playerLevelXPs;
        }
    }
}