# Sistem preporuke usluga

LuxSalon klijentima na početnom ekranu mobilne aplikacije prikazuje sekciju "Preporučeno za vas" —
listu usluga generisanu hibridnim algoritmom preporuke, implementiranim u
[`RecommendationService.cs`](../eCommerce/eCommerce.Services/RecommendationService.cs).

Endpoint: `GET /Recommendation/{klijentId}?broj=5` ([`RecommendationController.cs`](../eCommerce/eCommerce.WebAPI/Controllers/RecommendationController.cs)).

## Pregled

Algoritam kombinuje dva pristupa:

1. **Content-Based Filtering** — preporučuje usluge slične onima koje je klijent već rezervisao,
   na osnovu tagova usluge (cosine similarity).
2. **Popularity-Based Filtering** — preporučuje usluge koje su generalno najčešće rezervisane
   među svim klijentima.

Krajnji skor je ponderisana suma ova dva:

```
skor(usluga) = w_content * ContentScore(usluga) + w_popularity * PopularityScore(usluga)
```

## 1. Content-Based Filtering

Svaka usluga ima polje `Tagovi` (npr. `"kosa,farbanje,zensko"`) — kratke ključne riječi koje opisuju
uslugu. Iz svih aktivnih usluga se izgradi **vokabular** (skup svih jedinstvenih tagova u sistemu),
a zatim se svaka usluga predstavi kao **binarni vektor** dužine vokabulara (1 ako usluga ima taj tag,
0 ako nema) — ovo je pojednostavljena varijanta bag-of-words / one-hot enkodinga.

Sličnost dvije usluge se računa **kosinusnom sličnošću** njihovih vektora:

```
cos_sim(A, B) = (A · B) / (‖A‖ * ‖B‖)
```

gdje je `A · B` skalarni proizvod, a `‖A‖`, `‖B‖` euklidske norme vektora. Rezultat je broj između
0 (nimalo zajedničkih tagova) i 1 (identičan skup tagova).

Za klijenta se izgradi "profil" — vektori svih usluga koje je ranije rezervisao (bez otkazanih). Za
svaku kandidat-uslugu, `ContentScore` je **prosjek** kosinusne sličnosti sa svakom uslugom iz istorije
klijenta:

```
ContentScore(usluga) = prosjek( cos_sim(usluga, h) )  za svako h iz istorije klijenta
```

Usluge koje je klijent već rezervisao se isključuju iz kandidata (nema smisla preporučiti nešto što
je već probao).

## 2. Popularity-Based Filtering

Za svaku uslugu se prebroji koliko puta je rezervisana (preko svih klijenata, isključujući otkazane
termine), a zatim se normalizuje u odnosu na najpopularniju uslugu u sistemu:

```
PopularityScore(usluga) = broj_rezervacija(usluga) / max(broj_rezervacija(sve usluge))
```

Rezultat je broj između 0 i 1, gdje 1 znači "najpopularnija usluga u salonu".

## 3. Kombinovanje i cold start problem

Nov klijent (bez ijedne rezervacije) nema istoriju iz koje bi se izračunao `ContentScore` — cosine
similarity nema šta da poredi, pa bi taj dio uvijek vraćao 0. Zbog toga su težine **različite** za
nove i postojeće klijente:

| Klijent | Content-Based | Popularity-Based |
|---|---|---|
| Ima istoriju rezervacija | 70% | 30% |
| Nov klijent (cold start) | 30% | 70% |

Za novog klijenta preporuka se u praksi svodi skoro isključivo na najpopularnije usluge u salonu
(sigurna, generička preporuka), dok se za postojećeg klijenta favorizuje personalizacija na osnovu
onoga što je već probao.

## Rezultat

Za svaku preporučenu uslugu API vraća:

- `Skor` — finalni ponderisani skor
- `ContentBasedSkor`, `PopularityBasedSkor` — pojedinačni skorovi (korisno za debug/transparentnost)
- `Objasnjenje` — kratak tekst za korisnika (npr. "Slično uslugama koje ste već rezervisali." ili
  "Popularna usluga među našim klijentima.")

Lista se sortira opadajuće po `Skor`, a kao tie-breaker po `PopularityBasedSkor`, i vraća se
`broj` (default 5) top usluga.

## Primjer

Klijent je ranije rezervisao "Žensko šišanje" (tagovi: `kosa,sisanje,zensko`). Vokabular sadrži i tag
"farbanje". Usluga "Farbanje kose" (tagovi: `kosa,farbanje`) dijeli tag `kosa` sa istorijom klijenta →
manja, ali nenulta cosine sličnost → dobija umjeren `ContentScore`. Ako je "Farbanje kose" pritom i
često rezervisana usluga u salonu, njen `PopularityScore` dodatno podiže ukupni skor, pa se vjerovatno
nađe visoko na listi preporuka.
