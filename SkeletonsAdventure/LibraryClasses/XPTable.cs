using System.Globalization;
using System.IO;
using System.Linq;

namespace SkeletonsAdventure.LibraryClasses
{
    internal class XPTable
    {
        private const int DefaultLevelCap = 100;
        private const int DefaultCurveMultiplier = 16;
        private const int PreviousCurveMultiplier = 20;
        public IReadOnlyList<int> PlayerLevelXPs { get; }
        public int MaximumLevel => PlayerLevelXPs.Count - 1;
        public int MaximumXP => PlayerLevelXPs[MaximumLevel];

        public XPTable(string savePath)
        {
            PlayerLevelXPs = CreatePlayerLevelXPs(savePath).AsReadOnly();
        }

        public int GetPlayerLevelAtXP(int xp)
        {
            int level = 0;
            while (level < MaximumLevel && xp >= PlayerLevelXPs[level + 1])
                level++;
            return level;
        }

        public int GetLevelXPAtLevel(int level)
        {
            if (level < 0 || level > MaximumLevel)
                throw new ArgumentOutOfRangeException(nameof(level));
            return PlayerLevelXPs[level];
        }

        public (int Current, int Required, bool AtMaximumLevel) GetProgress(int xp)
        {
            int level = GetPlayerLevelAtXP(xp);
            if (level == MaximumLevel)
                return (1, 1, true);
            int start = PlayerLevelXPs[level];
            return (Math.Max(0, xp - start), PlayerLevelXPs[level + 1] - start, false);
        }

        public static List<int> CreatePlayerLevelXPs(string savePath)
        {
            Directory.CreateDirectory(savePath);
            string path = Path.Combine(savePath, "PlayerLevels.txt");
            List<int> thresholds = [];
            if (File.Exists(path))
            {
                foreach (string line in File.ReadLines(path))
                {
                    string[] parts = line.Split(',');
                    if (parts.Length != 2 ||
                        !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int level) ||
                        !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int xp) ||
                        level != thresholds.Count || xp < 0 ||
                        (level == 0 ? xp != 0 : xp <= thresholds[^1]))
                        throw new InvalidDataException($"Invalid XP table at line {thresholds.Count + 1}: {path}");
                    thresholds.Add(xp);
                }
                if (thresholds.Count == 0)
                    throw new InvalidDataException($"The XP table is empty: {path}");
                // Only migrate the old default. Keep custom thresholds and level caps.
                if (!thresholds.SequenceEqual(DefaultThresholds(PreviousCurveMultiplier)))
                    return thresholds;
            }

            thresholds = DefaultThresholds(DefaultCurveMultiplier).ToList();
            string temporary = path + $".{Guid.NewGuid():N}.tmp";
            try
            {
                File.WriteAllLines(temporary, thresholds.Select((xp, level) => FormattableString.Invariant($"{level},{xp}")));
                File.Move(temporary, path, overwrite: true);
            }
            finally
            {
                File.Delete(temporary);
            }

            return thresholds;
        }

        private static IEnumerable<int> DefaultThresholds(int multiplier) =>
            Enumerable.Range(0, DefaultLevelCap + 1)
                .Select(level => level == 0 ? 0 : (level + 1) * (level + 1) * multiplier);
    }
}
