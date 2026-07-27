using LuxSalon.Model.Responses;
using LuxSalon.Services.Database;
using Microsoft.EntityFrameworkCore;

namespace LuxSalon.Services
{
    /// <summary>
    /// Hibridni sistem preporuke usluga - implementacija tacno prema opisu iz prijave teme (poglavlje 4.6):
    ///
    ///   - Content-Based Filtering: analizira atribute usluga koje je klijent ranije koristio
    ///     (kategorija usluge, trajanje, cijena, frizer). Svaka usluga je predstavljena vektorom
    ///     atributa, a slicnost izmedju usluga racuna se Cosine Similarity mjerom.
    ///
    ///   - Popularity-Based Filtering: koristi se prvenstveno za nove klijente bez istorije
    ///     rezervacija (cold start). Kombinuje tri stvarna signala koja se upisuju i prate u bazi:
    ///     broj rezervacija usluge, prosjecnu ocjenu frizera koji je izvode (FrizerOcjena) i
    ///     ukupnu frekvenciju koristenja usluge (broj zaista odradjenih termina).
    ///
    /// Tezine: 70% Content-Based / 30% Popularity-Based za klijente sa istorijom rezervacija.
    /// Za nove klijente (cold start) omjer se obrce: 0% Content-Based / 100% Popularity-Based,
    /// jer content-based komponenta nema na cemu da racuna slicnost.
    /// </summary>
    public class RecommendationService : IRecommendationService
    {
        private const double TezinaContentPostojeciKorisnik = 0.7;
        private const double TezinaPopularityPostojeciKorisnik = 0.3;

        private const double TezinaContentNoviKorisnik = 0.0;
        private const double TezinaPopularityNoviKorisnik = 1.0;

        private readonly ECommerceDbContext _dbContext;
        private readonly MapsterMapper.IMapper _mapper;

