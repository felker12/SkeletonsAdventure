using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using SkeletonsAdventure.Controls;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.Entities;
using SkeletonsAdventure.Quests;
using System.Linq;
using System.Text;

namespace SkeletonsAdventure.GameObjects
{
    internal class Teleporter
    {
        internal string Name { get; set; } = string.Empty;
        internal string DestinationName { get; set; } = string.Empty;
        internal Vector2 Destination { get; set; } = new();
        internal Vector2 Position { get; set; } = Vector2.Zero;
        internal int Width { get; set; } = 32;
        internal int Height { get; set; } = 32;
        internal Rectangle Rectangle => new((int)Position.X, (int)Position.Y, Width, Height);
        internal List<string> RequiredQuestNames { get; set; } = [];
        internal LevelRequirements Requirements { get; set; } = new();
        internal bool Teleported { get; set; } = false;
        internal Label Info { get; set; } = new(string.Empty)
        {
            Visible = true,
        };

        internal Teleporter() { }

        internal Teleporter(string name)
        {
            Name = name;
        }

        internal Teleporter(string name, Vector2 destination)
        {
            Name = name;
            Destination = destination;
        }

        internal void Update(Player player)
        {
            // Check if the player is within the teleporter's rectangle
            if (Intersects(player.Rectangle))
            {
                Info.Visible = true;
                // Logic to handle player interaction with the teleporter
                if (CheckRequirements(player))
                {
                    if (InputHandler.KeyReleased(Keys.R) ||
                        InputHandler.ButtonDown(Buttons.A, PlayerIndex.One))
                    {
                        player.Position = Destination;
                        Teleported = true;
                        Info.Visible = false;
                    }
                }
            }
            else
            {
                Info.Visible = false;
            }
        }

        internal void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawRectangle(Rectangle, Color.White, 1, 1);

            if(Info.Visible)
                Info.Draw(spriteBatch);
        }

        internal Teleporter Clone()
        {
            return new Teleporter(Name, Destination);
        }

        internal bool Intersects(Rectangle rec)
        {
            return Rectangle.Intersects(rec);
        }

        internal bool CheckRequirements(Player player)
        {
            if (player is null)
                throw new ArgumentNullException(nameof(player), "Player cannot be null.");
            if (RequiredQuestNames.Count > 0
                && !RequiredQuestNames.Any(q => player.CompletedQuests.Any(quest => quest.Name == q && quest.IsCompleted)))
                return false;

            if (Requirements.CheckRequirements(player))
            {
                Info.Text = "Press R to interact";
                return true;
            }
            else
            {
                Info.Text = "Requirements not met";
                return false;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine($"Teleporter: {Name}");
            sb.AppendLine($"Destination Name: {DestinationName}");
            sb.AppendLine($"Destination: {Destination}");
            sb.AppendLine($"Position: {Position}");
            sb.AppendLine($"Rectangle: {Rectangle}");
            if (RequiredQuestNames.Count > 0)
                sb.AppendLine($"Required Quests: {string.Join(", ", RequiredQuestNames)}");

            return sb.ToString();
        }
    }
}
