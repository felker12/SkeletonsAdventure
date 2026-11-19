namespace RpgLibrary.QuestClasses;

public enum QuestLocationAction
{
    Interact,
    ReachArea
}

public class LocationTaskData : BaseTaskData
{
    public QuestLocationAction Action { get; set; }
    public string MapName { get; set; } = string.Empty;
    public int ObjectId { get; set; }

    public LocationTaskData() { }

    public LocationTaskData(LocationTaskData data) : base(data)
    {
        Action = data.Action;
        MapName = data.MapName;
        ObjectId = data.ObjectId;
    }

    public override LocationTaskData Clone() => new(this);

    public override string ToString() => $"{(Action == QuestLocationAction.Interact ? "Interact" : "Reach area")} — {TaskToComplete} ({MapName}, object {ObjectId})";
}
