# AED – prototyp (apps)

Prototyp mechanizmu na `https://aed.hacknation-2026.pl`. Mapa i wyszukiwarka AED, panel dysponenta, import z walidacją i deduplikacją oraz eksport otwartych danych (CSV, GeoJSON) z metadanymi dla dane.gov.pl. Wzór układu: `C:\car-reminder\apps`.

> Status: działa:
> - API: baza PostgreSQL + PostGIS, model danych AED, logowanie z rolami, dane demo;
> - publiczna mapa AED: wyszukiwarka, filtry, „Najbliższe AED”, karta punktu, PL/EN, tryb ciemny;
> - otwarte dane dla dane.gov.pl: CSV, GeoJSON i JSON pod stałymi adresami, metadane, katalog DCAT-AP-PL, wzorcowy zakres danych XLSX, strona „Otwarte dane”.
>
> Panel dysponenta i import powstają w kolejnych krokach.

## Otwarte dane (dane.gov.pl)

Opis pól, słowników i zasad publikacji: [`docs/schemat-danych.md`](docs/schemat-danych.md). Strona w aplikacji: http://localhost:3060/pl/open-data.

| Plik (web: `/api/open-data/…`, API: `/api/v1/OpenData/…`) | Co zawiera |
|---|---|
| `aed.csv`, `aed.geojson`, `aed.json` | Cały zbiór, generowany przy każdym pobraniu. Filtry: `?wojewodztwo=02`, `?zrodlo=<id>` |
| `metadata.json` | Metadane zbioru: tytuł, opis, słowa kluczowe, kategoria „Zdrowie”, licencja, częstotliwość, zasoby, źródła z zakresem referencyjnym, pola, słowniki |
| `catalog.rdf` | Katalog DCAT-AP-PL 2.1 (RDF/XML): Catalog, Dataset i Distribution, słowniki UE (file-type, data-theme, frequency, language), licencje CC |
| `wzorcowy-zakres-danych-aed.xlsx` | Wzorzec zakresu danych: pola, słowniki, przykład, zasady (tylko dokumentacja, dane publikujemy w CSV, GeoJSON i JSON) |

Pliki są też w repo:
- [`data/wzorcowy-zakres-danych-aed.xlsx`](data/wzorcowy-zakres-danych-aed.xlsx): wymagany w zgłoszeniu;
- przykłady `data/przyklad-*` wygenerowane z danych demo.

Adres w metadanych i katalogu ustawia `OpenData:PublicBaseUrl`.

## Jak uruchomić

Wymagania: .NET SDK 10, Node.js 22, Docker.

Najprościej przez Aspire. AppHost sam uruchamia `docker compose`, API i web:

```powershell
cd apps
npm install --prefix web                               # pierwszy raz
dotnet run --project api/Aed/AppHost
```

Osobno:

```powershell
cd apps
docker compose up -d                                   # PostgreSQL 16 + PostGIS na porcie 5433
dotnet run --project api/Aed/Api --launch-profile http # API na 5296
cd web; npm run dev                                    # web na 3060 (API_URL domyślnie http://localhost:5296)
```

Przy starcie API stosuje migracje i uruchamia seedery. `GET /health` zwraca 200, gdy skończą.

| Co | Adres |
|---|---|
| Mapa AED (web) | http://localhost:3060 |
| API | http://localhost:5296 |
| Dokumentacja API (Scalar) | http://localhost:5296/scalar |
| Dashboard Aspire | http://localhost:15122 |
| PostgreSQL | `localhost:5433`, baza `aed`, `postgres` / `postgres` |

Porty nie kolidują z Car Reminder (5432, 5196, 3050).

### Publiczne API mapy (bez logowania)

| Endpoint | Co zwraca |
|---|---|
| `GET /api/v1/PublicAed/map?minLat&minLng&maxLat&maxLng` (+ `availability`, `status`, `facilityType`, `openNow`) | Punkty w widocznym obszarze mapy, do 2000 |
| `GET /api/v1/PublicAed/nearest?latitude&longitude&limit&radiusKm&openNow` | Najbliższe AED z odległością w metrach (PostGIS `ST_DWithin` / `ST_Distance`) |
| `GET /api/v1/PublicAed/search?q=` | Wyszukiwanie po miejscowości, gminie, adresie, nazwie miejsca |
| `GET /api/v1/PublicAed/{id}` | Karta AED ze źródłem, licencją, zakresem referencyjnym i datą weryfikacji |

„Dostępne teraz” liczy się z godzin w formacie OpenStreetMap `opening_hours` (np. `Mo-Fr 08:00-16:00; Sa 10:00-15:00`, `24/7`, `Jun-Aug 09:00-19:00`), w czasie polskim. Gdy format jest nieznany, API zwraca „brak danych”, nigdy fałszywe „otwarte”.

### Web (`web/`)

Szkielet z Car Reminder: Next.js 16, Tailwind 4, next-intl, CSP z nonce w `src/proxy.ts` i wybrane komponenty kitu Eyris w `src/components/ui`. Teksty są w `web/messages/<przestrzeń>/<pl|en>.json`, a test `npm run test:unit` pilnuje `src/i18n/clientNamespaces.ts`.

