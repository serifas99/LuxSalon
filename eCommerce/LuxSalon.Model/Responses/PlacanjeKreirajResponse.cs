namespace LuxSalon.Model.Responses
{
    public class PlacanjeKreirajResponse
    {
        public int PlacanjeId { get; set; }
        public string PaypalOrderId { get; set; } = string.Empty;

        /// <summary>
        /// Link na koji treba preusmjeriti klijenta da odobri placanje na PayPalu (sandbox).
        /// </summary>
        public string ApprovalUrl { get; set; } = string.Empty;
    }
}
