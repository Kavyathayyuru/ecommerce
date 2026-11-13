using OrderService.Models;

namespace OrderService.Services;

public static class OrderRules
{
    private static readonly string[] AllowedStatuses = ["Pending", "Processing", "Completed", "Cancelled"];

    public static bool IsValidRequest(int authenticatedUserId, int userId, int productId, int quantity) =>
        userId == authenticatedUserId && productId > 0 && quantity > 0;

    public static bool IsAllowedStatus(string? status) =>
        !string.IsNullOrWhiteSpace(status) && AllowedStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);

    public static string NormalizeStatus(string status) =>
        AllowedStatuses.First(allowed => allowed.Equals(status, StringComparison.OrdinalIgnoreCase));

    public static bool CanCancel(Order order) => order.Status != "Completed";
}
