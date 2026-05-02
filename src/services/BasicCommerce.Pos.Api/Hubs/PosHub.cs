using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BasicCommerce.Pos.Api.Hubs;

[Authorize]
public class PosHub : Hub
{
    public async Task JoinStore(string storeId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"store-{storeId}");
    }

    public async Task JoinTerminal(string terminalId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"terminal-{terminalId}");
    }

    public async Task LeaveStore(string storeId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"store-{storeId}");
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}

public static class PosHubEvents
{
    public const string PriceUpdated = "PriceUpdated";
    public const string StockUpdated = "StockUpdated";
    public const string TransactionCompleted = "TransactionCompleted";
    public const string TerminalStatusChanged = "TerminalStatusChanged";
    public const string LowStockAlert = "LowStockAlert";
}
