using Microsoft.AspNetCore.SignalR;

namespace DemoBanCo_20DTHE2.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
            //Clients.Clients()
        }
        public override Task OnConnectedAsync()
        {
            string id = Context.ConnectionId;
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            string id = Context.ConnectionId;
            return base.OnDisconnectedAsync(exception);
        }
        public async Task SendToGroup(string roomId, string user, string message)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            await Clients.Group(roomId).SendAsync("ReceiveMessage", user, message);
        }
    }
}
