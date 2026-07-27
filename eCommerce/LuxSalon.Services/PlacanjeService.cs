using LuxSalon.Common.Services.Messaging;
using LuxSalon.Common.Services.Payments;
using LuxSalon.Common.Services.Realtime;
using LuxSalon.Model.Exceptions;
using LuxSalon.Model.Responses;
using LuxSalon.Services.Database;
using Microsoft.EntityFrameworkCore;

namespace LuxSalon.Services
{
    /// <summary>
    /// Obradjuje placanje termina preko PayPal-a (sandbox). Napomena: PayPal ne podrzava BAM,
    /// pa se cijena usluge (u KM) direktno koristi kao iznos u USD - pojednostavljenje za potrebe projekta.
    /// </summary>
    public class PlacanjeService : IPlacanjeService
    {
        private readonly ECommerceDbContext _dbContext;
        private readonly IPayPalClient _payPalClient;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;
        private readonly IRealtimeNotifier _realtimeNotifier;
        private readonly IAuthenticatedUserAccessor _userAccessor;

        public PlacanjeService(ECommerceDbContext dbContext, IPayPalClient payPalClient, IRabbitMqPublisher rabbitMqPublisher, IRealtimeNotifier realtimeNotifier, IAuthenticatedUserAccessor userAccessor)
        {
            _dbContext = dbContext;
            _payPalClient = payPalClient;
            _rabbitMqPublisher = rabbitMqPublisher;
            _realtimeNotifier = realtimeNotifier;
            _userAccessor = userAccessor;
        }

        // Provjerava da JWT-om prijavljeni korisnik zaista vlasnik datog termina - sprjecava
        // da jedan klijent placa/potvrdjuje placanje za tudji termin (ownership check, ne vjeruje
        // se nikakvom klijentId-u koji bi eventualno dosao kroz request, sve ide preko tokena).
        private void ProvjeriVlasnistvo(Termin termin)
        {
            if (_userAccessor.IsInRole("Admin") || _userAccessor.IsInRole("Frizer"))
                return;

            var korisnikId = _userAccessor.GetUserId();
            if (korisnikId == null)
                throw new ClinetException("Korisnik nije prijavljen.");

            if (termin.KlijentId != korisnikId.Value)
                throw new ClinetException("Mozete platiti samo svoj termin.");
        }

        // Refund je iskljucivo akcija osoblja (Admin/Frizer) - klijent ne moze sam sebi vratiti novac.
        private void ProvjeriOvlascenjeZaRefund()
        {
            if (!_userAccessor.IsInRole("Admin") && !_userAccessor.IsInRole("Frizer"))
                throw new ClinetException("Samo osoblje (frizer/admin) moze izvrsiti povrat novca.");
        }

