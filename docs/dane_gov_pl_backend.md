# Backend dane.gov.pl: co z niego wynika dla AED Radar

Analiza kodu z https://dane.gov.pl/source-code/backend_latest.zip (wersja **2.63.0**, 2026-10-28, wg `CHANGELOG.md`). Licencja GPL v3, autor KPRM, oryginalne repozytorium `gitlab.dane.gov.pl/mcod/backend`. Ścieżki niżej są względem katalogu głównego archiwum.

Kryterium „zgodność z zasadami działania portalu dane.gov.pl” ma wagę 30%. Ten dokument zbiera reguły, które portal faktycznie egzekwuje w kodzie, żeby nasz zbiór, eksport i API dało się opublikować bez poprawek.

## 1. Co to za system

| Element | Technologia | Gdzie |
|---|---|---|
| Panel administracyjny (dostawcy danych dodają zbiory i zasoby) | Django 2.2 + django-suit | `mcod/*/admin.py` |
| Publiczne API `api.dane.gov.pl` | Falcon, JSON:API (`marshmallow-jsonapi`), OpenAPI | `mcod/api.py`, `mcod/routes.py` |
| CMS (strony informacyjne) | Wagtail 2.13 | `mcod/cms/` |
| Wyszukiwarka | Elasticsearch 6 | `mcod/search/`, `docker/elasticsearch/` |
| Zadania w tle (pobieranie plików, walidacja, harvester) | Celery + RabbitMQ, Redis | `mcod/celeryapp.py`, `mcod/*/tasks.py` |
| Dane powiązane (DCAT-AP, SPARQL) | rdflib, pyshacl, Apache Jena Fuseki | `mcod/core/api/rdf/`, `data/shacl/`, `docker/rdfdb/` |
| Walidacja tabel | goodtables (frictionless) | `mcod/resources/goodtables_checks.py` |
| Baza danych | PostgreSQL | `docker/postgres/` |

Najważniejsze moduły w `mcod/`: `datasets` (zbiory), `resources` (zasoby: pliki, API, strony), `organizations` (instytucje), `harvester` (import automatyczny), `categories`, `tags`, `licenses`, `regions` (TERYT), `special_signs` (znaki umowne), `submissions` i `suggestions` (propozycje nowych danych od obywateli), `showcases` (aplikacje na danych), `watchers` (subskrypcje zmian).

## 2. Model danych portalu

**Instytucja** (dostawca) → **zbiór danych** (Dataset) → **zasoby** (Resource: plik, link do API lub strona WWW).

### Zbiór danych (`mcod/datasets/models.py`)

- `title`, `notes` (opis), `url`, `ext_ident` (identyfikator w systemie dostawcy), kategorie i tagi (pl/en).
- `update_frequency` (linie 55–65): `daily`, `weekly`, `monthly`, `quarterly`, `everyHalfYear`, `yearly`, `irregular`, `notPlanned`.
- Licencje (linie 183–199): CC0 1.0, CC BY 4.0, CC BY-SA 4.0, CC BY-NC 4.0, CC BY-NC-SA 4.0, CC BY-ND 4.0, CC BY-NC-ND 4.0. Domyślnie **CC0**. Gdy dostawca ustawi warunek ponownego wykorzystania (podanie źródła, informacja o modyfikacji, zakres odpowiedzialności), licencja podnosi się do **CC BY 4.0** (`license_code`, linie 568–583).
- Flagi: `has_dynamic_data`, `has_high_value_data`, `has_high_value_data_from_ec_list` (HVD), `has_research_data`.
- `verified` to data aktualizacji widoczna w portalu.

### Zasób (`mcod/resources/models.py`)

- `type` (linie 171–177): `file`, `website`, `api`.
- `link` (do 2000 znaków), `format`, `openness_score`, `data_date` (data danych), `regions` (do 20 kodów TERYT), `special_signs`, `contains_protected_data`.
- `has_table`, `has_map`, `has_chart`, `tabular_data_schema`: portal sam czyta tabelę z pliku i udostępnia ją w API (`/resources/{id}/data`) oraz na mapie (`/resources/{id}/geo`).
- `file_encoding`: kodowanie wykrywane automatycznie (`mcod/resources/guess.py`, cchardet), separator CSV przez `csv.Sniffer` (`models.py:718`).

## 3. Formaty i ocena otwartości (5 gwiazdek)

`mcod/settings/base.py:1207` (`SUPPORTED_CONTENT_TYPES`) podaje domyślną ocenę dla każdego formatu:

