# Import danych o AED od dysponentów

Import łączy dane wielu dysponentów w jeden zbiór. Działa w panelu dysponenta (`/pl/panel/import`: sprawdzenie bez zapisu, import, historia) i przez API (Scalar, grupa **AedImport**). Kod jest w `api/Aed/Application/Modules/AedImport/`.

## Kto i dokąd importuje

- Konto **dysponenta** (rola `Dispatcher`) importuje tylko do źródeł swojej organizacji, a **administrator** do każdego.
- Źródła użytkownika zwraca `GET /api/v1/DataSource` (nazwa, licencja, zakres referencyjny, liczba rekordów).
- Każdy rekord zapamiętuje swoje źródło, więc pochodzenie i zakres referencyjny zostają znane po integracji.

## Formaty

| Format | Jak czytany |
|---|---|
| **CSV** | RFC 4180, wartości w cudzysłowie mogą zawierać separator i nowe linie. Separator wykrywany z nagłówka: przecinek (format wzorcowy), średnik (polski Excel) albo tabulator. Kodowanie UTF-8 (z BOM lub bez), a gdy plik nie jest UTF-8, Windows-1250 (polski Excel). |
| **GeoJSON** | `FeatureCollection` punktów (RFC 7946). Współrzędne pochodzą z geometrii, reszta z `properties`. Działają eksporty OpenStreetMap (overpass-turbo, `emergency=defibrillator`): `opening_hours`, `indoor`, `access`, `phone`, `defibrillator:location`, `addr:*`. |

Nazwy kolumn dopasowuje się do pól wzorcowych (`docs/schemat-danych.md`) bez względu na wielkość liter, polskie znaki, spacje i myślniki. Działają też popularne nazwy, np. `Nazwa`, `Miasto`, `lat`, `lon`, `Telefon`, `Godziny otwarcia`. Kolumny nieznane są pomijane i wymienione w raporcie.

Szablon CSV z nagłówkiem i przykładowym wierszem: `GET /api/v1/AedImport/template.csv`. Limit: 5 MB i 10 000 wierszy na import.

## Endpointy

| Endpoint | Co robi |
|---|---|
| `POST /api/v1/AedImport/file` (multipart) | `dataSourceId`, `file`, `dryRun`, `markMissingAsOutdated` |
| `POST /api/v1/AedImport/url` (JSON) | `{ dataSourceId, url, dryRun, markMissingAsOutdated }`. Bez `url` używany jest adres zapisany w źródle, więc dysponent publikujący plik pod stałym adresem odświeża zbiór jednym wywołaniem |
| `GET /api/v1/AedImport?dataSourceId=` | Historia importów |
| `GET /api/v1/AedImport/{id}` | Pełny raport zapisanego importu |

- `dryRun=true` sprawdza plik i zwraca raport wszystkich wierszy bez zapisu.
- `markMissingAsOutdated=true` oznacza rekordy źródła nieobecne w pliku jako `nieaktualny`. To pełne odświeżenie: nic nie jest usuwane, historia zostaje.

## Walidacja wiersza

| Błąd: wiersz odrzucony | Ostrzeżenie: wiersz wczytany i oznaczony „do weryfikacji” |
|---|---|
| brak nazwy lokalizacji albo miejscowości | brak adresu |
| brak lub zły zapis współrzędnych | zamienione szerokość i długość (poprawione automatycznie) |
| punkt poza Polską | telefon, kod pocztowy, TERYT, data nie do odczytania (pole puste) |
| brak województwa (podaj `wojewodztwo` albo `teryt_gminy`, z którego jest wyliczane) | nieznany kod słownika (użyta wartość domyślna) |
| wartość dłuższa niż limit pola | godziny poza formatem `opening_hours` („dostępne teraz” nieznane) |
| powtórzony `id_zrodlowe` w tym samym pliku | weryfikacja starsza niż 12 miesięcy; możliwy duplikat |

Przy wczytywaniu wartości są ujednolicane:
- telefon do E.164 (`71 777 00 00` → `+48717770000`);
- kod pocztowy do `00-000`;
- przecinek dziesiętny we współrzędnych zamieniany na kropkę;
- dostępność z tagu OSM `access` (`yes`, `customers`, `private`) albo z godzin (`24/7` oznacza całodobowo).

## Integracja i duplikaty

- **Ten sam rekord źródła:** ten sam `id_zrodlowe` aktualizuje istniejący rekord. Bez identyfikatora za ten sam rekord uznawany jest rekord źródła w promieniu 5 m. Ponowny import tego samego pliku nic nie zmienia (raport: `unchanged`).
- **Możliwy duplikat z innego źródła:** rekord innego źródła w promieniu 30 m z podobną nazwą (wspólne słowa) albo tym samym adresem. Oba rekordy zostają w zbiorze, a importowany dostaje flagę `wymaga_weryfikacji` i wskazanie `possibleDuplicateOf`. O scaleniu decyduje administrator.
- **Raport zapisany w historii:** liczniki oraz wiersze z błędami i ostrzeżeniami. Kody (`Import.OutsidePoland`, `Import.PossibleDuplicate`…) tłumaczy panel. Opis po angielsku podaje wartość, która wywołała problem.

## Import z adresu URL: bezpieczeństwo

Pobiera serwer, więc dostęp jest ograniczony:
- tylko adresy `http(s)`;
- host musi wskazywać adresy publiczne: adresy prywatne, lokalne, link-local i CGNAT są odrzucane, a połączenie idzie na sprawdzony adres, bez drugiego zapytania DNS;
- bez przekierowań;
- maksymalnie 5 MB i 20 s.

## Przykład (dane demo)

Pliki w `data/`:
- `import-przyklad-wroclaw.csv`:
  - średnik, nazwy kolumn z Excela;
  - 2 aktualizacje i 3 nowe rekordy;
  - zamienione współrzędne, zły telefon, nieznany kod;
  - 3 wiersze odrzucone;
  - 2 możliwe duplikaty ze „Społecznej mapy AED”.

  Konto `wroclaw@aed.local`, źródło „Rejestr AED w budynkach miejskich Wrocławia”.
- `import-przyklad-osm.geojson`: format eksportu OpenStreetMap, 3 nowe rekordy i 1 odrzucony (brak województwa). Konto administratora.

Uwaga licencyjna: dane z OpenStreetMap są na licencji ODbL, a dane.gov.pl przyjmuje licencje CC. Import z OSM nadaje się do porównania i uzupełniania, ale do publikacji potrzebna jest zgoda właściciela danych albo dane od samego dysponenta.
