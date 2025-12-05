using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace ChatService.Hubs
{
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var threadId = Context.GetHttpContext()?.Request.Query["threadId"];
            if (!string.IsNullOrEmpty(threadId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, threadId!);
            }

            await base.OnConnectedAsync();
        }

        public async Task JoinThread(string threadId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, threadId);
        }

        public async Task LeaveThread(string threadId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, threadId);
        }
    }

}
