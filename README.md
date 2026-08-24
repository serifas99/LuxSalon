# LuxSalon

Sistem za zakazivanje termina u frizerskom salonu — seminarski rad iz predmeta **Razvoj softvera II**.

Projekat se sastoji od tri aplikacije:

- **Backend** (`LuxSalon.WebAPI`) — ASP.NET Core 9 REST API
- **Desktop aplikacija** (`ecommerce_desktop`) — Flutter, za administratore i frizere
- **Mobilna aplikacija** (`ecommerce_mobile`) — Flutter, za klijente

## Funkcionalnosti

- Registracija i prijava korisnika (JWT autentifikacija), uloge: Admin, Frizer, Klijent
- CRUD nad uslugama, kategorijama usluga, frizerima, korisnicima
- Zakazivanje termina sa provjerom preklapanja termina po frizeru
- Status termina kroz stanja: Zakazan → Potvrđen → Odrađen / Otkazan / Nije se odazvao
- **Sistem preporuke usluga** (hibridni Content-Based + Popularity-Based) — vidi [recommender-dokumentacija.md](recommender-dokumentacija.md)
- Plaćanje termina preko **PayPal** (sandbox)
- Email notifikacije preko **RabbitMQ** + worker servisa (`LuxSalon.Subscriber`)
- **Notifikacije uživo** preko SignalR (desktop i mobile, bez ručnog refresha)
- PDF izvještaji (desktop aplikacija)

## Arhitektura / projekti u rješenju

| Projekat | Opis |
|---|---|
| `LuxSalon.WebAPI` | REST API, kontroleri, SignalR hub, autentifikacija |
| `LuxSalon.Services` | Poslovna logika, EF Core entiteti i migracije, validatori |
| `LuxSalon.Model` | DTO-ovi (Request/Response/SearchObject) |
| `LuxSalon.Common.Services` | Zajednički servisi (RabbitMQ publisher, PayPal klijent, SignalR notifier apstrakcija) |
| `LuxSalon.Subscriber` | Worker servis — sluša RabbitMQ i šalje email notifikacije (preko MailHog-a u razvoju) |
| `UI/ecommerce_desktop` | Flutter desktop app za admin/frizere |
| `UI/ecommerce_mobile` | Flutter mobilna app za klijente |

## Tehnologije

ASP.NET Core 9, Entity Framework Core 9, MS SQL Server, Mapster, FluentValidation, JWT, SignalR, RabbitMQ, PayPal REST API, Flutter (Provider, flutter_form_builder, signalr_netcore).

## Pokretanje projekta

Potrebno je pokrenuti nekoliko servisa odvojeno.

### 0. Konfiguracija (.env)

Tajne (connection string, JWT secret, RabbitMQ i PayPal kredencijali) se ne nalaze u `appsettings.json` nego u `.env` fajlovima koji se ne komituju u git. Za svaki `.env.example` napravi kopiju bez `.example` nastavka i popuni stvarnim vrijednostima:

```
eCommerce/.env.example                  → eCommerce/.env                  (SA_PASSWORD za SQL Server kontejner)
eCommerce/LuxSalon.WebAPI/.env.example → eCommerce/LuxSalon.WebAPI/.env
eCommerce/LuxSalon.Subscriber/.env.example → eCommerce/LuxSalon.Subscriber/.env
```

`SA_PASSWORD` u `eCommerce/.env` mora biti ista vrijednost kao lozinka u `ConnectionStrings__DefaultConnection` unutar `LuxSalon.WebAPI/.env`.

### 1. Infrastruktura (baza, RabbitMQ, mail)

```
cd eCommerce
docker-compose up
```

Ovo pokreće:
- MS SQL Server na portu `1435`
- RabbitMQ na portu `5672` (management UI: http://localhost:15672, guest/guest)
- MailHog (lažni mail server za razvoj) na portu `1025`, web UI: http://localhost:8025
- **Backend API** (`luxsalon-webapi`) na portu `5126` (mikroservis, vlastiti `Dockerfile`)
- **Worker servis** (`luxsalon-subscriber`) - zaseban kontejner, sluša RabbitMQ i šalje email obavještenja preko MailHog-a

Svih 5 servisa je definisano u `docker-compose.yml`, svaki u svom kontejneru (prava mikroservisna arhitektura - API i worker nisu in-process pozadinski taskovi).

Za pokretanje samo infrastrukture (baza/RabbitMQ/mail), bez rebuilda API-ja i workera pri svakoj izmjeni koda tokom razvoja, koristi:

```
docker-compose up ecomm-fit-2026 luxsalon-rabbitmq luxsalon-mailhog
```

pa API i worker pokreni ručno (koraci 2 i 3 ispod) - brže je za iterativni razvoj jer ne zahtijeva rebuild Docker image-a pri svakoj promjeni.

### 2. Backend API

Ako nisi pokrenula `luxsalon-webapi` kroz `docker-compose up`, pokreni ga ručno:

```
cd eCommerce/LuxSalon.WebAPI
dotnet run
```

API je dostupan na `http://localhost:5126`. Baza se automatski kreira i puni test podacima (seed) pri prvom pokretanju.

### 3. Worker servis (email notifikacije)

Ako nisi pokrenula `luxsalon-subscriber` kroz `docker-compose up`, pokreni ga ručno:

```
cd eCommerce/LuxSalon.Subscriber
dotnet run
```

### 4. Desktop aplikacija

```
cd eCommerce/UI/ecommerce_desktop
flutter pub get
flutter run -d windows --dart-define=API_BASE_URL=http://localhost:5126
```

### 5. Mobilna aplikacija

```
cd eCommerce/UI/ecommerce_mobile
flutter pub get
flutter run --dart-define=API_BASE_URL=http://10.0.2.2:5126
```


## Test korisnici

Lozinka za sve korisnike je `Test123` (frizeri: `Test123!`).

| Uloga | Username |
|---|---|
| Admin | admin1, admin2, admin3 |
| Frizer | frizer1, frizer2 |
| Klijent | customer1, customer2 |

## Sistem preporuke

Detaljno objašnjenje algoritma (Content-Based + Popularity-Based, cold start rješenje za nove klijente) nalazi se u [recommender-dokumentacija.md](recommender-dokumentacija.md).
