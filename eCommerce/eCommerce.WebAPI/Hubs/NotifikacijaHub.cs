using eCommerce.WebAPI.Services.AccessManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace eCommerce.WebAPI.Hubs
{
    /// <summary>
    /// SignalR hub za notifikacije u realnom vremenu. Svaki konektovani klijent se
    /// dodaje u grupu po svom KorisnikId (iz JWT-a), pa se poruke salju samo njemu -
    /// (npr. TerminService/PlacanjeService pozivaju Clients.Group(korisnikId) kad se
    /// desi nesto relevantno za tog korisnika: termin potvrdjen, placanje uspjesno, itd.)
    /// </summary>
    [Authorize]
    public class NotifikacijaHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var korisnikId = Context.User?.FindFirstValue(ClaimNames.Id)
                ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(korisnikId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, korisnikId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var korisnikId = Context.User?.FindFirstValue(ClaimNames.Id)
                ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(korisnikId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, korisnikId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
