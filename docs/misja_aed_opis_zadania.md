# MISJA AED – DANE NA RATUNEK

## Opis zadania

**MISJA AED – DANE NA RATUNEK** to zadanie konkursowe HackNation 2026, którego celem jest stworzenie rozwiązania ułatwiającego zebranie, standaryzację i udostępnienie w portalu **dane.gov.pl** ogólnopolskiego, otwartego oraz możliwie aktualnego zbioru danych o defibrylatorach AED.

Zadanie ma pomóc użytkownikom — zarówno służbom ratowniczym, jak i każdemu obywatelowi — szybko znaleźć najbliższy dostępny defibrylator w sytuacji zagrożenia życia. Obecnie dane o AED są rozproszone, niepełne i często niezweryfikowane, dlatego rozwiązanie powinno integrować informacje od różnych dysponentów danych w jednym, spójnym zbiorze.

## Cel rozwiązania

Rozwiązanie powinno:

- Zbierać dane o defibrylatorach AED z wielu rozproszonych źródeł.
- Wskazywać dla każdego rekordu źródło danych oraz jego referencyjny zakres.
- Integrować informacje od wszystkich dysponentów danych w jednym zbiorze.
- Projektować wzorcowy zakres danych do publikacji w portalu dane.gov.pl.
- Umożliwiać organizacjom posiadającym dane udostępnianie ich w jednolity sposób.
- Zachęcać dysponentów do dzielenia się danymi w portalu danych.
- Zawierać nazwę akcji, hasło oraz logotyp o potencjale promocyjnym.

## Użytkownicy rozwiązania

- Służby ratownicze.
- Organizacje i instytucje dysponujące defibrylatorami, np. samorządy, szkoły, firmy, OSP, centra handlowe.
- Każdy obywatel, który w sytuacji nagłego zatrzymania krążenia potrzebuje szybko znaleźć najbliższe dostępne AED.

## Oczekiwany rezultat

Oczekiwanym rezultatem jest rozwiązanie, które:

- Zbiera dane z wielu rozproszonych źródeł.
- Wskazuje zakres danych oraz źródło każdego rekordu.
- Umożliwia tworzenie otwartego, maszynowo odczytywalnego zbioru danych.
- Przygotowuje dane do publikacji w portalu dane.gov.pl.
- Zawiera hasło i logotyp akcji zachęcającej do udostępniania danych o defibrylatorach.

## Wymagane elementy zgłoszenia

Projekt przesyłany do oceny powinien zawierać:

- Szczegółowy opis i tytuł projektu.
- Prezentację zawierającą minimum 5 slajdów, w tym zrzuty ekranu jako dowody realizacji wymagań technicznych.
- Opis funkcjonalności oraz kroków użytkownika.
- Listę ewentualnych ograniczeń rozwiązania.
- Działający prototyp.
- Plik XLSX ze wzorcowym zakresem danych o defibrylatorach.
- Kod źródłowy.
- Logotyp w formatach `.jpg`, `.png`, `.pdf` oraz w dowolnym formacie otwartym.
- Księgę znaku w formacie `.pdf`.

## Wymagania techniczne

- Dane udostępniane w ramach rozwiązania muszą być w formacie odczytywalnym maszynowo.
- Niedopuszczalne są formaty takie jak PDF lub DOC dla danych — wyjątkiem są logotyp i księga znaku.
- Rozwiązanie musi być zgodne z zasadami działania portalu dane.gov.pl.
- Interfejs powinien spełniać aktualną wersję WCAG.
- Aplikacja powinna działać poprawnie na komputerach i urządzeniach mobilnych.
- Rozwiązanie powinno uwzględniać zasady UX.
- Można korzystać z AI, pod warunkiem że końcowe rozwiązanie jest efektem pracy człowieka i zespołu oraz że zespół posiada do niego wszelkie prawa.
- Zespół musi wskazać zakres wykorzystania AI przy opracowaniu rozwiązania.

## Dostępne zasoby

- Otwarty kod portalu dane.gov.pl: https://dane.gov.pl/source-code/
- Baza wiedzy portalu dane.gov.pl: https://dane.gov.pl/pl/knowledgebase/useful-materials

## Kryteria oceny

| Kryterium | Waga |
|---|---:|
| Zgodność z zasadami działania portalu dane.gov.pl | 30% |
| Kreatywność i innowacyjność rozwiązania | 15% |
| Zgodność z tematem wyzwania | 15% |
| Wzorcowy zakres danych o defibrylatorach | 10% |
| Prezentacja wypracowanego i kompletnego rozwiązania | 5% |
| Logotyp i hasło — proste, zrozumiałe, zgodne z tematem, nowoczesne, kreatywne i promocyjne | 25% |

## Sposób testowania

Rozwiązanie będzie testowane poprzez uruchomienie prototypu mechanizmu oraz weryfikację wymogów technicznych i funkcjonalności.

## Proponowany zakres MVP

### 1. Mapa i wyszukiwarka AED

