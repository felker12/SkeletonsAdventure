using RpgLibrary.QuestClasses;
using SkeletonsAdventure.Entities.PlayerClasses;

namespace SkeletonsAdventure.Quests;

internal sealed class QuestAreaTracker(string mapName)
{
    private readonly List<(int Id, Rectangle Bounds)> areas = [];
    private HashSet<int> occupied = [];
    private HashSet<int> current = [];
    private readonly HashSet<int> entered = [];

    public void Add(int id, Rectangle bounds) => areas.Add((id, bounds));

    public void Reset() => occupied.Clear();

    public void Update(Player player)
    {
        if (areas.Count == 0)
            return;
        if (player.IsDead || player.Health <= 0)
        {
            Reset();
            return;
        }

        current.Clear();
        entered.Clear();
        foreach (var area in areas)
            if (area.Bounds.Intersects(player.Rectangle) && current.Add(area.Id) && !occupied.Contains(area.Id))
                entered.Add(area.Id);

        (occupied, current) = (current, occupied);
        if (entered.Count > 0)
            player.ProgressLocationTasks(QuestLocationAction.ReachArea, mapName, entered.Contains);
    }
}
