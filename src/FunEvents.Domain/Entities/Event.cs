namespace FunEvents.Domain.Entities;

public class Event
{
    public Event()
    {
    }

    public Event(
        int id,
        string code,
        string name,
        DateTime eventDate,
        int capacity,
        EventStatus status = EventStatus.Draft)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Event code is required.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Event name is required.", nameof(name));

        //if (eventDate <= DateTime.UtcNow)
        //    throw new ArgumentException("Event date must be in the future.", nameof(eventDate));

        if (capacity <= 0)
            throw new ArgumentException("Event capacity must be greater than zero.", nameof(capacity));

        Id = id;
        Code = code;
        Name = name;
        EventDate = eventDate;
        Capacity = capacity;
        Status = status;
    }

    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateTime EventDate { get; set; }

    public int Capacity { get; set; }

    public EventStatus Status { get; set; }

    public void Publish()
    {
        if (Status != EventStatus.Draft)
            throw new InvalidOperationException("Only draft events can be published.");

        Status = EventStatus.Published;
    }

    public void Cancel()
    {
        if (Status == EventStatus.Completed)
            throw new InvalidOperationException("Completed events cannot be cancelled.");

        Status = EventStatus.Cancelled;
    }

    public void Complete()
    {
        if (Status != EventStatus.Published)
            throw new InvalidOperationException("Only published events can be completed.");

        Status = EventStatus.Completed;
    }
}