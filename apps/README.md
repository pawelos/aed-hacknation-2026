# AED – prototyp (apps)

Prototyp mechanizmu na `https://aed.hacknation-2026.pl`. Mapa i wyszukiwarka AED, panel dysponenta, import z walidacją i deduplikacją oraz eksport otwartych danych (CSV, GeoJSON) z metadanymi dla dane.gov.pl. Wzór układu: `C:\car-reminder\apps`.

> Status: działa:
> - API: baza PostgreSQL + PostGIS, model danych AED, logowanie z rolami, dane demo;
> - publiczna mapa AED: wyszukiwarka, filtry, „Najbliższe AED”, karta punktu, PL/EN, tryb ciemny;
> - otwarte dane dla dane.gov.pl: CSV, GeoJSON i JSON pod stałymi adresami, metadane, katalog DCAT-AP-PL, wzorcowy zakres danych XLSX, strona „Otwarte dane”;
> - import od dysponentów: CSV i GeoJSON z pliku albo z adresu URL, walidacja wierszy, aktualizacja po identyfikatorze dysponenta, wykrywanie duplikatów z innych źródeł, raport i historia;
> - panel dysponenta w web: logowanie, pulpit, lista i edycja rekordów, potwierdzanie aktualności, import z raportem, zgłoszenia poprawek z mapy.

## Panel dysponenta

Wejście: pozycja **Panel** w menu aplikacji albo przycisk „Zaloguj” na wizytówce. Obie prowadzą do `/pl/sign-in`, a po zalogowaniu do pulpitu.

**Szybkie wypełnienie**, jak w Car Reminder: nad formularzem logowania przełącznik „Dysponent – Wrocław”, „Dysponent – OSP”, „Administrator” wpisuje dane konta demo, wystarczy kliknąć „Zaloguj”.
- **Kiedy widoczny:** w Development zawsze. Na serwerze przy `DEMO_LOGIN=true` (domyślnie, `deploy/stacks/aed.env.example`).
- **Skąd hasła:** ze zmiennych środowiskowych (`DEMO_ADMIN_PASSWORD`, `DEMO_PASSWORD`, na VPS te same co `SEED_*`). Gdy `aed.env` ich nie podaje, `deploy/stacks/app.yml` przyjmuje publiczne hasła demo z Development, więc produkcja ma konta i szybkie wypełnienie od razu po wdrożeniu. Seeder ustawia istniejącemu kontu hasło z konfiguracji.
- **Po hackathonie:** `DEMO_LOGIN=false` i własne hasła w `aed.env`. Dopóki przełącznik działa, każdy może jako Administrator zmienić dane demo.
- **Ograniczenie:** przy włączonym przełączniku hasła kont demo są widoczne dla każdego na stronie logowania. To celowe w prototypie z fikcyjnymi danymi.

Konta demo są w tabeli niżej.

| Ekran | Co robi |
|---|---|
| **Pulpit** (`/pl/panel`) | Liczba rekordów, rekordy do weryfikacji, nowe zgłoszenia, weryfikacje starsze niż 12 miesięcy, ostatni import, źródła danych z zakresem referencyjnym i licencją |
| **Rekordy AED** (`/pl/panel/aeds`) | Lista jak w Car Reminder: wyszukiwanie, filtr źródła i „do weryfikacji”, zaznaczanie rekordów z paskiem akcji na dole. Dodawanie i edycja w szufladzie z prawej (opis niżej) |
| **Import** (`/pl/panel/import`) | Wybór źródła i pliku. „Sprawdź plik” pokazuje raport bez zapisu, „Importuj” zapisuje. Poniżej historia importów |
| **Zgłoszenia** (`/pl/panel/reports`) | Zgłoszenia z karty AED na mapie. Przyjęcie oznacza rekord do weryfikacji, a jego edycja lub potwierdzenie zdejmuje oznaczenie |

Rekordy AED, wzorem modułu „Pojazdy” w Car Reminder:
- **Adres:** filtry, strona i otwarty rekord są w adresie, więc każdy widok można odświeżyć lub wysłać linkiem. `?id=<id>` otwiera rekord w szufladzie, `?new=1` nowy rekord. Stare adresy `/panel/aeds/new` i `/panel/aeds/<id>` przekierowują do szuflady.
- **Zaznaczanie:** po zaznaczeniu rekordów na dole pojawia się pasek „Wybrano N rekordów” z przyciskami Edytuj (przy jednym rekordzie), Potwierdź aktualność i Usuń. Usunięcie wymaga potwierdzenia w oknie. Każda akcja to jedno zapytanie do API (`POST /api/v1/AedDevice/batch-delete` i `batch-verify`, 1–100 rekordów) i działa na zasadzie „wszystko albo nic”: gdy któregoś rekordu już nie ma, nic się nie zmienia, komunikat podaje, ilu nie znaleziono, a zaznaczenie zostaje do ponownej próby.
- **Szuflada:** tytuł „Edytuj defibrylator: …” z przyciskiem pełnego ekranu, pola w sekcjach (Lokalizacja, Miejsce urządzenia, Dostępność, Kontakt i stan). Stopka: Usuń i Potwierdź aktualność po lewej, Aktualizuj (zapisuje i zostaje) i Aktualizuj i wróć po prawej. Przy nowym rekordzie: Anuluj, Utwórz, Utwórz i wróć. Po „Utwórz” szuflada przechodzi w edycję nowego rekordu. Zamknięcie z niezapisanymi zmianami pyta o potwierdzenie.
- **Listy wyboru:** źródło danych, województwo (z kodem TERYT), rodzaj miejsca, dostępność i stan to pole z lupą, które otwiera okno z wyszukiwarką i tabelą. Tak samo działa filtr źródła nad listą, źródło w imporcie i filtr „Rodzaj miejsca” na mapie (wspólny komponent `src/components/shared/LookupSelect.tsx`).
- **Godziny dostępności:** zapis jak w OpenStreetMap (`opening_hours`, angielskie skróty dni i miesięcy). Pod polem i na karcie AED na mapie widać odczyt w języku strony, np. `Jun-Aug 08:00-22:00` → „cze–sie 08:00–22:00” (`src/utils/openingHours.ts`).
- **Walidacja:** pola wymagane mają gwiazdkę, a nad formularzem jest legenda. Błędy formularza i API (`error.fields`) pokazują się pod polami, a fokus trafia na pierwsze błędne pole. `maxLength` jest równy limitom API.
- **Dostępność (WCAG 2.2 AA):** szuflada i okna mają nazwę z nagłówka i trzymają fokus w środku, Esc zamyka najwyższe okno, a fokus wraca do miejsca, z którego je otwarto. Obramowania pól mają kontrast 3:1, komunikaty są odczytywane przez czytniki ekranu, tabelę można przewijać klawiaturą, a przy ograniczonym ruchu nie ma animacji.