| Ocena | Formaty |
|---|---|
| 1 | PDF, DOC/DOCX, ODT, RTF, PPT |
| 2 | XLS, XLSX |
| 3 | CSV, JSON, GeoJSON, XML, GML, KML, SHP, ODS |
| 4–5 | RDF, JSON-LD i JSON z kontekstem `@context` (dane powiązane) |

- Kalkulatory w `mcod/resources/score_computation/calculators/` dają JSON-owi 4 lub 5 gwiazdek, jeśli parsuje się jako JSON-LD (także przez nagłówek HTTP `Link` z `application/ld+json`). Za 5 gwiazdek trzeba mieć linki do zewnętrznych URI (`contains_linked_data.py`).
- Portal sam generuje do zasobów tabelarycznych kopię CSV i JSON-LD (`csv_file`, `jsonld_file`). Dlatego pliki CSV w portalu często dostają 4 gwiazdki.
- Maksymalny rozmiar pliku to 1 GB (`base.py:1016`). Archiwa (zip, 7z, rar, gz) są rozpakowywane, a ocenę liczy się dla zawartości.

## 4. Mapa w portalu: wymagania dla kolumn

`GEO_TYPES` w `mcod/settings/base.py:1838` określa, jak dostawca oznacza kolumny, żeby portal narysował mapę. Wymagany jest jeden z trzech wariantów:

1. **Współrzędne geograficzne**: kolumny `długość geograficzna` (l) i `szerokość geograficzna` (b) oraz `etykieta`. Układ **WGS84**, kolumny **numeryczne**.
2. **Adres uniwersalny** (`uaddress`) oraz `etykieta`. Format to pola rozdzielone `|`. Pola 6–7 to współrzędne w PUWG 1992 (EPSG:2180), przeliczane na WGS84 (`mcod/resources/geo.py:338–346`).
3. **Adres**: `miejscowość` i `kod pocztowy` oraz `etykieta` (ulica i numer domu są opcjonalne). Portal geokoduje go własnym geokoderem (`geo.py`, `GEOCODER_URL`).

**Wniosek:** wzorcowy zbiór AED powinien mieć osobne kolumny `dlugosc_geograficzna` i `szerokosc_geograficzna` (liczby, kropka jako separator dziesiętny, WGS84) oraz kolumnę z etykietą. Pełny adres (miejscowość, kod, ulica, numer) zostaje jako zapasowa droga do mapy.

## 5. Import automatyczny (harvester)

`mcod/harvester/models.py:124–141`: portal pobiera dane cyklicznie (co 1, 7, 30 lub 90 dni) z trzech typów źródeł:

| Typ | Opis |
|---|---|
| `ckan` | Katalog CKAN (np. portale miejskie) |
| `xml` | Plik XML w formacie „Otwarte Dane” wystawiony przez dostawcę pod stałym adresem |
| `dcat` | Katalog DCAT-AP (RDF, endpoint SPARQL) |

### Format XML importu (najważniejsze dla nas)

Schemat: `data/harvester/xml_import_otwarte_dane_1_13.xsd` (namespace `urn:otwarte-dane:harvester:1.13`), przykład: `data/test_samples/harvester/import_example1.13.xml`.

```xml
<ns2:datasets xmlns:ns2="urn:otwarte-dane:harvester:1.13">
  <dataset status="published">
    <extIdent>aed-gmina-0918123</extIdent>              <!-- wymagane, max 36 -->
    <title><polish>…</polish><english>…</english></title> <!-- polish wymagane, max 300 -->
    <description><polish>…</polish></description>       <!-- wymagane, max 1000 -->
    <updateFrequency>daily</updateFrequency>             <!-- wymagane -->
    <categories><category>HEAL</category></categories>   <!-- wymagane -->
    <conditions>…<dbOrCopyrightedLicenseChosen>CC0 1.0</dbOrCopyrightedLicenseChosen></conditions>
    <resources>                                          <!-- wymagane, min. 1 -->
      <resource status="published">
        <extIdent>…</extIdent>
        <url>https://…/aed.csv</url>                      <!-- wymagane, tylko https://, max 2000 -->
        <title>…</title><description>…</description>
        <availability>remote</availability>               <!-- local = portal kopiuje plik -->
        <dataDate>2026-11-21</dataDate>
        <hasDynamicData>false</hasDynamicData>
        <regions><terytIdent>1465011</terytIdent></regions> <!-- 2–7 znaków, max 20 -->
      </resource>
    </resources>
    <tags><tag lang="pl">defibrylator</tag></tags>       <!-- wymagane -->
  </dataset>
</ns2:datasets>
```

