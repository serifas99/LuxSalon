using LuxSalon.Model.Requests;
using LuxSalon.Model.Responses;
using LuxSalon.Model.SearchObjects;

namespace LuxSalon.Services
{
    public interface ITerminService : IBaseCRUDService<TerminResponse, TerminSearchObject, TerminInsertRequest, TerminUpdateRequest>
    {
        Task<TerminResponse> PotvrdiAsync(int id);
        Task<TerminResponse> OtkaziAsync(int id);
        Task<TerminResponse> OznaciOdradjenAsync(int id);
        Task<TerminResponse> OznaciNijeSeOdazvaoAsync(int id);
        Task<List<string>> GetAllowedActionsAsync(int id);

        /// <summary>
        /// Dostupnost svakog dana u zadanom mjesecu za frizera/uslugu - koristi se za bojenje
        /// color-coded kalendara na mobileu (zeleno = ima slobodnih termina, crveno = nema).
        /// </summary>
        Task<List<DostupnostDanaResponse>> DostupnostAsync(int frizerId, int uslugaId, int godina, int mjesec);

        /// <summary>
        /// Konkretni slobodni vremenski slotovi ("HH:mm") za odabrani dan, frizera i uslugu.
        /// </summary>
        Task<List<string>> DostupniSlotoviAsync(int frizerId, int uslugaId, DateTime datum);
    }
}
