using eCommerce.Common.Services.Realtime;
using eCommerce.WebAPI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace eCommerce.WebAPI.Services
{
    public class SignalRRealtimeNotifier : IRealtimeNotifier
    {
        private readonly IHubContext<NotifikacijaHub> _hubContext;
        private readonly ILogger<SignalRRealtimeNotifier> _logger;

        public SignalRRealtimeNotifier(IHubContext<NotifikacijaHub> hubContext, ILogger<SignalRRealtimeNotifier> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task ObavijestiKorisnikaAsync(int korisnikId, object notifikacija)
        {
            try
            {
                await _hubContext.Clients.Group(korisnikId.ToString()).SendAsync("NovaNotifikacija", notifikacija);
            }
            catch (Exception ex)
            {
                // Ne rusi glavnu operaciju (npr. zakazivanje termina) ako push obavijest ne uspije -
                // notifikacija je vec sacuvana u bazi, korisnik ce je vidjeti kad sljedeci put otvori app.
                _logger.LogWarning(ex, "Slanje SignalR notifikacije korisniku {KorisnikId} nije uspjelo.", korisnikId);
            }
        }
    }
}