Kategorie (`categoryType`): AGRI, EDUC, ENVI, ENER, TRAN, TECH, ECON, SOCI, **HEAL** (Zdrowie), GOVE, REGI, JUST, INTR. Słowniki pochodzą z UE (DCAT-AP).

**Wniosek:** prototyp może wystawić **gotowy plik harvestera XML 1.13** (np. `/api/export/harvester.xml`). Wtedy portal pobiera nasz zbiór sam co dobę, bez ręcznej publikacji. To najmocniejszy argument za „zgodnością z zasadami portalu”. Plik trzeba walidować XSD z archiwum.

## 6. Publiczne API (`mcod/routes.py`, `mcod/core/api/versions.py`)

- Wersje `1.0` i `1.4` (domyślna z dokumentacją), nagłówek `X-API-VERSION` lub prefiks `/1.4/`. Dokumentacja jest pod `/doc`, specyfikacja OpenAPI pod `/spec`.
- Format **JSON:API** (`application/vnd.api+json`): `data[].attributes`, `relationships`, `meta`, `links`.
- Endpointy: `/datasets`, `/datasets/{id}/resources`, `/resources/{id}`, `/resources/{id}/data` (wiersze tabeli z filtrowaniem), `/resources/{id}/geo` (punkty na mapę), `/institutions`, `/search`, `/catalog` (DCAT-AP w RDF), `/sparql`.
- `/submissions` przyjmuje propozycje udostępnienia danych od obywateli. To mechanizm podobny do naszego „zgłoś AED / poprawkę”.

**Wniosek:** odpowiedzi naszego API o AED warto zrobić w stylu JSON:API (te same nazwy pól, paginacja `page` / `per_page`), a do eksportu katalogu dodać DCAT-AP w JSON-LD.

## 7. DCAT-AP i SHACL

`data/shacl/` zawiera kształty DCAT-AP 2.0.1 i 2.1.0, `data/spec/` specyfikację API RDF DCAT-AP i DCAT-AP PL. Portal waliduje katalogi DCAT importowane przez harvester tymi kształtami (pyshacl). Opis metadanych naszego zbioru w JSON-LD zgodny z DCAT-AP dałby się bezpośrednio podpiąć jako źródło `dcat`.

## 8. Istniejące zbiory AED w portalu (stan z 2026-11-21, API 1.4)

| ID | Zbiór | Formaty | Licencja | Kategorie | Uwagi |
|---|---|---|---|---|---|
| 3624 | Defibrylatory w Mieście Krosno | CSV | CC0 | SOCI, HEAL | kolumny: `Długość geograficzna, Szerokość geograficzna, Nazwa, Lokalizacja, Adres, Numer Id` |
| 4581 | Lokalizacje AED w Przemyślu | CSV, GeoJSON, JSON, GML | CC0 | SOCI, HEAL | link do zewnętrznego API (skycms), aktualizacja codziennie |
| 4955 | Defibrylatory w Mieście Marki | CSV, XLSX | CC BY 4.0 | SOCI, HEAL | ten sam układ kolumn co Krosno, ale CSV jest uszkodzony: cały wiersz w cudzysłowie, szerokość i długość zamienione miejscami |
| 39512 | Lokalizacja defibrylatorów (AED), Warszawa | JSON | CC BY 4.0 | REGI | link do portalu miejskiego |

Co z tego wynika:

- Danych jest mało, każda gmina ma inny układ, a jakość bywa zła (Marki). To potwierdza sens zadania i jest dobrym przykładem do prezentacji: walidator wyłapałby oba błędy z Marek.
- Wspólny trzon Krosna i Marek (współrzędne, nazwa, lokalizacja, adres, id) warto zachować w naszym wzorcu jako **podzbiór zgodny wstecz**.
- Kategorie: **HEAL** (główna) i **SOCI**, tak jak w istniejących zbiorach.

## 9. Rekomendacje dla AED Radar