- Mapa Polski z punktami defibrylatorów.
- Wyszukiwanie po adresie, mieście, gminie lub lokalizacji użytkownika.
- Filtrowanie po dostępności, statusie, właścicielu i rodzaju obiektu.
- Karta punktu AED z adresem, współrzędnymi, kontaktem, godzinami dostępności, źródłem danych i datą weryfikacji.

### 2. Panel dysponenta danych

- Formularz dodawania nowego defibrylatora.
- Możliwość aktualizacji danych istniejącego urządzenia.
- Wskazanie właściciela lub dysponenta danych.
- Dodanie źródła danych, adresu URL źródła, daty weryfikacji oraz licencji.
- Zgłaszanie poprawek lub nieaktualnych informacji.

### 3. Walidacja i integracja danych

- Import danych z plików CSV, GeoJSON lub API.
- Wykrywanie duplikatów.
- Walidacja współrzędnych, adresów, numerów telefonów, dat i statusów.
- Oznaczanie rekordów wymagających weryfikacji.
- Zachowanie informacji o źródle i zakresie referencyjnym danych.

### 4. Eksport danych

- Eksport danych do formatu CSV lub GeoJSON.
- Przygotowanie metadanych zgodnych z zasadami portalu dane.gov.pl.
- Dokumentacja struktury danych i sposobu ich aktualizacji.

### 5. Kampania społeczna

- Nazwa akcji, np. **„AED ODKRYTE”**.
- Hasło, np. **„Znajdź defibrylator. Odkryj dane. Uratuj życie.”**
- Nowoczesny, prosty i rozpoznawalny logotyp.
- Księga znaku obejmująca kolorystykę, pole ochronne, minimalną wielkość oraz niedozwolone wykorzystanie znaku.
- Krótki opis koncepcji kampanii, liczący maksymalnie 3–5 zdań.

## Proponowany schemat danych

Plik XLSX ze wzorcowym zakresem danych powinien zawierać m.in. następujące pola:

| Pole | Opis |
|---|---|
| `id_rekordu` | Unikalny identyfikator rekordu |
| `nazwa_lokalizacji` | Nazwa miejsca, w którym znajduje się AED |
| `adres` | Pełny adres lokalizacji |
| `gmina` | Gmina |
| `powiat` | Powiat |
| `wojewodztwo` | Województwo |
| `szerokosc_geograficzna` | Szerokość geograficzna |
| `dlugosc_geograficzna` | Długość geograficzna |
| `wlasiciel_dysponent` | Właściciel lub dysponent urządzenia |
| `rodzaj_obiektu` | Rodzaj miejsca, np. szkoła, urząd, sklep, OSP |
| `dostepnosc` | Informacja o dostępności urządzenia |
| `godziny_dostepnosci` | Godziny, w których AED jest dostępne |
| `status_urzadzenia` | Status, np. sprawny, do weryfikacji, nieaktualny |
| `telefon_kontaktowy` | Telefon kontaktowy do dysponenta |
| `zrodlo_danych` | Nazwa źródła danych |
| `url_zrodla` | Adres URL źródła danych |
| `data_ostatniej_weryfikacji` | Data ostatniej weryfikacji danych |
| `licencja_danych` | Licencja udostępnianych danych |
| `zakres_referencyjny` | Zakres, którego dotyczy rekord lub zbiór danych |

## Rekomendowana nazwa i hasło

**Nazwa akcji:** AED ODKRYTE

**Hasło:** „Znajdź defibrylator. Odkryj dane. Uratuj życie.”

**Opis koncepcji:**  
AED ODKRYTE to ogólnopolska akcja, która łączy rozproszone informacje o defibrylatorach w jeden otwarty i aktualny zbiór danych. Dzięki wspólnej pracy samorządów, instytucji, firm i organizacji każdy będzie mógł szybciej znaleźć dostępne AED w sytuacji zagrożenia życia. Udostępniając dane, pomagasz uratować czyjeś życie.

## Nagroda

Nagroda główna wynosi **27 777,78 zł brutto**. Jest jedna nagroda dla zespołu, którego rozwiązanie zostanie najwyżej ocenione przez jury. Wypłata następuje po podpisaniu umowy przeniesienia autorskich praw majątkowych na Ministra Cyfryzacji.

## Najważniejsze ryzyka

- Dane o AED są rozproszone, niekompletne i mogą być nieaktualne.
- Trzeba zapewnić wysoką jakość, spójność i możliwość aktualizacji danych.
- Kluczowa jest zgodność rozwiązania z zasadami portalu dane.gov.pl.
- Branding, czyli hasło i logotyp, stanowi 25% oceny i nie powinien być traktowany jako element drugorzędny.
- Rozwiązanie musi być dostępne, responsywne i zgodne z WCAG.

## Wniosek

MISJA AED to zadanie łączące **open data, integrację danych, mapę, UX oraz kampanię społeczną**. Najlepsze rozwiązanie nie będzie wyłącznie mapą defibrylatorów, lecz kompletnym mechanizmem umożliwiającym organizacjom łatwe przekazywanie, weryfikowanie i publikowanie danych o AED w portalu dane.gov.pl.
