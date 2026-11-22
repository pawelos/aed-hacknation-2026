# Instrukcja dla jury – AED Radar

Wszystko sprawdzisz w przeglądarce, bez instalowania czegokolwiek. Czas: około 15 minut. Dane w prototypie są fikcyjne.

| Co | Adres |
|---|---|
| Prototyp | https://aed.hacknation-2026.pl |
| Wizytówka akcji | https://hacknation-2026.pl |
| Otwarte dane | https://aed.hacknation-2026.pl/pl/open-data |
| Kod źródłowy | to repozytorium (opis w [`README.md`](README.md)) |

Konta demo: na stronie logowania kliknij przełącznik „Dysponent – Wrocław” albo „Administrator”, a potem „Zaloguj”. Hasła wpisują się same.

## A. Ścieżka obywatela (bez logowania, najlepiej na telefonie)

1. Otwórz https://aed.hacknation-2026.pl. Widzisz mapę Polski z punktami AED.
2. Wybierz **„Najbliższe AED”** i zezwól na lokalizację. Możesz też wpisać adres, np. „Wrocław, Rynek”.
3. Włącz filtr **„Tylko dostępne teraz”**. Zostają tylko AED, które są teraz dostępne według godzin otwarcia.
4. Otwórz kartę punktu. Sprawdź: adres, współrzędne, godziny, sposób dostępu, **źródło danych, licencję i datę weryfikacji**, przycisk „Nawiguj” i „Zadzwoń 112”.
5. Na karcie wybierz **„Zgłoś poprawkę”** i wyślij krótkie zgłoszenie. Nie trzeba się logować.
6. Przełącz widok na **listę** (alternatywa dla mapy), język na EN i tryb ciemny.

## B. Ścieżka dysponenta AED (dostawcy danych)

7. Wejdź na https://aed.hacknation-2026.pl/pl/sign-in i zaloguj się jako **„Dysponent – Wrocław”**. Pulpit pokazuje, ile rekordów czeka na weryfikację i ile jest nowych zgłoszeń.
8. **Rekordy AED:** wybierz „Dodaj defibrylator”, zostaw puste pole obowiązkowe i zobacz błąd pod polem. Zaznacz kilka rekordów i wybierz „Potwierdź aktualność”.
9. **Import:** wybierz źródło i plik [`apps/data/import-przyklad-wroclaw.csv`](apps/data/import-przyklad-wroclaw.csv), potem „Sprawdź plik”. Raport pokazuje dla każdego wiersza: nowy, zaktualizowany, odrzucony albo możliwy duplikat. Nic się nie zapisuje do czasu „Importuj”.
10. **Zgłoszenia:** znajdziesz tu poprawkę z kroku 5. Przyjmij ją, a rekord zostanie oznaczony do weryfikacji.
11. Wyloguj się i zaloguj jako **„Dysponent – OSP”**. Widzisz tylko rekordy OSP, bez danych Wrocławia: każdy dostawca odpowiada tylko za swój wykaz.

## C. Otwarte dane i zgodność z dane.gov.pl

