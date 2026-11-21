# Wzorcowy zakres danych o defibrylatorach AED

Wersja schematu 1.0. Dokument wygenerowany z metadanych API (`GET /api/v1/OpenData/metadata.json`). Ich źródłem jest `api/Aed/Application/Modules/OpenData/AedDataSchema.cs`, więc ta sama definicja pól obsługuje CSV, GeoJSON, JSON, szablon XLSX i ten opis. Po zmianie pól wygeneruj dokument ponownie.

## Publikacja

| Zasób | Adres (produkcja) | Format |
|---|---|---|
| Defibrylatory AED – CSV | `https://aed.hacknation-2026.pl/api/open-data/aed.csv` | text/csv |
| Defibrylatory AED – GeoJSON | `https://aed.hacknation-2026.pl/api/open-data/aed.geojson` | application/geo+json |
| Defibrylatory AED – JSON | `https://aed.hacknation-2026.pl/api/open-data/aed.json` | application/json |
| Metadane zbioru | `https://aed.hacknation-2026.pl/api/open-data/metadata.json` | application/json |
| Katalog DCAT-AP-PL 2.1 | `https://aed.hacknation-2026.pl/api/open-data/catalog.rdf` | application/rdf+xml |
| Wzorcowy zakres danych | `https://aed.hacknation-2026.pl/api/open-data/wzorcowy-zakres-danych-aed.xlsx` | XLSX, wyłącznie jako wzorzec |

Filtry plików danych: `?wojewodztwo=02` (kod TERYT albo nazwa, np. `dolnośląskie`) oraz `?zrodlo=<id źródła>` (id z metadanych).

Zasady:
- **Kodowanie i CSV**: UTF-8. CSV według RFC 4180: bez BOM, separator przecinek, CRLF, nagłówek z nazwami pól.
- **GeoJSON**: zgodny z RFC 7946. Geometria `Point` [długość, szerokość], wszystkie pola w `properties`.
- **Współrzędne**: WGS 84 (EPSG:4326), 6 miejsc po kropce.
- **Daty**: `YYYY-MM-DD`. Data i czas: ISO 8601 w UTC.
- **Godziny dostępności**: format OpenStreetMap `opening_hours`. Na jego podstawie API liczy „dostępne teraz”.
- **Świeżość plików**: pliki generowane są z bazy przy każdym pobraniu (stały adres, nagłówki `Last-Modified` i `Cache-Control: max-age=300`) i dostępne dla innych serwisów (CORS `*`).
- **Licencja zbioru**: najbardziej wymagająca z licencji źródeł (obecnie CC-BY-4.0). Każdy rekord ma też własną licencję w polu `licencja_danych`.

## Metadane dla dane.gov.pl

| Pole | Wartość |
|---|---|
| Tytuł | Defibrylatory AED w Polsce – zbiór zintegrowany AED Radar |
| Kategoria | Zdrowie (motyw UE `HEAL`) |
| Słowa kluczowe | AED, defibrylator, pierwsza pomoc, nagłe zatrzymanie krążenia, ratownictwo, zdrowie, lokalizacje |
| Częstotliwość aktualizacji | na bieżąco (pliki generowane przy każdym pobraniu) (`UPDATE_CONT`) |
| Zasięg | Polska |
| Dostawca | AED Radar – prototyp zespołu HackNation 2026 |
| Kontakt | kontakt@hacknation-2026.pl |

## Pola

