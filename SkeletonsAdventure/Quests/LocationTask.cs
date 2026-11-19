using RpgLibrary.QuestClasses;

namespace SkeletonsAdventure.Quests;

internal sealed class LocationTask(LocationTaskData data) : BaseTask(data)
{
    public QuestLocationAction Action { get; } = data.Action;
    public string MapName { get; } = data.MapName;
    public int ObjectId { get; } = data.ObjectId;

    public override LocationTaskData GetBaseTaskData() => new()
    {
        TaskToComplete = TaskToComplete,
        RequiredAmount = RequiredAmount,
        CompletedAmount = CompletedAmount,
        Action = Action,
        MapName = MapName,
        ObjectId = ObjectId
    };

    public override LocationTask Clone() => new(GetBaseTaskData());
}
