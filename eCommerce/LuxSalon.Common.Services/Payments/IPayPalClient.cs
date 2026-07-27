namespace LuxSalon.Common.Services.Payments
{
    public class PayPalOrderResult
    {
        public string OrderId { get; set; } = string.Empty;
        public string ApprovalUrl { get; set; } = string.Empty;
    }

    public class PayPalCaptureResult
    {
        public bool Uspjesno { get; set; }
        public string CaptureId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public interface IPayPalClient
    {
        /// <summary>Kreira PayPal narudzbu (Orders API v2) i vraca OrderId + link na koji se klijent salje da odobri placanje.</summary>
        /// <param name="backendBaseUrl">
        /// Bazni URL nase WebAPI (npr. "http://10.0.2.2:5126" za Android emulator) - koristi se za
        /// return_url/cancel_url "most" stranicu (vidi PlacanjeController.PayPalReturn/PayPalCancel),
        /// jer PayPal-ova checkout stranica ne podrzava custom "luxsalon://" semu direktno.
        /// </param>
        Task<PayPalOrderResult> KreirajNarudzbuAsync(decimal iznos, string referenca, string backendBaseUrl);

        /// <summary>Nakon sto klijent odobri placanje na PayPalu, ovo "hvata" (capture) novac.</summary>
        Task<PayPalCaptureResult> PotvrdiNarudzbuAsync(string paypalOrderId);

        /// <summary>Vraca novac za vec uhvacenu (captured) uplatu.</summary>
        Task<bool> VratiNovacAsync(string captureId, decimal iznos);
    }
}