| Lp. | Pole | Typ | Wymagane | Maks. długość | Format | Opis |
|---|---|---|---|---|---|---|
| 1 | `id_rekordu` | tekst | tak | 36 | UUID | Unikalny, trwały identyfikator rekordu w zbiorze krajowym. |
| 2 | `id_zrodlowe` | tekst | nie | 100 | tekst | Identyfikator rekordu w systemie dysponenta; razem ze źródłem pozwala aktualizować rekord przy kolejnym imporcie. |
| 3 | `nazwa_lokalizacji` | tekst | tak | 300 | tekst | Nazwa miejsca, w którym znajduje się AED. |
| 4 | `adres` | tekst | tak | 300 | ulica i numer | Ulica i numer budynku (bez kodu i miejscowości). |
| 5 | `kod_pocztowy` | tekst | nie | 6 | 00-000 | Kod pocztowy. |
| 6 | `miejscowosc` | tekst | tak | 100 | tekst | Miejscowość. |
| 7 | `gmina` | tekst | nie | 100 | tekst | Gmina. |
| 8 | `teryt_gminy` | tekst | nie | 7 | 7 cyfr (TERYT TERC) | Kod TERYT gminy z rodzajem, pozwala łączyć zbiór z innymi danymi publicznymi. |
| 9 | `powiat` | tekst | nie | 100 | tekst | Powiat. |
| 10 | `wojewodztwo` | tekst | tak | 30 | nazwa urzędowa małymi literami | Województwo. |
| 11 | `teryt_wojewodztwa` | tekst | tak | 2 | 2 cyfry (TERYT) | Kod TERYT województwa. |
| 12 | `szerokosc_geograficzna` | liczba dziesiętna | tak |  | WGS 84 (EPSG:4326), stopnie dziesiętne, 6 miejsc po kropce | Szerokość geograficzna urządzenia. |
| 13 | `dlugosc_geograficzna` | liczba dziesiętna | tak |  | WGS 84 (EPSG:4326), stopnie dziesiętne, 6 miejsc po kropce | Długość geograficzna urządzenia. |
| 14 | `lokalizacja_szczegolowa` | tekst | nie | 500 | tekst | Gdzie dokładnie wisi urządzenie (piętro, pomieszczenie, przy czym). |
| 15 | `wewnatrz_budynku` | logiczny | tak |  | true / false | true – w budynku, false – na zewnątrz (np. szafka na elewacji). |
| 16 | `rodzaj_obiektu` | kod słownikowy | tak |  | kod ze słownika rodzaj_obiektu | Rodzaj miejsca. |
| 17 | `dostepnosc` | kod słownikowy | tak |  | kod ze słownika dostepnosc | Kto i kiedy może sięgnąć po urządzenie. |
| 18 | `godziny_dostepnosci` | tekst | nie | 200 | format OpenStreetMap opening_hours, np. Mo-Fr 08:00-16:00; Sa 10:00-14:00 albo 24/7 | Godziny, w których AED jest dostępne; format czytelny maszynowo, z niego liczone jest „dostępne teraz”. |
| 19 | `uwagi_dostepu` | tekst | nie | 500 | tekst | Jak uzyskać dostęp, gdy urządzenie nie wisi swobodnie (np. klucz w portierni). |
| 20 | `status_urzadzenia` | kod słownikowy | tak |  | kod ze słownika status_urzadzenia | Stan urządzenia. |
| 21 | `telefon_kontaktowy` | tekst | nie | 20 | E.164, np. +48717770000 | Telefon kontaktowy do dysponenta urządzenia. |
| 22 | `wlasciciel_dysponent` | tekst | tak | 300 | tekst | Właściciel lub dysponent urządzenia. |
| 23 | `zrodlo_danych` | tekst | tak | 300 | tekst | Nazwa źródła, z którego pochodzi rekord. |
| 24 | `url_zrodla` | tekst | nie | 1000 | URL | Adres źródła danych dysponenta. |
| 25 | `licencja_danych` | kod słownikowy | tak |  | identyfikator SPDX ze słownika licencja_danych | Licencja, na której dysponent udostępnił rekord. |
| 26 | `zakres_referencyjny` | tekst | tak | 500 | tekst | Zakres, którego dotyczy źródło (obszar i rodzaj obiektów), np. „Gmina Wrocław – budynki miejskie”. |
| 27 | `data_ostatniej_weryfikacji` | data | nie |  | YYYY-MM-DD | Kiedy dysponent ostatnio potwierdził dane (czas polski). |
| 28 | `wymaga_weryfikacji` | logiczny | tak |  | true / false | true, gdy rekord czeka na sprawdzenie (ostrzeżenie importu, zgłoszenie, weryfikacja starsza niż 12 miesięcy). |
| 29 | `data_aktualizacji` | data i czas | tak |  | ISO 8601 UTC, YYYY-MM-DDThh:mm:ssZ | Kiedy rekord ostatnio zmieniono w zbiorze krajowym. |

