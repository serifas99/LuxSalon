namespace eCommerce.Common.Services.Realtime
{
    public interface IRealtimeNotifier
    {
        /// <summary>
        /// Salje "NovaNotifikacija" event uzivo (SignalR) konektovanom korisniku, ako je trenutno online.
        /// Notifikacija je u tom trenutku vec sacuvana u bazi (NotifikacijaService/direktan insert) -
        /// ovo je samo "best effort" push da se otvorena desktop/mobilna app odmah azurira bez
        /// rucnog refresh-a. Ne baca izuzetak ako korisnik nije konektovan.
        /// </summary>
        Task ObavijestiKorisnikaAsync(int korisnikId, object notifikacija);
    }
}
