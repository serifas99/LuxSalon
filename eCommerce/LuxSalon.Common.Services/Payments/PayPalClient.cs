using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LuxSalon.Common.Services.Payments
{
    /// <summary>
    /// Tanak wrapper oko PayPal REST (Orders v2) API-ja - sandbox okruzenje.
    /// Napomena: PayPal ne podrzava BAM (konvertibilnu marku), pa se placanja rade u USD.
    /// </summary>
    public class PayPalClient : IPayPalClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PayPalClient> _logger;

        private const string Currency = "USD";

        public PayPalClient(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<PayPalClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        private string BaseUrl => _configuration["PayPal:BaseUrl"] ?? "https://api-m.sandbox.paypal.com";
        private string ClientId => _configuration["PayPal:ClientId"] ?? string.Empty;
        private string ClientSecret => _configuration["PayPal:ClientSecret"] ?? string.Empty;

        private async Task<string> DobaviTokenAsync(HttpClient client)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v1/oauth2/token");
            var authBytes = Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials"
            });

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("access_token").GetString() ?? string.Empty;
        }

        public async Task<PayPalOrderResult> KreirajNarudzbuAsync(decimal iznos, string referenca, string backendBaseUrl)
        {
            var client = _httpClientFactory.CreateClient();
            var token = await DobaviTokenAsync(client);

            var body = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        reference_id = referenca,
                        amount = new
                        {
                            currency_code = Currency,
                            value = iznos.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
                        }
                    }
                },
                application_context = new
                {
                    // PayPal-ova checkout stranica ne podrzava custom "luxsalon://" semu direktno kao
                    // return_url/cancel_url (ostaje zaglavljena u ucitavanju) - zato ide preko nase
                    // WebAPI "most" stranice (PlacanjeController.PayPalReturn/PayPalCancel) koja u
                    // browseru na telefonu radi JS/meta-refresh redirekciju na "luxsalon://..." i tek
                    // to hvata mobilna app (AndroidManifest.xml intent-filter + placanje_screen.dart).
                    return_url = $"{backendBaseUrl}/Placanje/PayPalReturn",
                    cancel_url = $"{backendBaseUrl}/Placanje/PayPalCancel",
                    brand_name = "LuxSalon",
                    user_action = "PAY_NOW"
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v2/checkout/orders");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = JsonContent.Create(body);

            var response = await client.SendAsync(request);
            var responseJson = await response.Content.ReadFromJsonAsync<JsonElement>();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal CreateOrder neuspjesan: {Response}", responseJson);
                throw new InvalidOperationException("Kreiranje PayPal narudzbe nije uspjelo.");
            }

            var orderId = responseJson.GetProperty("id").GetString() ?? string.Empty;
            var approvalUrl = string.Empty;

            foreach (var link in responseJson.GetProperty("links").EnumerateArray())
            {
                if (link.GetProperty("rel").GetString() == "approve")
                {
                    approvalUrl = link.GetProperty("href").GetString() ?? string.Empty;
                    break;
                }
            }

            return new PayPalOrderResult { OrderId = orderId, ApprovalUrl = approvalUrl };
        }

        public async Task<PayPalCaptureResult> PotvrdiNarudzbuAsync(string paypalOrderId)
        {
            var client = _httpClientFactory.CreateClient();
            var token = await DobaviTokenAsync(client);

            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v2/checkout/orders/{paypalOrderId}/capture");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = JsonContent.Create(new { });

            var response = await client.SendAsync(request);
            var responseJson = await response.Content.ReadFromJsonAsync<JsonElement>();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal Capture neuspjesan: {Response}", responseJson);
                return new PayPalCaptureResult { Uspjesno = false, Status = "FAILED" };
            }

            var status = responseJson.GetProperty("status").GetString() ?? string.Empty;
            var captureId = responseJson
                .GetProperty("purchase_units")
                .EnumerateArray().First()
                .GetProperty("payments")
                .GetProperty("captures")
                .EnumerateArray().First()
                .GetProperty("id")
                .GetString() ?? string.Empty;

            return new PayPalCaptureResult
            {
                Uspjesno = status == "COMPLETED",
                CaptureId = captureId,
                Status = status
            };
        }

        public async Task<bool> VratiNovacAsync(string captureId, decimal iznos)
        {
            var client = _httpClientFactory.CreateClient();
            var token = await DobaviTokenAsync(client);

            var body = new
            {
                amount = new
                {
                    value = iznos.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                    currency_code = Currency
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v2/payments/captures/{captureId}/refund");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = JsonContent.Create(body);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                _logger.LogError("PayPal Refund neuspjesan: {Response}", errorJson);
                return false;
            }

            return true;
        }
    }
}
