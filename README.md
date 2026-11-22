# AED Radar – Misja AED: Dane na ratunek

**Udostępnij dane. Daj komuś szansę.**

Projekt zespołu na HackNation 2026, zadanie Ministerstwa Cyfryzacji „Misja AED – Dane na ratunek”. Każdy dysponent AED publikuje w portalu [dane.gov.pl](https://dane.gov.pl) własny wykaz defibrylatorów i odpowiada za jego jakość. Wykazy wszystkich dostawców tworzą jedną ogólnopolską listę. Dla kogo: służby ratownicze, dysponenci AED (samorządy, szkoły, firmy, OSP) i każdy, kto w nagłej sytuacji szuka najbliższego defibrylatora.

## Problem i model rozwiązania

**Problem:** dane o AED są rozproszone i niezweryfikowane, bo nikt nie bierze za nie odpowiedzialności. W dane.gov.pl są dziś 4 zbiory AED (Krosno, Przemyśl, Marki, Warszawa), każdy w innym układzie kolumn. Plik z Marek ma zamienione współrzędne. Listy prowadzone przez osoby trzecie nie mają właściciela, który poprawi błąd.

**Model:** robimy dla AED to, co Ministerstwo Cyfryzacji zrobiło już dla **wykazu rzeczy znalezionych**. Tam każde starostwo jest osobnym dostawcą w portalu i publikuje swój wykaz według wzorcowego zakresu danych przez automatyczny import XML.
1. **Wzorcowy zakres danych AED:** XLSX, CSV i opis pól.
2. **Dostawca = dysponent.** Samorząd, szkoła, firma lub OSP prowadzi rekordy w panelu albo importuje plik. Jako instytucja w dane.gov.pl odpowiada za swój wykaz.
3. **Automatyczny import XML.** Prototyp generuje dla każdego dostawcy plik `aed-<id źródła>.xml` ze schematem portalu `urn:otwarte-dane:harvester:1.13` i sumą `.md5`. Dostawca rejestruje ten adres w portalu, a portal sam pobiera wykaz. Prototyp nie publikuje danych w imieniu innych.
4. **Jedna lista:** wykazy dostawców łączą się w zbiór ogólnopolski (`aed.xml`) z deduplikacją i źródłem przy każdym rekordzie.

**Hasło i logotyp** („Udostępnij dane. Daj komuś szansę.”) służą akcji promocyjnej skierowanej do potencjalnych dostawców danych, czyli dysponentów AED. Nie są elementem prezentacji rozwiązania.

## Linki

| Co | Adres |
|---|---|
| **Instrukcja dla jury** | [`INSTRUKCJA-DLA-JURY.md`](INSTRUKCJA-DLA-JURY.md): kroki od 1 do 15 i gdzie jest każde wymaganie |
| Wizytówka | https://hacknation-2026.pl |
| Repozytorium | https://github.com/pawelos/aed-hacknation-2026 |
| Prototyp | https://aed.hacknation-2026.pl (mapa AED, otwarte dane: `/pl/open-data`, API: `/scalar`) |
| Demo (wideo, do 2 min) | wkrótce |
| Zadanie | https://hacknation.pl/zadania |

## Struktura repo

Jedno repo, pięć części, jak w Car Reminder (tam to osobne repo `apps`, `deploy` i `wordpress`):

| Ścieżka | Co | Pipeline |
|---|---|---|
| [`apps/`](apps/README.md) | Prototyp na `aed.hacknation-2026.pl`: API .NET, web Next.js, dane, testy E2E (Playwright) | `apps/azure-pipelines.yml` (sam build obrazów) |
| [`deploy/`](deploy/README.md) | Infrastruktura VPS: compose za Traefikiem, skrypty, [instrukcja wdrożenia](deploy/WDROZENIE.md) | `deploy/azure-pipelines.yml` (build obrazów i wdrożenie wizytówki oraz prototypu, ręczny Run w Azure DevOps) |
| [`site/`](site/README.md) | Wizytówka `hacknation-2026.pl`: statyczny HTML i CSS (PL/EN) w `site/public/` | wdrażana przez pipeline `deploy` |
| [`brand/`](brand/README.md) | Logotyp (SVG, PNG, JPG, PDF) i księga znaku (PDF) akcji | `brand/build.ps1` (ręcznie) |
| [`portal/`](portal/hacknation/README.md) | Kod portalu dane.gov.pl (backend 2.63.0, GPL v3) dodany bez zmian, a w `portal/hacknation/` nasze przyrosty: uruchomienie na localhost i import wykazów AED importerem portalu | ręcznie, Docker Compose |
| `docs/` | Opis zadania, ustalenia z Discorda ([`ustalenia_mentorow_aed.md`](docs/ustalenia_mentorow_aed.md)) i analiza kodu portalu ([`dane_gov_pl_backend.md`](docs/dane_gov_pl_backend.md)) | – |

## Jak uruchomić lokalnie

Wizytówka (wymaga Dockera):

```bash
git clone https://github.com/pawelos/aed-hacknation-2026.git
cd aed-hacknation-2026
docker compose -f site/docker-compose.yml up
```

Potem otwórz http://localhost:8080. To ten sam kontener (busybox httpd) co na serwerze. Bez Dockera wystarczy otwarcie `site/public/index.html` z dysku.

HTTPS lokalnie, jak na serwerze (`https://hacknation-2026.test`, Traefik z własnym CA): [`deploy/edge/local/README.md`](deploy/edge/local/README.md).

Treści stron są w `site/src/`. Po zmianie uruchom `powershell -NoProfile -ExecutionPolicy Bypass -File site/build.ps1` ([`site/README.md`](site/README.md)).

Prototyp: [`apps/README.md`](apps/README.md).

## Co działa

**Prototyp** (`aed.hacknation-2026.pl`, szczegóły w [`apps/README.md`](apps/README.md)):
- **Mapa i wyszukiwarka AED:**
  - mapa Polski z grupowaniem punktów;
  - wyszukiwanie po adresie i miejscowości oraz „Najbliższe AED” z lokalizacji telefonu;
  - filtry: dostępność, rodzaj miejsca, „dostępne teraz”;
  - karta punktu: adres, współrzędne, godziny (odczyt po polsku z zapisu OpenStreetMap), kontakt, źródło danych, licencja, data weryfikacji, przycisk „Nawiguj”;
  - lista wyników jako alternatywa dla mapy.
- **Zgłoszenie poprawki** z karty AED, bez logowania, z limitem i zabezpieczeniem przed botami.
- **Panel dysponenta** (logowanie z rolami, każdy dysponent widzi tylko swoje dane):
  - pulpit z liczbami wymagającymi uwagi;
  - lista rekordów z wyszukiwaniem, filtrami i zaznaczaniem wielu rekordów (usuń i potwierdź aktualność jednym zapytaniem);
  - dodawanie i edycja w szufladzie z walidacją pod polami;
  - rozpatrywanie zgłoszeń.
- **Import od dysponentów:**
  - CSV (przecinek lub średnik, UTF-8 lub Windows-1250) i GeoJSON, z pliku albo z adresu URL;
  - sprawdzenie bez zapisu i raport z wynikiem dla każdego wiersza;
  - wykrywanie duplikatów z innych źródeł;
  - oznaczanie rekordów do weryfikacji;
  - historia importów.
- **Otwarte dane dla dane.gov.pl:**
  - cały zbiór w CSV, GeoJSON i JSON pod stałymi adresami;
  - plik automatycznego importu dane.gov.pl (XML 1.13 + MD5) dla całego zbioru (`/api/open-data/aed.xml`) i osobno dla każdego dostawcy (`/api/open-data/aed-<id źródła>.xml`), sprawdzany w testach schematem XSD z kodu portalu;
  - metadane i katalog DCAT-AP-PL 2.1;
  - wzorcowy zakres danych w XLSX ([`apps/data/wzorcowy-zakres-danych-aed.xlsx`](apps/data/wzorcowy-zakres-danych-aed.xlsx));
  - dla każdego rekordu źródło i zakres referencyjny.
- **Publiczne API** z dokumentacją (`/scalar`).
- **Dla wszystkich ekranów:**
  - PL/EN i tryb ciemny;
  - responsywność od 320 px;
  - WCAG 2.2 AA: kontrast, obsługa klawiaturą, etykiety i błędy powiązane z polami, komunikaty dla czytników ekranu, brak animacji przy ograniczonym ruchu.

**Wizytówka** (`hacknation-2026.pl`):
- strony PL/EN: Start, Rozwiązanie, Dla kogo, Materiały, Zespół, Kontakt, z przyciskiem „Zaloguj” do aplikacji;
- wybór języka z wyszukiwarką, tryb jasny i ciemny, WCAG 2.2 AA.

**Wdrożenie:**
- Docker Compose za Traefikiem na VPS, z certyfikatem Let's Encrypt;
- pipeline w Azure DevOps;
- lokalny HTTPS (`https://hacknation-2026.test`).

## Kroki użytkownika

**Obywatel w nagłej sytuacji** (bez logowania):
1. Otwiera `aed.hacknation-2026.pl` na telefonie i wybiera „Najbliższe AED”, co wymaga zgody na lokalizację. Może też wpisać adres lub miejscowość.
2. Widzi najbliższe punkty na mapie albo na liście, z odległością i informacją, czy AED jest teraz dostępne.
3. Otwiera kartę punktu: sprawdza, gdzie dokładnie jest urządzenie i jak się do niego dostać, potem wybiera „Nawiguj”. W każdej chwili może użyć przycisku „Zadzwoń 112”.
4. Jeśli dane są nieaktualne, wybiera „Zgłoś poprawkę” na karcie.

**Dysponent AED** (np. urząd, szkoła, OSP):
1. Loguje się przez „Panel” albo „Zaloguj” na wizytówce i widzi pulpit: ile rekordów czeka na weryfikację i ile jest nowych zgłoszeń.
2. **Import:** wybiera źródło i plik CSV lub GeoJSON, wybiera „Sprawdź plik”, czyta raport (nowe, zaktualizowane, odrzucone, możliwe duplikaty), potem „Importuj”.
3. **Rekordy AED:** dodaje lub poprawia defibrylator w szufladzie albo zaznacza kilka rekordów i potwierdza ich aktualność.
4. **Zgłoszenia:** przyjmuje lub odrzuca poprawki zgłoszone z mapy. Przyjęte oznaczają rekord do weryfikacji.

**Służby, analitycy i portal dane.gov.pl:**
1. Pobierają zbiór ze strony „Otwarte dane” (`/pl/open-data`): CSV, GeoJSON albo JSON, z filtrem województwa lub źródła.
2. Dysponent jako instytucja w dane.gov.pl rejestruje źródło importu XML ze swoim adresem `aed-<id źródła>.xml` (podany w `metadata.json`). Portal pobiera plik, sprawdza sumę MD5 i schemat, a potem tworzy lub aktualizuje wykaz dostawcy. Każdy rekord ma źródło, licencję, zakres referencyjny i datę weryfikacji.

## Ograniczenia

Świadome uproszczenia prototypu opisuje [`apps/README.md`](apps/README.md#ograniczenia-świadome-uproszczenia-prototypu). Najważniejsze:
- plik importu XML sprawdziliśmy na portalu dane.gov.pl uruchomionym lokalnie z jego kodu (backend 2.63.0, folder [`portal/`](portal/hacknation/README.md)). Importer portalu przyjął wykazy 2 dostawców i zbudował tabele z CSV (12/12 i 6/6 wierszy). Lokalnie zastąpiliśmy wewnętrzny geokoder portalu (TERYT), którego nie ma w publicznym kodzie. Katalog DCAT-AP-PL nie był pobierany przez portal;
- łączenie wykazów wielu dostawców w jedną listę robi dziś prototyp, nie sam portal;
- konta zakłada seeder lub administrator (brak rejestracji i e-maili);
- uprawnienia oparte tylko na rolach;
- token ważny 12 h bez odświeżania;
- testy E2E (Playwright) uruchamia się ręcznie na działającym prototypie, pipeline ich nie uruchamia.

## Technologie

| Część | Technologia |
|---|---|
| **API** | .NET 10, ASP.NET Core, EF Core, PostgreSQL + PostGIS (NetTopologySuite), Mediator, FluentValidation, Scalar |
| **Web** | Next.js 16, React 19, next-intl (PL/EN), Tailwind CSS 4, Leaflet + OpenStreetMap |
| **Wizytówka** | HTML5 i CSS bez frameworków, busybox httpd |
| **Infrastruktura** | Docker Compose, Traefik v3, Let's Encrypt, Azure Pipelines, .NET Aspire (lokalnie) |
| **Testy** | NUnit (API), `node --test` (web), Playwright (E2E, [`apps/e2e`](apps/e2e/README.md)) |

## Wykorzystanie AI

Zgodnie z regulaminem zadania (§2 ust. 8) opisujemy zakres użycia AI. Zespół korzystał z asystenta AI (Claude Code). Zespół wybierał zakres zmian, przeglądał kod i testował wynik:
- **Wizytówka i wdrożenie:** szkic kodu HTML/CSS, konfiguracja Docker Compose, Traefika i pipeline'ów.
- **Prototyp:**
  - kod API (moduły rekordów, importu, otwartych danych, operacje zbiorcze) i testy jednostkowe;
  - kod web: mapa, karta AED, panel dysponenta, formularze z walidacją, poprawki dostępności (WCAG) i responsywności.
- **Analiza** otwartego kodu portalu dane.gov.pl (metadane, katalog DCAT-AP-PL, import XML).
- **Zgodność z dane.gov.pl:**
  - eksport plików importu XML (`aed.xml`, `aed-<id źródła>.xml`) z testami na schemacie XSD portalu;
  - uruchomienie portalu lokalnie w `portal/hacknation/` (konfiguracja, zastępczy geokoder, skrypt importu, zrzuty ekranu).
- **Znak akcji:** szkic logotypu (sygnet z serca i łuków radaru), generator plików SVG/PNG/JPG/PDF i księga znaku w `brand/`.
- **Dokumentacja:** README, opis schematu danych i importu, instrukcja dla jury, szkic prezentacji.

Koncepcję rozwiązania, wzorcowy zakres danych, nazwę i hasło akcji, wybór kierunku znaku, decyzje projektowe oraz weryfikację wszystkich wyników wykonał zespół, który ma do rozwiązania pełne prawa (tak samo opisuje to strona „Zespół” na wizytówce).
