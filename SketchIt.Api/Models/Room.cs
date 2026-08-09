namespace SketchIt.Api.Models;

public class Room
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public DateTime CreatedAt { get; set; }
    public RoomStatus Status { get; set; }

    public string? Host { get; set; }
}

public enum RoomStatus
{
    Waiting,
    InProgress,
    Finished
}
