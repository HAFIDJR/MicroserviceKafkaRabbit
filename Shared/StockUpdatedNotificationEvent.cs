namespace Shared.Events;

public class StockUpdatedNotificationEvent
{
    public int ProductId { get; set; }

    public int RemainingStock { get; set; }

    public string Message { get; set; } =
        string.Empty;
}