Jak działa logowanie:
- Token API (JWT, 12 h) jest w ciasteczku httpOnly `aed-session`, więc przeglądarka nigdy nie łączy się z API bezpośrednio.
- Dysponent widzi tylko rekordy, źródła i zgłoszenia swojej organizacji, a administrator wszystkie.

Zgłoszenie poprawki („Zgłoś poprawkę” na karcie AED) działa bez logowania. Ma limit 10 na minutę z jednego adresu i ukryte pole-pułapkę na boty.

Na produkcji konta powstają po wpisaniu `SEED_ADMIN_PASSWORD` i `SEED_DEMO_PASSWORD` w `~/hacknation-2026/aed.env` na VPS i ponownym `deploy.sh app up` (`deploy/WDROZENIE.md`).

## Import danych od dysponentów

Opis: [`docs/import-danych.md`](docs/import-danych.md). Najprościej w panelu (Import). Przez Scalar (`/scalar`):
1. **Auth → login**, np. `wroclaw@aed.local` / `AedDemo2026x`. Skopiuj `accessToken` do „Authentication → Bearer”.
2. **DataSource → GET**: id źródła.
3. **AedImport → POST file**: `dataSourceId`, plik `data/import-przyklad-wroclaw.csv` i `dryRun=true`, żeby zobaczyć raport bez zapisu. Potem to samo z `dryRun=false`.

Wynik od razu widać na mapie i w otwartych danych.

## Otwarte dane (dane.gov.pl)

Opis pól, słowników i zasad publikacji: [`docs/schemat-danych.md`](docs/schemat-danych.md). Strona w aplikacji: http://localhost:3060/pl/open-data.

| Plik (web: `/api/open-data/…`, API: `/api/v1/OpenData/…`) | Co zawiera |
|---|---|
| `aed.csv`, `aed.geojson`, `aed.json` | Cały zbiór, generowany przy każdym pobraniu. Filtry: `?wojewodztwo=02`, `?zrodlo=<id>` |
| `metadata.json` | Metadane zbioru: tytuł, opis, słowa kluczowe, kategoria „Zdrowie”, licencja, częstotliwość, zasoby, źródła z zakresem referencyjnym, pola, słowniki |
| `aed.xml`, `aed.md5` | Plik automatycznego importu dane.gov.pl (schemat `urn:otwarte-dane:harvester:1.13`) dla całego zbioru i jego suma MD5. GET i HEAD, `application/xml` |
| `aed-<id źródła>.xml`, `aed-<id źródła>.md5` | Wykaz jednego dostawcy (źródła danych, id bez myślników) do zarejestrowania w portalu pod jego instytucją. Adres podaje `metadata.json` (`sources[].harvesterUrl`) |
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
dotnet test api/Aed/Application.Test   # API: walidatory i logika (NUnit)
npm run test:unit --prefix web         # web: funkcje pomocnicze (node --test)
```

E2E (Playwright, na uruchomionym prototypie): mapa i karta AED, „Najbliższe AED”, otwarte dane, logowanie i uprawnienia, rekordy w panelu, import, zgłoszenie poprawki od mapy do panelu. Opis i uruchomienie: [`e2e/README.md`](e2e/README.md).

```powershell
cd e2e; npm install; npm run install:browsers
npm run test:critical
```

## Ograniczenia (świadome uproszczenia prototypu)

- Uprawnienia przez role (`Admin`, `Dispatcher`) w `[Authorize(Roles = …)]`. Car Reminder ma tabele uprawnień do modułów, tu ich nie ma.
- Brak refresh tokenów: token dostępu ważny 12 h.
- Brak e-maili (rejestracja, reset hasła). Konta zakłada seeder lub administrator.
- Plik importu XML (`aed.xml`, `aed-<id źródła>.xml`) przechodzi w testach schemat XSD z kodu portalu. Sprawdziliśmy go też na portalu dane.gov.pl uruchomionym lokalnie (`../portal/hacknation/README.md`): importer przyjął wykazy 2 dostawców, a CSV stał się tabelą (12/12 i 6/6 wierszy). Na produkcyjnym dane.gov.pl źródło XML zakłada administrator portalu na wniosek instytucji-dostawcy.
- Katalog DCAT-AP-PL jest zgodny z profilem 2.1 (klasy i słowniki ze specyfikacji), ale nie był pobierany przez portal. Importer DCAT wymaga endpointu SPARQL, więc podstawową drogą jest import XML.
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
