namespace eCommerce.Common.Services.Payments
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
        Task<PayPalOrderResult> KreirajNarudzbuAsync(decimal iznos, string referenca);

        /// <summary>Nakon sto klijent odobri placanje na PayPalu, ovo "hvata" (capture) novac.</summary>
        Task<PayPalCaptureResult> PotvrdiNarudzbuAsync(string paypalOrderId);

        /// <summary>Vraca novac za vec uhvacenu (captured) uplatu.</summary>
        Task<bool> VratiNovacAsync(string captureId, decimal iznos);
    }
}