12. Otwórz https://aed.hacknation-2026.pl/pl/open-data. Pobierz zbiór w **CSV, GeoJSON lub JSON**, także z filtrem województwa albo źródła.
13. Otwórz [`metadata.json`](https://aed.hacknation-2026.pl/api/open-data/metadata.json). Lista `sources` pokazuje każde źródło z licencją, **zakresem referencyjnym** i liczbą rekordów.
14. **Plik automatycznego importu dane.gov.pl** (XML 1.13 + MD5), ten sam mechanizm co w wykazie rzeczy znalezionych:
    - cały zbiór: https://aed.hacknation-2026.pl/api/open-data/aed.xml i `aed.md5`;
    - wykaz jednego dostawcy: adres `harvesterUrl` przy źródle w `metadata.json` (`aed-<id źródła>.xml`).
    Zgodność ze schematem portalu sprawdza test [`HarvesterXmlWriterTests.cs`](apps/api/Aed/Application.Test/Modules/OpenData/Harvester/HarvesterXmlWriterTests.cs), który używa pliku XSD z kodu dane.gov.pl.
    **Sprawdzone na prawdziwym portalu:** uruchomiliśmy dane.gov.pl lokalnie z jego kodu źródłowego (backend 2.63.0). Importer XML portalu pobrał wykazy dwóch dostawców z adresów `aed-<id źródła>.xml`. Utworzył dwa opublikowane zbiory z zasobami CSV, GeoJSON i JSON, a z CSV zbudował tabelę danych: 12 z 12 wierszy i 6 z 6, z oceną 4★. Test wykrył jeden błąd: format `data_aktualizacji`, którego portal nie przyjmował. Poprawiliśmy go. Kod portalu jest w [`portal/`](portal/) (pierwszy commit bez zmian), a nasze przyrosty w [`portal/hacknation/`](portal/hacknation/README.md): kroki uruchomienia, zmiany środowiska i wynik.
15. Katalog DCAT-AP-PL 2.1: [`catalog.rdf`](https://aed.hacknation-2026.pl/api/open-data/catalog.rdf). Publiczne API z dokumentacją: https://aed.hacknation-2026.pl/scalar.

## D. Gdzie jest każde wymaganie zadania

| Wymaganie z treści zadania | Gdzie sprawdzić |
|---|---|
| Zbieranie danych z wielu rozproszonych źródeł | Krok 9 (import CSV/GeoJSON z pliku lub URL), krok 12 (filtr źródła); opis: [`apps/docs/import-danych.md`](apps/docs/import-danych.md) |
| Źródło i zakres referencyjny każdego rekordu | Krok 4 (karta AED), krok 13 (`sources` w metadanych), kolumny `zrodlo`, `url_zrodla`, `licencja` w CSV |
| Integracja danych wszystkich dysponentów w jednym zbiorze | Krok 9 (wykrywanie duplikatów), krok 14 (`aed.xml` całego zbioru) |
| Wzorcowy zakres danych dla dane.gov.pl (XLSX) | [`wzorcowy-zakres-danych-aed.xlsx`](https://aed.hacknation-2026.pl/api/open-data/wzorcowy-zakres-danych-aed.xlsx), kopia w [`apps/data/`](apps/data/wzorcowy-zakres-danych-aed.xlsx); opis pól: [`apps/docs/schemat-danych.md`](apps/docs/schemat-danych.md) |
| Jednolity sposób udostępniania danych przez organizacje | Kroki 7–11 (panel dysponenta), krok 14 (osobny plik importu XML dla każdego dostawcy) |
| Zachęcanie dysponentów do dzielenia się danymi | Wizytówka https://hacknation-2026.pl, strona „Dla kogo” |
| Nazwa akcji, hasło, logotyp | Hasło „Udostępnij dane. Daj komuś szansę.” na wizytówce. Logotyp: [`brand/src`](brand/src) (SVG), [`brand/logo`](brand/logo) (PNG, JPG, `logo.pdf`). Księga znaku: [`brand/ksiega-znaku.pdf`](brand/ksiega-znaku.pdf). Do pobrania także na wizytówce, strona „Materiały” |
| Dane w formacie odczytywalnym maszynowo (bez PDF/DOC) | Krok 12: CSV, GeoJSON, JSON; krok 14: XML |
| Zgodność z zasadami portalu dane.gov.pl | Krok 14: format importu XML 1.13 z kodu portalu, MD5, https oraz import przez portal uruchomiony lokalnie z jego kodu (2 zbiory, tabele 12/12 i 6/6 wierszy). Krok 15: DCAT-AP-PL. Analiza kodu portalu: [`docs/dane_gov_pl_backend.md`](docs/dane_gov_pl_backend.md) |
| WCAG, komputer i telefon, UX | Krok 6; każdy ekran od 320 px, obsługa klawiaturą (Tab), etykiety i błędy pod polami (krok 8) |
| Opis funkcji i kroków użytkownika, ograniczenia | [`README.md`](README.md): „Co działa”, „Kroki użytkownika”, „Ograniczenia” |
| Zakres wykorzystania AI | [`README.md`](README.md#wykorzystanie-ai) |
| Prezentacja (min. 5 slajdów) i wideo demo | **do uzupełnienia** |
