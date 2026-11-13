using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Hubs;
using OrderService.Models;

namespace OrderService.Services;

public static class OrderStatusOutbox
{
    public static void Record(OrderDbContext context, Order order) =>
        context.OrderOutboxMessages.Add(new OrderOutboxMessage
        {
            Order = order,
            UserId = order.UserId,
            Status = order.Status,
            CreatedAtUtc = DateTime.UtcNow
        });
}

public sealed class OrderStatusPublisher(
    IServiceScopeFactory scopeFactory,
    IHubContext<OrderUpdatesHub> hubContext,
    ILogger<OrderStatusPublisher> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);
        do
        {
            try
            {
                await PublishPendingAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to publish order status notifications.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task PublishPendingAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var events = await context.OrderOutboxMessages
            .Where(message => message.ProcessedAtUtc == null)
            .OrderBy(message => message.Id)
            .Take(100)
            .ToListAsync(cancellationToken);

        foreach (var message in events)
        {
            var payload = JsonSerializer.Serialize(new { orderId = message.OrderId, status = message.Status });
            try
            {
                await hubContext.Clients.Group($"customer:{message.UserId}").SendAsync("OrderStatusChanged", payload, cancellationToken);
                await hubContext.Clients.Group("admins").SendAsync("OrderStatusChanged", payload, cancellationToken);
                message.ProcessedAtUtc = DateTime.UtcNow;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception, "Order notification {OutboxMessageId} will be retried.", message.Id);
                break;
            }
        }

        if (events.Count > 0) await context.SaveChangesAsync(cancellationToken);
    }
}
