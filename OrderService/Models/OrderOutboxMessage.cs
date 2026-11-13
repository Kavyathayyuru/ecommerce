namespace OrderService.Models;

public sealed class OrderOutboxMessage
{
    public long Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
}
