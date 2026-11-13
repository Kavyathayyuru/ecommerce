using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OrderService.Hubs;

[Authorize]
public sealed class OrderUpdatesHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        if (int.TryParse(Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"customer:{userId}");
        }

        if (Context.User?.IsInRole("Admin") == true)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
        }

        await base.OnConnectedAsync();
    }
}
