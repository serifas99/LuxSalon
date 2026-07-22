# LuxSalon

Sistem za zakazivanje termina u frizerskom salonu — seminarski rad iz predmeta **Razvoj softvera II**.

Projekat se sastoji od tri aplikacije:

- **Backend** (`eCommerce.WebAPI`) — ASP.NET Core 9 REST API
- **Desktop aplikacija** (`ecommerce_desktop`) — Flutter, za administratore i frizere
- **Mobilna aplikacija** (`ecommerce_mobile`) — Flutter, za klijente

## Funkcionalnosti

- Registracija i prijava korisnika (JWT autentifikacija), uloge: Admin, Frizer, Klijent
- CRUD nad uslugama, kategorijama usluga, frizerima, korisnicima
- Zakazivanje termina sa provjerom preklapanja termina po frizeru
- Status termina kroz stanja: Zakazan → Potvrđen → Odrađen / Otkazan / Nije se odazvao
- **Sistem preporuke usluga** (hibridni Content-Based + Popularity-Based) — vidi [docs/RECOMMENDER.md](docs/RECOMMENDER.md)
- Plaćanje termina preko **PayPal** (sandbox)
- Email notifikacije preko **RabbitMQ** + worker servisa (`eCommerce.Subscriber`)
- **Notifikacije uživo** preko SignalR (desktop i mobile, bez ručnog refresha)
- PDF izvještaji (desktop aplikacija)

## Arhitektura / projekti u rješenju

| Projekat | Opis |
|---|---|
| `eCommerce.WebAPI` | REST API, kontroleri, SignalR hub, autentifikacija |
| `eCommerce.Services` | Poslovna logika, EF Core entiteti i migracije, validatori |
| `eCommerce.Model` | DTO-ovi (Request/Response/SearchObject) |
| `eCommerce.Common.Services` | Zajednički servisi (RabbitMQ publisher, PayPal klijent, SignalR notifier apstrakcija) |
| `eCommerce.Subscriber` | Worker servis — sluša RabbitMQ i šalje email notifikacije (preko MailHog-a u razvoju) |
| `UI/ecommerce_desktop` | Flutter desktop app za admin/frizere |
| `UI/ecommerce_mobile` | Flutter mobilna app za klijente |

## Tehnologije

ASP.NET Core 9, Entity Framework Core 9, MS SQL Server, Mapster, FluentValidation, JWT, SignalR, RabbitMQ, PayPal REST API, Flutter (Provider, flutter_form_builder, signalr_netcore).

## Pokretanje projekta

Potrebno je pokrenuti nekoliko servisa odvojeno.

### 1. Infrastruktura (baza, RabbitMQ, mail)

```
cd eCommerce
docker-compose up
```

Ovo pokreće:
- MS SQL Server na portu `1435`
- RabbitMQ na portu `5672` (management UI: http://localhost:15672, guest/guest)
- MailHog (lažni mail server za razvoj) na portu `1025`, web UI: http://localhost:8025

### 2. Backend API

```
cd eCommerce/eCommerce.WebAPI
dotnet run
```

API je dostupan na `http://localhost:5126`. Baza se automatski kreira i puni test podacima (seed) pri prvom pokretanju.

### 3. Worker servis (email notifikacije)

```
cd eCommerce/eCommerce.Subscriber
dotnet run
```

### 4. Desktop aplikacija

```
cd eCommerce/UI/ecommerce_desktop
flutter pub get
flutter run -d windows
```

### 5. Mobilna aplikacija

```
cd eCommerce/UI/ecommerce_mobile
flutter pub get
flutter run
```

(Ako nema Android emulatora, može i `flutter run -d web-server --dart-define=BASE_URL=http://localhost:5126/Access --dart-define=baseUrl=http://localhost:5126/`)

## Test korisnici

Lozinka za sve korisnike je `Test123` (frizeri: `Test123!`).

| Uloga | Username |
|---|---|
| Admin | admin1, admin2, admin3 |
| Frizer | frizer1, frizer2 |
| Klijent | customer1, customer2 |

## Sistem preporuke

Detaljno objašnjenje algoritma (Content-Based + Popularity-Based, cold start rješenje za nove klijente) nalazi se u [docs/RECOMMENDER.md](docs/RECOMMENDER.md).
