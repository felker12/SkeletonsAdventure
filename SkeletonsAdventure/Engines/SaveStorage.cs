using RpgLibrary.AttackData;
using RpgLibrary.DataClasses;
using RpgLibrary.MenuClasses;
using RpgLibrary.SettingsClasses;
using RpgLibrary.WorldClasses;
using System.IO;

namespace SkeletonsAdventure.Engines
{
    internal static class SaveStorage
    {
        private static readonly object Gate = new();
        internal const string FileName = "SaveGame.xml";

        public static void Write(string directory, SaveGameData data, bool preserveBackup = false)
        {
            lock (Gate)
            {
                Validate(data);
                Directory.CreateDirectory(directory);
                string target = Path.Combine(directory, FileName);
                string temporary = Path.Combine(directory, $".{Guid.NewGuid():N}.tmp");
                try
                {
                    XnaSerializer.Serialize(temporary, data);
                    // Validate the actual bytes before replacing the previous snapshot.
                    ReadSnapshot(temporary);
                    using (var stream = new FileStream(temporary, FileMode.Open, FileAccess.ReadWrite))
                        stream.Flush(flushToDisk: true);

                    if (File.Exists(target))
                    {
                        // Do not replace a good backup with a damaged primary after recovery.
                        bool validPrimary = false;
                        if (!preserveBackup)
                        {
                            try { ReadSnapshot(target); validPrimary = true; }
                            catch (Exception) { }
                        }
                        File.Replace(temporary, target, validPrimary ? target + ".bak" : null);
                    }
                    else
                        File.Move(temporary, target);
                }
                finally
                {
                    if (File.Exists(temporary))
                        File.Delete(temporary);
                }
            }
        }

        public static SaveGameData Read(string directory) => Read(directory, null, out _);

        public static SaveGameData Read(string directory, Action<SaveGameData> restore, out Exception primaryFailure)
        {
            lock (Gate)
            {
                primaryFailure = null;
                string target = Path.Combine(directory, FileName);
                string backup = target + ".bak";
                if (File.Exists(target) || File.Exists(backup))
                {
                    try { return ReadAndRestore(target); }
                    catch (Exception ex) when (File.Exists(backup))
                    {
                        primaryFailure = ex;
                        try { return ReadAndRestore(backup); }
                        catch (Exception backupFailure)
                        {
                            throw new InvalidDataException($"Could not load either '{target}' or '{backup}'.",
                                new AggregateException(ex, backupFailure));
                        }
                    }
                }

                // Read the original six-file format without modifying it.
                var legacy = new SaveGameData
                {
                    World = XnaSerializer.Deserialize<WorldData>(Path.Combine(directory, "World.xml")),
                    Menus = XnaSerializer.Deserialize<MenuManagerData>(Path.Combine(directory, "GameScreenMenuData.xml")),
                    PauseMenu = XnaSerializer.Deserialize<TabbedMenuData>(Path.Combine(directory, "ExitScreenData.xml")),
                    Messages = XnaSerializer.Deserialize<List<string>>(Path.Combine(directory, "MessageBox.xml")),
                    Bindings = XnaSerializer.Deserialize<KeyBindingsManagerData>(Path.Combine(directory, "Keybindings.xml")),
                    LearnedAttacks = XnaSerializer.Deserialize<LearnedAttackManagerData>(Path.Combine(directory, "LearnedAttackManager.xml"))
                };
                // Older builds saved bound defaults without recording them as learned.
                foreach (var name in legacy.Bindings.Keybindings.Values)
                    if (!string.IsNullOrEmpty(name) && !legacy.LearnedAttacks.LearnedAttackNames.Contains(name))
                        legacy.LearnedAttacks.LearnedAttackNames.Add(name);
                Validate(legacy);
                restore?.Invoke(legacy);
                return legacy;
            }

            SaveGameData ReadAndRestore(string path)
            {
                var data = ReadSnapshot(path);
                restore?.Invoke(data);
                return data;
            }
        }

        private static SaveGameData ReadSnapshot(string path)
        {
            var data = XnaSerializer.Deserialize<SaveGameData>(path);
            Validate(data);
            return data;
        }

        private static void Validate(SaveGameData data)
        {
            if (data is null || data.FormatVersion != 1)
                throw new InvalidDataException("Unsupported save version.");
            if (data.World?.PlayerData is null || data.World.Levels is null ||
                string.IsNullOrEmpty(data.World.CurrentLevel) ||
                !data.World.Levels.ContainsKey(data.World.CurrentLevel) ||
                data.Menus?.Menus is null || data.PauseMenu is null || data.Messages is null ||
                data.Bindings?.Keybindings is null || data.LearnedAttacks?.LearnedAttackNames is null)
                throw new InvalidDataException("The save is incomplete.");
        }
    }
}
