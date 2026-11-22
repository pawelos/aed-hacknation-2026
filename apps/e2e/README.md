# Testy E2E (Playwright)

Testy prowadzą aplikację w Chromium na **działającym** prototypie (web, API i baza z danymi demo). Sprawdzają główne ścieżki użytkownika, których nie pokrywają testy jednostkowe. Wzór: `e2e` w Car Reminder.

| Plik | Ścieżka | Tag |
|---|---|---|
| `public-map.spec.ts` | Obywatel: mapa z listą, wyszukiwanie miejscowości i karta AED, błąd zbyt krótkiego zapytania, „Najbliższe AED” z lokalizacji (z odległością i bez zgody na lokalizację) | `@smoke` |
| `open-data.spec.ts` | dane.gov.pl: strona „Otwarte dane”, CSV z kolumnami wzorca, GeoJSON z filtrem województwa, metadane i katalog DCAT-AP-PL | `@smoke` |
| `auth.spec.ts` | Panel bez sesji prowadzi do logowania, złe hasło (błąd pod polem, e-mail zostaje), logowanie i wylogowanie | `@smoke` |
| `access.spec.ts` | Dysponent widzi tylko swoje źródło, administrator wszystkie | `@smoke` |
| `panel-aeds.spec.ts` | Rekordy AED: wyszukiwanie, edycja w szufladzie, walidacja nowego rekordu pod polami, usunięcie przez pasek zaznaczenia | `@critical` |
| `panel-import.spec.ts` | Import: sprawdzenie pliku `data/import-przyklad-wroclaw.csv` bez zapisu (podsumowanie i wiersze z błędami), brak pliku | `@critical` |
| `correction-report.spec.ts` | Od mapy do panelu: obywatel zgłasza poprawkę z karty AED, dysponent odrzuca ją w panelu | `@critical` |

## Przygotowanie

```powershell
cd apps/e2e
npm install
npm run install:browsers
copy .env.example .env   # opcjonalnie: inne adresy lub hasła kont demo
```

Domyślne adresy to web `http://localhost:3060` i API `http://localhost:5296`. Konta demo pochodzą z `api/Aed/Api/appsettings.Development.json`.

## Uruchomienie

1. Uruchom prototyp (Aspire albo osobno API i `npm run dev` w `web/`, opis w [`../README.md`](../README.md#jak-uruchomić)). API musi odpowiadać `200` na `GET /health`.
2. Uruchom testy:

```powershell
npm run test:smoke      # @smoke: mapa, otwarte dane, logowanie, uprawnienia (ok. 1 min)
npm run test:critical   # @smoke + @critical: wszystko powyżej i ścieżki panelu
npm test                # wszystkie testy
npm run test:ui         # tryb interaktywny
npm run report          # raport HTML ostatniego przebiegu
```

W Windows PowerShell 5.1, a zwłaszcza w **PowerShell ISE**, znaki `›` i polskie litery mogą się wyświetlać jako `â€ş` czy `ĹĽ`, a kolory jako dosłowne `[36m`. Testom to nie szkodzi: ISE czyta wyjście Node w stronie kodowej Windows i nie obsługuje kolorów ANSI. Pomagają dwie linie przed testami, w każdej nowej sesji albo na początku skryptu:

```powershell
[Console]::OutputEncoding = [Text.Encoding]::UTF8   # wyjście Node jako UTF-8
$env:FORCE_COLOR = '0'                              # bez kodów kolorów (ISE ich nie pokazuje)
```

Windows Terminal i terminal VS Code tego nie potrzebują.

Ślady i zrzuty ekranu nieudanych testów trafiają do `test-results/`, a raport do `playwright-report/`.

## Dane testowe

- Rekordy tworzone przez testy mają nazwę z przedrostkiem `e2e-` (`uniqueName`) i powstają przez API (`support/api.ts`), a nie przez formularz.
- Każda specyfikacja usuwa swoje rekordy w `afterAll`, a na starcie sprząta pozostałości wcześniejszych przebiegów.
- Import uruchamiany jest tylko w trybie sprawdzenia (`dryRun`), więc niczego nie zapisuje.
- Zgłoszenie poprawki zostaje w bazie jako odrzucone. Jego rekord AED jest usuwany.
- Testy biegną po kolei (`workers: 1`): korzystają z jednej bazy i z limitu 10 zgłoszeń na minutę z jednego adresu.

## Pomocnicze (`tests/support/`)

- `accounts.ts`: konta demo, `signIn()` i `signInToPanel()`, `storageStateOf(role)`. Sesje zapisuje `auth.setup.ts` (ciasteczko `aed-session`).
- `api.ts`: `apiAs(role)` (klient API z tokenem), `createAed()`, `removeE2eAeds()`, `valueOf()`.

## Zasady

- Elementy wybieramy po roli, etykiecie i widocznym tekście (`getByRole`, `getByLabel`). Przy okazji testy sprawdzają dostępność: błędy pod polami mają `aria-invalid` i `aria-describedby`.
- Teksty interfejsu są polskie (adresy `/pl/...`).
- Tagi `@smoke` i `@critical` nadajemy na `test.describe`. `auth.setup.ts` ma oba tagi, żeby `--grep` zawsze najpierw logował.
