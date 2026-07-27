# Sistem preporuke usluga

LuxSalon klijentima na početnom ekranu mobilne aplikacije prikazuje sekciju "Preporučeno za vas" —
listu usluga generisanu hibridnim algoritmom preporuke, implementiranim u
[`RecommendationService.cs`](eCommerce/LuxSalon.Services/RecommendationService.cs).

Endpoint: `GET /Recommendation?broj=5` ([`RecommendationController.cs`](eCommerce/LuxSalon.WebAPI/Controllers/RecommendationController.cs)).
`klijentId` se ne prima iz URL-a niti query stringa - uzima se isključivo iz JWT tokena prijavljenog
korisnika (sprječava da bilo ko pogodi tuđi Id i vidi tuđe personalizovane preporuke).

## Pregled

Algoritam kombinuje dva pristupa:

1. **Content-Based Filtering** — preporučuje usluge slične onima koje je klijent već rezervisao,
   na osnovu atributa usluge: kategorija, trajanje, cijena i frizer (cosine similarity).
2. **Popularity-Based Filtering** — koristi se prvenstveno za nove klijente bez istorije rezervacija
   (cold start problem), kombinujući broj rezervacija, prosječnu ocjenu frizera i ukupnu frekvenciju
   korištenja usluge.

Krajnji skor je ponderisana suma ova dva:

```
skor(usluga) = w_content * ContentScore(usluga) + w_popularity * PopularityScore(usluga)
```

## 1. Content-Based Filtering

Svaka usluga je predstavljena **vektorom atributa**:

- **kategorija** — one-hot enkodirana (1 za kategoriju kojoj usluga pripada, 0 za ostale)
- **trajanje** — `TrajanjeMinuta`, min-max normalizovano na [0, 1] u odnosu na sve aktivne usluge
- **cijena** — `Cijena`, min-max normalizovano na [0, 1] u odnosu na sve aktivne usluge
- **frizer** — one-hot/multi-hot nad frizerima koji tu uslugu mogu izvesti (tabela `FrizerUsluga`)

Za svaki termin iz klijentove istorije poznat je i **konkretan frizer** koji ga je uslužio, pa se za
taj dio istorije vektor gradi sa tačno tim frizerom (a ne sa svim frizerima koji uslugu nude). Za
kandidat-usluge (još nije izabran frizer) koristi se skup svih frizera koji tu uslugu mogu izvesti.

Sličnost dvije usluge se računa **kosinusnom sličnošću** njihovih vektora:

```
cos_sim(A, B) = (A · B) / (‖A‖ * ‖B‖)
```

Za klijenta se izgradi "profil" — vektori svih (usluga, frizer) parova koje je ranije rezervisao (bez
otkazanih). Za svaku kandidat-uslugu, `ContentScore` je **prosjek** kosinusne sličnosti sa svakim
zapisom iz istorije klijenta:

```
ContentScore(usluga) = prosjek( cos_sim(usluga, h) )  za svako h iz istorije klijenta
```

Usluge koje je klijent već rezervisao se isključuju iz kandidata.

## 2. Popularity-Based Filtering

Kombinuje tri stvarna signala koja se prate u bazi, svaki normalizovan na [0, 1] i uprosječen:

```
PopularityScore(usluga) = ( BrojRezervacijaNorm + FrekvencijaNorm + ProsjecnaOcjenaNorm ) / 3
```

- **Broj rezervacija** — koliko puta je usluga rezervisana (svi ne-otkazani termini), normalizovano
  u odnosu na najrezervisaniju uslugu u sistemu.
- **Frekvencija korištenja** — koliko puta je usluga stvarno **odrađena** (status `Odradjen`),
  normalizovano u odnosu na najčešće odrađivanu uslugu. Ovo je namjerno odvojeno od broja rezervacija
  jer rezervacija ne znači nužno i realizovan termin.
- **Prosječna ocjena frizera** — prosjek `FrizerOcjena.Ocjena` (skala 1-5) frizera koji tu uslugu
  mogu izvesti, normalizovan dijeljenjem sa 5.

## 3. Kombinovanje i cold start problem

Nov klijent (bez ijedne rezervacije) nema istoriju iz koje bi se izračunao `ContentScore`, pa je
Content-Based komponenta za njega u potpunosti isključena:

| Klijent | Content-Based | Popularity-Based |
|---|---|---|
| Ima istoriju rezervacija | 70% | 30% |
| Nov klijent (cold start) | 0% | 100% |

Za novog klijenta preporuka se u potpunosti zasniva na popularnosti usluga u salonu (broj rezervacija,
frekvencija i ocjene frizera), dok se za postojećeg klijenta favorizuje personalizacija na osnovu
onoga što je već probao.

## Rezultat

Za svaku preporučenu uslugu API vraća:

- `Skor` — finalni ponderisani skor
- `ContentBasedSkor`, `PopularityBasedSkor` — pojedinačni skorovi (korisno za debug/transparentnost)
- `Objasnjenje` — kratak tekst za korisnika (npr. "Na osnovu vaših prethodnih termina." ili
  "Popularna usluga među našim klijentima.")

Lista se sortira opadajuće po `Skor`, a kao tie-breaker po `PopularityBasedSkor`, i vraća se
`broj` (default 5) top usluga.

## Primjer

Klijent je ranije rezervisao "Žensko šišanje" kod frizera Amele, u kategoriji "Šišanje" (30 min,
20 KM). Kandidat usluga "Farbanje kose" pripada drugoj kategoriji, ali je duže i skuplje, a nudi je
i Amela — dijeljenje frizera i blizina po trajanju/cijeni daju umjerenu cosine sličnost. Ako je
"Farbanje kose" pritom i često rezervisana usluga sa visoko ocijenjenim frizerima, njen
`PopularityScore` dodatno podiže ukupni skor, pa se vjerovatno nađe visoko na listi preporuka.
