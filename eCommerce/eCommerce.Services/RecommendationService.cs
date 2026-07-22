using eCommerce.Model.Responses;
using eCommerce.Services.Database;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Services
{
    /// <summary>
    /// Hibridni sistem preporuke usluga:
    ///   - Content-Based Filtering (Cosine Similarity nad tagovima usluga)
    ///   - Popularity-Based Filtering (normalizovan broj rezervacija po usluzi)
    ///
    /// Tezine: 70% Content-Based / 30% Popularity-Based za klijente sa istorijom rezervacija.
    /// Za nove klijente bez istorije (cold start) tezine su OBRNUTE: 30% Content-Based / 70% Popularity-Based,
    /// jer content-based komponenta nema na cemu da racuna slicnost.
    /// </summary>
    public class RecommendationService : IRecommendationService
    {
        private const double TezinaContentPostojeciKorisnik = 0.7;
        private const double TezinaPopularityPostojeciKorisnik = 0.3;

        private const double TezinaContentNoviKorisnik = 0.3;
        private const double TezinaPopularityNoviKorisnik = 0.7;

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
                .ToListAsync();

            if (aktivneUsluge.Count == 0)
                return new List<UslugaPreporukaResponse>();

            // Usluge koje je klijent vec rezervisao (bilo kada, osim otkazanih) - iskljucujemo ih iz preporuka
            // i koristimo kao "profil" klijenta za content-based dio.
            var istorijaUslugaIds = await _dbContext.Termini
                .Where(t => t.KlijentId == klijentId && t.Status != TerminStatus.Otkazan)
                .Select(t => t.UslugaId)
                .Distinct()
                .ToListAsync();

            bool noviKorisnik = istorijaUslugaIds.Count == 0;

            // --- Popularity-Based ---
            var brojRezervacijaPoUsluzi = await _dbContext.Termini
                .Where(t => t.Status != TerminStatus.Otkazan)
                .GroupBy(t => t.UslugaId)
                .Select(g => new { UslugaId = g.Key, Broj = g.Count() })
                .ToListAsync();

            int maxBroj = brojRezervacijaPoUsluzi.Any() ? brojRezervacijaPoUsluzi.Max(x => x.Broj) : 0;

            double PopularityScore(int uslugaId)
            {
                if (maxBroj == 0) return 0;
                var zapis = brojRezervacijaPoUsluzi.FirstOrDefault(x => x.UslugaId == uslugaId);
                return zapis == null ? 0 : (double)zapis.Broj / maxBroj;
            }

            // --- Content-Based (Cosine Similarity nad tagovima) ---
            var vokabular = aktivneUsluge
                .SelectMany(u => ParsirajTagove(u.Tagovi))
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            var vektori = aktivneUsluge.ToDictionary(u => u.Id, u => IzgradiVektor(u.Tagovi, vokabular));

            var istorijaVektori = istorijaUslugaIds
                .Where(id => vektori.ContainsKey(id))
                .Select(id => vektori[id])
                .ToList();

            double ContentScore(int uslugaId)
            {
                if (istorijaVektori.Count == 0) return 0;
                var kandidatVektor = vektori[uslugaId];
                return istorijaVektori.Average(v => CosinusSlicnost(kandidatVektor, v));
            }

            double tezinaContent = noviKorisnik ? TezinaContentNoviKorisnik : TezinaContentPostojeciKorisnik;
            double tezinaPopularity = noviKorisnik ? TezinaPopularityNoviKorisnik : TezinaPopularityPostojeciKorisnik;

            var kandidati = aktivneUsluge.Where(u => !istorijaUslugaIds.Contains(u.Id));

            var preporuke = kandidati
                .Select(u =>
                {
                    var content = ContentScore(u.Id);
                    var popularity = PopularityScore(u.Id);
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
                return "Slicno uslugama koje ste vec rezervisali.";

            return "Popularna usluga medju klijentima sa slicnim navikama.";
        }

        private static HashSet<string> ParsirajTagove(string tagovi)
        {
            if (string.IsNullOrWhiteSpace(tagovi))
                return new HashSet<string>();

            return tagovi
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => t.ToLowerInvariant())
                .ToHashSet();
        }

        private static double[] IzgradiVektor(string tagovi, List<string> vokabular)
        {
            var tagSet = ParsirajTagove(tagovi);
            var vektor = new double[vokabular.Count];

            for (int i = 0; i < vokabular.Count; i++)
            {
                vektor[i] = tagSet.Contains(vokabular[i]) ? 1.0 : 0.0;
            }

            return vektor;
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
