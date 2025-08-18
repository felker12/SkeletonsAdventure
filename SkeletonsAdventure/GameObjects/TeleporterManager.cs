using SkeletonsAdventure.Entities;
using System.Linq;

namespace SkeletonsAdventure.GameObjects
{
    internal class TeleporterManager()
    {
        internal List<Teleporter> Teleporters { get; private set; } = [];
        internal bool Teleported { get; set; } = false;
        
        internal void Update(Player player)
        {
            if (Teleported)
            {
                Teleported = false;
                return;
            }

            foreach (Teleporter teleporter in Teleporters)
            {
                if (teleporter is null)
                    continue;

                teleporter.Update(player);

                if(teleporter.Teleported)
                {
                    Teleported = true;
                    teleporter.Teleported = false;
                    break;
                }
            }
        }

        internal void Draw(SpriteBatch spriteBatch)
        {
            foreach (var teleporter in Teleporters)
            {
                teleporter.Draw(spriteBatch);
            }
        }

        internal void AddTeleporter(Teleporter teleporter)
        {
            if (teleporter is null)
                throw new ArgumentNullException(nameof(teleporter), "Teleporter cannot be null.");

            Teleporters.Add(teleporter);
        }

        internal void RemoveTeleporter(Teleporter teleporter)
        {
            if (teleporter is null)
                throw new ArgumentNullException(nameof(teleporter), "Teleporter cannot be null.");

            Teleporters.Remove(teleporter);
        }

        internal void ClearTeleporters()
        {
            Teleporters.Clear();
        }

        internal Teleporter GetTeleporterByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Teleporter name cannot be null or empty.", nameof(name));

            return Teleporters.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        internal void SetDestinationForAllTeleporters()
        {
            List<string> names = [.. Teleporters.Select(t => t.Name).Distinct()];

            foreach (var teleporter in Teleporters)
            {
                if (teleporter is null)
                    continue;

                foreach(string name in names)
                {
                    if (teleporter.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(teleporter.DestinationName))
                    {
                        Teleporter destinationTeleporter = GetTeleporterByName(teleporter.DestinationName);
                        if (destinationTeleporter != null)
                        {
                            teleporter.Destination = destinationTeleporter.Position;
                        }
                    }
                }
            }
        }
    }
}