        public RecommendationService(ECommerceDbContext dbContext, MapsterMapper.IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<UslugaPreporukaResponse>> PreporuciAsync(int klijentId, int brojPreporuka = 5)
        {
            var aktivneUsluge = await _dbContext.Usluge
                .Where(u => u.IsActive)
                .Include(u => u.UslugaKategorija)
                .Include(u => u.FrizerUsluge)
                .ToListAsync();

            if (aktivneUsluge.Count == 0)
                return new List<UslugaPreporukaResponse>();

            // Istorija klijenta: parovi (UslugaId, FrizerId) iz njegovih ranijih ne-otkazanih termina.
            // Ovo je "profil" klijenta za content-based dio - konkretna usluga I konkretan frizer koji ju je izveo.
            var istorijaTermina = await _dbContext.Termini
                .Where(t => t.KlijentId == klijentId && t.Status != TerminStatus.Otkazan)
                .Select(t => new { t.UslugaId, t.FrizerId })
                .ToListAsync();

            var istorijaUslugaIds = istorijaTermina.Select(t => t.UslugaId).Distinct().ToList();
            bool noviKorisnik = istorijaTermina.Count == 0;

            // ==================== Popularity-Based signali ====================

            // 1) Broj rezervacija po usluzi (sve ne-otkazane rezervacije - trenutna traznja)
            var brojRezervacijaPoUsluzi = await _dbContext.Termini
                .Where(t => t.Status != TerminStatus.Otkazan)
                .GroupBy(t => t.UslugaId)
                .Select(g => new { UslugaId = g.Key, Broj = g.Count() })
                .ToDictionaryAsync(x => x.UslugaId, x => x.Broj);

            // 2) Ukupna frekvencija koristenja usluge - koliko puta je usluga STVARNO odradjena kroz historiju
            var frekvencijaPoUsluzi = await _dbContext.Termini
                .Where(t => t.Status == TerminStatus.Odradjen)
                .GroupBy(t => t.UslugaId)
                .Select(g => new { UslugaId = g.Key, Broj = g.Count() })
                .ToDictionaryAsync(x => x.UslugaId, x => x.Broj);

            // 3) Prosjecna ocjena frizera koji mogu izvesti pojedinu uslugu
            var prosjecnaOcjenaPoFrizeru = await _dbContext.FrizerOcjene
                .GroupBy(o => o.FrizerId)
                .Select(g => new { FrizerId = g.Key, Prosjek = g.Average(o => o.Ocjena) })
                .ToDictionaryAsync(x => x.FrizerId, x => x.Prosjek);

            int maxBrojRezervacija = brojRezervacijaPoUsluzi.Count > 0 ? brojRezervacijaPoUsluzi.Values.Max() : 0;
            int maxFrekvencija = frekvencijaPoUsluzi.Count > 0 ? frekvencijaPoUsluzi.Values.Max() : 0;

            double ProsjecnaOcjenaZaUslugu(Usluga usluga)
            {
                var ocjene = usluga.FrizerUsluge
                    .Where(fu => prosjecnaOcjenaPoFrizeru.ContainsKey(fu.FrizerId))
                    .Select(fu => prosjecnaOcjenaPoFrizeru[fu.FrizerId])
                    .ToList();

                return ocjene.Count == 0 ? 0 : ocjene.Average();
            }

            double PopularityScore(Usluga usluga)
            {
                double brojRezervacijaNorm = maxBrojRezervacija == 0 ? 0
                    : brojRezervacijaPoUsluzi.TryGetValue(usluga.Id, out var br) ? (double)br / maxBrojRezervacija : 0;

                double frekvencijaNorm = maxFrekvencija == 0 ? 0
                    : frekvencijaPoUsluzi.TryGetValue(usluga.Id, out var fr) ? (double)fr / maxFrekvencija : 0;

                double ocjenaNorm = ProsjecnaOcjenaZaUslugu(usluga) / 5.0; // skala ocjene 1-5 -> 0-1

                return (brojRezervacijaNorm + frekvencijaNorm + ocjenaNorm) / 3.0;
            }

            // ==================== Content-Based (Cosine Similarity nad atributima) ====================
            // Vektor atributa usluge: kategorija (one-hot) + trajanje (normalizovano) + cijena (normalizovano)
            // + frizer (one-hot/multi-hot nad frizerima koji tu uslugu mogu izvesti).

            var kategorijeVokabular = aktivneUsluge
                .Where(u => u.UslugaKategorijaId.HasValue)
                .Select(u => u.UslugaKategorijaId!.Value)
                .Distinct()
                .OrderBy(id => id)
                .ToList();

            var frizeriVokabular = aktivneUsluge
                .SelectMany(u => u.FrizerUsluge.Select(fu => fu.FrizerId))
                .Distinct()
                .OrderBy(id => id)
                .ToList();

            double minTrajanje = aktivneUsluge.Min(u => u.TrajanjeMinuta);
            double maxTrajanje = aktivneUsluge.Max(u => u.TrajanjeMinuta);
            double minCijena = (double)aktivneUsluge.Min(u => u.Cijena);
            double maxCijena = (double)aktivneUsluge.Max(u => u.Cijena);

            double NormalizujTrajanje(int trajanje) =>
                maxTrajanje - minTrajanje < 0.0001 ? 0.5 : (trajanje - minTrajanje) / (maxTrajanje - minTrajanje);

            double NormalizujCijenu(decimal cijena) =>
                maxCijena - minCijena < 0.0001 ? 0.5 : ((double)cijena - minCijena) / (maxCijena - minCijena);

            // konkretniFrizerId - kada gradimo vektor za JEDAN termin iz istorije klijenta, znamo tacno
            // kog je frizera koristio. Za KANDIDAT uslugu (jos nije izabran frizer) koristimo sve
            // frizere koji tu uslugu mogu izvesti (iz FrizerUsluga).
            double[] VektorZaUslugu(Usluga usluga, int? konkretniFrizerId = null)
            {
                var vektor = new double[kategorijeVokabular.Count + 2 + frizeriVokabular.Count];
                int idx = 0;

                foreach (var katId in kategorijeVokabular)
                    vektor[idx++] = usluga.UslugaKategorijaId == katId ? 1.0 : 0.0;

                vektor[idx++] = NormalizujTrajanje(usluga.TrajanjeMinuta);
                vektor[idx++] = NormalizujCijenu(usluga.Cijena);

                var relevantniFrizeri = konkretniFrizerId.HasValue
                    ? new List<int> { konkretniFrizerId.Value }
                    : usluga.FrizerUsluge.Select(fu => fu.FrizerId).ToList();

                foreach (var frizId in frizeriVokabular)
                    vektor[idx++] = relevantniFrizeri.Contains(frizId) ? 1.0 : 0.0;

                return vektor;
            }

            var uslugeById = aktivneUsluge.ToDictionary(u => u.Id);

            // Profil klijenta = vektori njegovih PRETHODNIH (usluga, frizer) parova
            var istorijaVektori = istorijaTermina
                .Where(t => uslugeById.ContainsKey(t.UslugaId))
                .Select(t => VektorZaUslugu(uslugeById[t.UslugaId], t.FrizerId))
                .ToList();

            double ContentScore(Usluga usluga)
            {
                if (istorijaVektori.Count == 0) return 0;
                var kandidatVektor = VektorZaUslugu(usluga);
                return istorijaVektori.Average(v => CosinusSlicnost(kandidatVektor, v));
            }

            double tezinaContent = noviKorisnik ? TezinaContentNoviKorisnik : TezinaContentPostojeciKorisnik;
            double tezinaPopularity = noviKorisnik ? TezinaPopularityNoviKorisnik : TezinaPopularityPostojeciKorisnik;

            var kandidati = aktivneUsluge.Where(u => !istorijaUslugaIds.Contains(u.Id));

            var preporuke = kandidati
                .Select(u =>
                {
                    var content = ContentScore(u);
                    var popularity = PopularityScore(u);
                    var skor = tezinaContent * content + tezinaPopularity * popularity;

                    return new UslugaPreporukaResponse
                    {
                        Usluga = _mapper.Map<UslugaResponse>(u),
                        Skor = Math.Round(skor, 4),
                        ContentBasedSkor = Math.Round(content, 4),
                        PopularityBasedSkor = Math.Round(popularity, 4),
                        Objasnjenje = IzgradiObjasnjenje(noviKorisnik, content, popularity)
                    };
                })
                .OrderByDescending(p => p.Skor)
                .ThenByDescending(p => p.PopularityBasedSkor)
                .Take(brojPreporuka)
                .ToList();

            return preporuke;
        }

        private static string IzgradiObjasnjenje(bool noviKorisnik, double content, double popularity)
        {
            if (noviKorisnik)
                return popularity > 0
                    ? "Popularna usluga medju nasim klijentima."
                    : "Predlozeno za nove klijente.";

            if (content >= popularity)
                return "Na osnovu vasih prethodnih termina.";

            return "Popularna usluga medju klijentima sa slicnim navikama.";
        }

        private static double CosinusSlicnost(double[] a, double[] b)
        {
            double dot = 0, normA = 0, normB = 0;

            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }

            if (normA == 0 || normB == 0)
                return 0;

            return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }
    }
}