1. **Wzorzec XLSX i CSV**: UTF-8, przecinek jako separator, kropka dziesiętna, nagłówek w pierwszym wierszu, nazwy kolumn bez spacji. Współrzędne WGS84 w dwóch kolumnach liczbowych, kolumna etykiety, pełny adres, kod TERYT gminy, `data_weryfikacji` (ISO 8601), `zrodlo` i `url_zrodla`, `licencja`, `id` stabilny (≤ 36 znaków, nadaje się na `extIdent`).
2. **Formaty eksportu**: CSV (3★), GeoJSON (3★), JSON-LD z `@context` schema.org/`dcat` (4–5★). XLSX tylko jako wzorzec do wypełnienia (2★), nie jako główny format danych.
3. **Plik harvestera XML 1.13**: jeden `dataset` z kategoriami HEAL i SOCI, `updateFrequency=daily`, licencja CC0 1.0 (domyślna w portalu, najprostsza dla służb), zasoby CSV, GeoJSON i JSON-LD jako `remote`.
4. **Walidacja przy imporcie**, wzorowana na portalu: wykrywanie kodowania i separatora, sprawdzenie, czy współrzędne są w Polsce (zamiana lat/lon jak w Markach), typy kolumn, duplikaty.
5. **API** w stylu JSON:API, z endpointem danych tabelarycznych i geo, podobnie jak `/resources/{id}/data` i `/geo`.
6. **Metadane zasobu**: `dataDate`, regiony TERYT (do 20 na zasób, więc zbiór ogólnopolski lepiej dzielić na zasoby według województw albo podać kody województw), `hasDynamicData=true`, jeśli dane są odświeżane na bieżąco.

## 10. Stan wdrożenia i weryfikacja na portalu uruchomionym lokalnie (22.11.2026)

Portal (backend 2.63.0) uruchomiliśmy z kodu źródłowego w Dockerze: folder [`portal/`](../portal/hacknation/README.md), panel `http://localhost:8001`, API `http://localhost:8000`. Importer XML portalu pobrał wykazy dwóch dostawców z produkcji AED Radar (`aed-<id źródła>.xml`).

| Rekomendacja z sekcji 9 | Stan |
|---|---|
| 1. Wzorzec XLSX i CSV | Zrobione: `wzorcowy-zakres-danych-aed.xlsx`, CSV RFC 4180, współrzędne WGS 84 w osobnych kolumnach |
| 2. Formaty eksportu | CSV, GeoJSON, JSON. W portalu CSV dostał 4★ po zbudowaniu tabeli, GeoJSON i JSON 3★. JSON-LD (5★) nie jest zrobiony |
| 3. Plik harvestera XML 1.13 | Zrobione inaczej niż w planie: oprócz `aed.xml` całego zbioru każde źródło ma własny plik `aed-<id>.xml`, tak jak w wykazie rzeczy znalezionych (dostawca = instytucja). Licencja z `<conditions>` według licencji źródła, kategorie HEAL i SOCI, regiony to kody województw |
| 4. Walidacja przy imporcie | Zrobione w panelu dysponenta (kodowanie, separator, współrzędne w Polsce, duplikaty) |
| 5. API | Publiczne API prototypu (`/scalar`) |
| 6. Metadane zasobu | `dataDate`, regiony TERYT, znak umowny `X`. `hasDynamicData=false` |

Co pokazał prawdziwy importer (czego nie widać w samym XSD):

- **Format daty i czasu w CSV.** Indeks tabel w Elasticsearch przyjmuje tylko formaty z `DATE_BASE_FORMATS` (`mcod/settings/base.py:1469`), np. `yyyy-MM-dd HH:mm:ss`. Zapis ISO 8601 `2026-11-21T19:17:07Z` portal odrzucał w każdym wierszu, więc tabela była pusta. AED Radar podaje teraz `data_aktualizacji` jako `YYYY-MM-DD hh:mm:ss` (UTC). Po zmianie: 12/12 i 6/6 wierszy.
- **Regiony TERYT** importer sprawdza w wewnętrznym geokoderze portalu (Pelias i Placeholder, `GEOCODER_URL`, `PLACEHOLDER_URL`). Tych usług nie ma w publicznym kodzie, więc lokalnie zastąpiliśmy je zaślepką dla 16 województw. Kody województw w XML są poprawne i portal zapisał je jako „woj. dolnośląskie” z regionem nadrzędnym „Polska”.
- **Zmieniony plik zasobu** portal pobiera ponownie dopiero przy kolejnym imporcie albo po wymuszeniu (`process_resource_from_url_task(id, forced_file_changed=True)`). Przy takim samym XML (ta sama suma MD5) nie widzi zmiany danych. Wniosek dla dostawców: data `dataDate` i `lastUpdateDate` w XML zmieniają się razem z danymi, więc zmienia się też MD5.
- **Mapa** (`has_map`) nie powstała automatycznie. Portal rysuje mapę dopiero wtedy, gdy dostawca oznaczy w panelu kolumny współrzędnych i etykiety (sekcja 4). Tego kroku nie sprawdzaliśmy.
- **Błąd w przykładowej konfiguracji portalu:** `mcod/settings/local.py.sample` podaje `timeout` jako ustawienie indeksu, które Elasticsearch 6.8 odrzuca. Wtedy nie indeksuje się żadna tabela. Poprawka jest w `portal/hacknation/local-overrides.py`.