Pola z zadania („Proponowany schemat danych”) są zachowane pod tymi samymi nazwami. Jedyna zmiana pisowni: `wlasciciel_dysponent` (w zadaniu `wlasiciel_dysponent`). Dodaliśmy pola potrzebne w nagłej sytuacji i przy integracji:
- `lokalizacja_szczegolowa`, `wewnatrz_budynku` i `uwagi_dostepu`: gdzie dokładnie wisi urządzenie i jak się do niego dostać. Odpowiadają na problem AED zamkniętych na klucz;
- `teryt_gminy` i `teryt_wojewodztwa`: do łączenia z innymi danymi publicznymi;
- `id_zrodlowe`: aktualizacja rekordu przy kolejnym imporcie tego samego źródła;
- `wymaga_weryfikacji` i `data_aktualizacji`: aktualność danych.

## Słowniki

### `rodzaj_obiektu`

| Kod | Opis |
|---|---|
| `urzad` | Urząd |
| `szkola` | Szkoła, przedszkole, uczelnia |
| `ochrona_zdrowia` | Szpital, przychodnia, apteka |
| `straz_pozarna` | Remiza OSP lub PSP |
| `policja` | Policja lub straż miejska |
| `handel` | Sklep lub centrum handlowe |
| `biuro` | Biurowiec lub zakład pracy |
| `transport` | Dworzec, lotnisko, metro |
| `sport` | Obiekt sportowy |
| `obiekt_sakralny` | Kościół lub inny obiekt sakralny |
| `kultura_turystyka` | Kultura, hotel, turystyka |
| `szafka_zewnetrzna` | Szafka w przestrzeni publicznej |
| `inne` | Inne |

### `dostepnosc`

| Kod | Opis |
|---|---|
| `calodobowa` | Dostępny dla każdego całą dobę |
| `w_godzinach_otwarcia` | Dostępny w godzinach otwarcia miejsca |
| `ograniczona` | Dostęp ograniczony (personel, klucz) |

### `status_urzadzenia`

| Kod | Opis |
|---|---|
| `sprawny` | Sprawny i na miejscu |
| `do_weryfikacji` | Do weryfikacji przez dysponenta |
| `nieaktualny` | Dane nieaktualne |
| `wycofany` | Urządzenie wycofane |

### `licencja_danych`

| Kod | Opis |
|---|---|
| `CC0-1.0` | Creative Commons Zero 1.0 (domena publiczna) |
| `CC-BY-4.0` | Creative Commons Uznanie autorstwa 4.0 |

### `wojewodztwo`

| Kod | Opis |
|---|---|
| `dolnośląskie` | 02 |
| `kujawsko-pomorskie` | 04 |
| `lubelskie` | 06 |
| `lubuskie` | 08 |
| `łódzkie` | 10 |
| `małopolskie` | 12 |
| `mazowieckie` | 14 |
| `opolskie` | 16 |
| `podkarpackie` | 18 |
| `podlaskie` | 20 |
| `pomorskie` | 22 |
| `śląskie` | 24 |
| `świętokrzyskie` | 26 |
| `warmińsko-mazurskie` | 28 |
| `wielkopolskie` | 30 |
| `zachodniopomorskie` | 32 |

## Źródła i zakres referencyjny

Każdy rekord wskazuje źródło (`zrodlo_danych`, `url_zrodla`, `licencja_danych`, `zakres_referencyjny`). W metadanych jest lista źródeł z liczbą rekordów i osobnym plikiem CSV dla każdego źródła. Dane w prototypie są fikcyjne.