        public async Task<PlacanjeKreirajResponse> KreirajAsync(int terminId, string backendBaseUrl)
        {
            var termin = await _dbContext.Termini.FindAsync(terminId);
            if (termin == null)
                throw new KeyNotFoundException($"Termin with id {terminId} not found.");

            ProvjeriVlasnistvo(termin);

            if (termin.Status == TerminStatus.Otkazan || termin.Status == TerminStatus.NijeSeOdazvao)
                throw new ClinetException("Ne moze se platiti otkazan termin.");

            // Placanje je 1:1 sa Terminom (baza ima unique index na TerminId), pa ne mozemo
            // dodati novi red za isti termin - ako postoji neuspjeli pokusaj, ponovo ga iskoristimo.
            var postojece = await _dbContext.Placanja.FirstOrDefaultAsync(p => p.TerminId == terminId);

            if (postojece != null)
            {
                if (postojece.Status == PlacanjeStatus.Zavrseno)
                    throw new ClinetException("Termin je vec placen.");

                if (postojece.Status == PlacanjeStatus.NaCekanju)
                    throw new ClinetException("Vec postoji placanje na cekanju za ovaj termin. Zavrsite ili sacekajte to placanje.");

                // Status je Neuspjesno - novi pokusaj, prepisujemo isti red.
                var noviRezultat = await _payPalClient.KreirajNarudzbuAsync(termin.Cijena, $"termin-{termin.Id}", backendBaseUrl);

                postojece.PaypalOrderId = noviRezultat.OrderId;
                postojece.PaypalTransactionId = null;
                postojece.Status = PlacanjeStatus.NaCekanju;
                postojece.Iznos = termin.Cijena;
                postojece.DatumPlacanja = null;
                postojece.DatumPovrata = null;
                postojece.CreatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                return new PlacanjeKreirajResponse
                {
                    PlacanjeId = postojece.Id,
                    PaypalOrderId = noviRezultat.OrderId,
                    ApprovalUrl = noviRezultat.ApprovalUrl
                };
            }

            var paypalRezultat = await _payPalClient.KreirajNarudzbuAsync(termin.Cijena, $"termin-{termin.Id}", backendBaseUrl);

            var placanje = new Placanje
            {
                TerminId = termin.Id,
                Iznos = termin.Cijena,
                Status = PlacanjeStatus.NaCekanju,
                PaypalOrderId = paypalRezultat.OrderId,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Placanja.Add(placanje);
            await _dbContext.SaveChangesAsync();

            return new PlacanjeKreirajResponse
            {
                PlacanjeId = placanje.Id,
                PaypalOrderId = paypalRezultat.OrderId,
                ApprovalUrl = paypalRezultat.ApprovalUrl
            };
        }

        public async Task<PlacanjeResponse> PotvrdiAsync(string paypalOrderId)
        {
            var placanje = await _dbContext.Placanja.FirstOrDefaultAsync(p => p.PaypalOrderId == paypalOrderId);
            if (placanje == null)
                throw new KeyNotFoundException($"Placanje with PayPal order id {paypalOrderId} not found.");

            var terminZaProvjeru = await _dbContext.Termini.FindAsync(placanje.TerminId);
            if (terminZaProvjeru == null)
                throw new KeyNotFoundException($"Termin with id {placanje.TerminId} not found.");

            ProvjeriVlasnistvo(terminZaProvjeru);

            if (placanje.Status == PlacanjeStatus.Zavrseno)
                return MapToResponse(placanje); // idempotentno

            var captureRezultat = await _payPalClient.PotvrdiNarudzbuAsync(paypalOrderId);

            if (!captureRezultat.Uspjesno)
            {
                placanje.Status = PlacanjeStatus.Neuspjesno;
                await _dbContext.SaveChangesAsync();
                throw new ClinetException("Placanje na PayPalu nije uspjelo ili nije odobreno.");
            }

            placanje.Status = PlacanjeStatus.Zavrseno;
            placanje.PaypalTransactionId = captureRezultat.CaptureId;
            placanje.DatumPlacanja = DateTime.UtcNow;

            var termin = await _dbContext.Termini.FindAsync(placanje.TerminId);
            if (termin != null && termin.Status == TerminStatus.Zakazan)
            {
                termin.Status = TerminStatus.Potvrdjen;
                termin.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            if (termin != null)
            {
                var klijent = await _dbContext.Users.FindAsync(termin.KlijentId);
                if (klijent != null)
                {
                    var naslov = "Placanje uspjesno";
                    var poruka = $"Uspjesno ste platili termin ({placanje.Iznos:F2} USD). Termin je potvrdjen.";

                    var notifikacija = new Notifikacija
                    {
                        KorisnikId = termin.KlijentId,
                        Naslov = naslov,
                        Poruka = poruka,
                        Tip = NotifikacijaTip.PlacanjeUspjesno,
                        Procitano = false,
                        CreatedAt = DateTime.UtcNow,
                        TerminId = termin.Id
                    };

                    _dbContext.Notifikacije.Add(notifikacija);
                    await _dbContext.SaveChangesAsync();

                    _rabbitMqPublisher.PublishNotifikacija(klijent.Email, naslov, poruka);

                    await _realtimeNotifier.ObavijestiKorisnikaAsync(termin.KlijentId, new
                    {
                        notifikacija.Id,
                        notifikacija.Naslov,
                        notifikacija.Poruka,
                        Tip = notifikacija.Tip.ToString(),
                        notifikacija.Procitano,
                        notifikacija.CreatedAt,
                        notifikacija.TerminId
                    });
                }
            }

            return MapToResponse(placanje);
        }

        public async Task<PlacanjeResponse> VratiNovacAsync(int placanjeId)
        {
            ProvjeriOvlascenjeZaRefund();

            var placanje = await _dbContext.Placanja.FindAsync(placanjeId);
            if (placanje == null)
                throw new KeyNotFoundException($"Placanje with id {placanjeId} not found.");

            if (placanje.Status != PlacanjeStatus.Zavrseno || string.IsNullOrWhiteSpace(placanje.PaypalTransactionId))
                throw new ClinetException("Samo zavrseno placanje moze biti vraceno.");

            var uspjesno = await _payPalClient.VratiNovacAsync(placanje.PaypalTransactionId, placanje.Iznos);
            if (!uspjesno)
                throw new ClinetException("Povrat novca preko PayPal-a nije uspio.");

            placanje.Status = PlacanjeStatus.Vraceno;
            placanje.DatumPovrata = DateTime.UtcNow;

            var termin = await _dbContext.Termini.FindAsync(placanje.TerminId);
            if (termin != null && termin.Status != TerminStatus.Odradjen)
            {
                termin.Status = TerminStatus.Otkazan;
                termin.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            if (termin != null)
            {
                var klijent = await _dbContext.Users.FindAsync(termin.KlijentId);
                if (klijent != null)
                {
                    var naslov = "Novac vracen";
                    var poruka = $"Vraceno vam je {placanje.Iznos:F2} USD za otkazan termin.";

                    var notifikacija = new Notifikacija
                    {
                        KorisnikId = termin.KlijentId,
                        Naslov = naslov,
                        Poruka = poruka,
                        Tip = NotifikacijaTip.PlacanjeVraceno,
                        Procitano = false,
                        CreatedAt = DateTime.UtcNow,
                        TerminId = termin.Id
                    };

                    _dbContext.Notifikacije.Add(notifikacija);
                    await _dbContext.SaveChangesAsync();

                    _rabbitMqPublisher.PublishNotifikacija(klijent.Email, naslov, poruka);

                    await _realtimeNotifier.ObavijestiKorisnikaAsync(termin.KlijentId, new
                    {
                        notifikacija.Id,
                        notifikacija.Naslov,
                        notifikacija.Poruka,
                        Tip = notifikacija.Tip.ToString(),
                        notifikacija.Procitano,
                        notifikacija.CreatedAt,
                        notifikacija.TerminId
                    });
                }
            }

            return MapToResponse(placanje);
        }

        public async Task<PlacanjeResponse> GetByIdAsync(int id)
        {
            var placanje = await _dbContext.Placanja.FindAsync(id);
            if (placanje == null)
                throw new KeyNotFoundException($"Placanje with id {id} not found.");

            var termin = await _dbContext.Termini.FindAsync(placanje.TerminId);
            if (termin != null)
                ProvjeriVlasnistvo(termin);

            return MapToResponse(placanje);
        }

        private static PlacanjeResponse MapToResponse(Placanje placanje)
        {
            return new PlacanjeResponse
            {
                Id = placanje.Id,
                TerminId = placanje.TerminId,
                Iznos = placanje.Iznos,
                Status = placanje.Status.ToString(),
                PaypalOrderId = placanje.PaypalOrderId,
                PaypalTransactionId = placanje.PaypalTransactionId,
                CreatedAt = placanje.CreatedAt,
                DatumPlacanja = placanje.DatumPlacanja,
                DatumPovrata = placanje.DatumPovrata
            };
        }
    }
}