Mapa to **Leaflet** (`react-leaflet` i `leaflet.markercluster`) z kafelkami OpenStreetMap: bez klucza API i bez opłat. Przeglądarka rozmawia tylko z własnym adresem (CSP `connect-src 'self'`), a `src/app/api/aed/[...path]/route.ts` przekazuje wywołania do publicznych endpointów API.

Dostępność (WCAG 2.2 AA):
- kształt pinezki pokazuje dostępność bez kolorów: koło to 24/7, kwadrat to godziny otwarcia, romb to dostęp ograniczony;
- lista wyników działa bez mapy;
- pinezki obsługuje się klawiaturą;
- jest skip-link i `aria-live` z liczbą wyników.

Komendy: `npm run lint`, `npm run build`, `npm run test:unit`.

### Konta demo (Development)

| E-mail | Hasło | Rola |
|---|---|---|
| `admin@aed.local` | `AedAdmin2026` | Admin: wszystkie dane, weryfikacja |
| `wroclaw@aed.local` | `AedDemo2026x` | Dysponent: Urząd Miejski Wrocławia (demo) |
| `osp@aed.local` | `AedDemo2026x` | Dysponent: OSP Gminy Przykładowej (demo) |

Hasła są w `api/Aed/Api/appsettings.Development.json`. Na produkcji konta powstają tylko wtedy, gdy hasła są podane w zmiennych środowiskowych (`Seed__AdminPassword`, `Seed__DemoPassword`).

Logowanie: `POST /api/v1/Auth/login` z `{ "email", "password" }` zwraca token JWT, który wysyła się jako `Authorization: Bearer …`. `GET /api/v1/Auth/me` zwraca role i dysponenta.

### Dane demo

3 dysponentów, 3 źródła (CC0 i CC BY 4.0, każde z zakresem referencyjnym) i 38 **fikcyjnych** AED we Wrocławiu, okolicach, Warszawie, Krakowie, Gdańsku i Poznaniu. Rekordy mają wszystkie rodzaje dostępności i statusy. Źródło „Społeczna mapa AED” powtarza dwa urządzenia z rejestru Wrocławia kilka metrów obok, pod innymi nazwami, żeby było co wykryć przy deduplikacji. To nie jest rejestr prawdziwych urządzeń.

### Testy

```powershell
dotnet test api/Aed/Application.Test
```

## Ograniczenia (świadome uproszczenia prototypu)

- Uprawnienia przez role (`Admin`, `Dispatcher`) w `[Authorize(Roles = …)]`. Car Reminder ma tabele uprawnień do modułów, tu ich nie ma.
- Brak refresh tokenów: token dostępu ważny 12 h.
- Brak e-maili (rejestracja, reset hasła). Konta zakłada seeder lub administrator.
- Katalog DCAT-AP-PL jest zgodny z profilem 2.1 (klasy i słowniki ze specyfikacji), ale nie był sprawdzany harwesterem dane.gov.pl. Podpięcie go do portalu wymaga zgłoszenia u administratora portalu.
- Brak pola `dcatappl:opennessScore`, bo specyfikacja nie podaje jego wartości.

## Stos

| Warstwa | Technologia |
|---|---|
| **API** | ASP.NET Core, EF Core, PostgreSQL + PostGIS, Scalar |
| **Web** | Next.js (szablon Eyris), next-intl (PL/EN), mapa Leaflet + OpenStreetMap |
| **Lokalnie** | .NET Aspire AppHost + `docker compose` (PostgreSQL) |
| **Produkcja** | Docker Compose za Traefikiem na VPS OVH (`../deploy/`) |

## Układ

```
apps/
├── api/Aed/                # Solution: Api, Application, Domain, Infrastructure, SharedKernel, ServiceDefaults, AppHost, Application.Test
├── docker-compose.yml      # Lokalna baza: PostgreSQL 16 + PostGIS (port 5433)
├── web/                    # Next.js: mapa, wyszukiwarka, karta AED, panel dysponenta
├── data/                   # Przykładowe CSV/GeoJSON, wzorcowy zakres danych (XLSX), słowniki
├── docker/postgres/init/   # Init lokalnej bazy (rozszerzenie PostGIS)
├── e2e/                    # Testy Playwright na działającej aplikacji
├── tools/                  # build-release.ps1 (obrazy na VPS)
├── docs/                   # Dokumentacja techniczna (schemat danych, walidacja, API)
└── azure-pipelines.yml     # Build obrazów api/web (artefakt release dla ../deploy)
```

## Zasady (z Car Reminder)

- Błędy walidacji pokazujemy pod polem, którego dotyczą. Dotyczy to błędów z formularza i z API (`error.fields`).
- Każde pole tekstowe ma `maxLength` równy `MaximumLength` walidatora API.
- Dane udostępniamy tylko w formatach maszynowych (CSV, JSON, GeoJSON). XLSX służy wyłącznie jako wzorzec zakresu danych.
- Interfejs spełnia WCAG 2.2 AA i działa na komputerach i telefonach.
