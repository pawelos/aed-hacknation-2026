# AED – wizytówka (site)

Strona `https://hacknation-2026.pl` (+ `www` → 301). Statyczny HTML i CSS, bez WordPressa. Na serwer trafia tylko `public/`, a busybox httpd w Dockerze serwuje te pliki bez zmian.

Wygląd:
- **Nagłówek** jak na stronie marketingowej Car Reminder: logo, menu podstron, zielony przycisk „Zaloguj” (→ aplikacja w języku strony: `https://aed.hacknation-2026.pl/pl` lub `/en`; lokalnie `site.js` podmienia adres na `http://localhost:3060/<język>`, a na `*.test` na `https://aed.hacknation-2026.test/<język>`), język z flagą.
- **Ikony** w rozmiarach z aplikacji Car Reminder: przyciski 40 px, ikony 20 px, flaga 24 px. Lista języków ma pole „Szukaj…” i ✓ przy wybranym.
- **Strona główna** w układzie strony logowania: tekst i przycisk po lewej, ilustracja po prawej.
- **Podstrony** mają baner ze ścieżką „Start › Kontakt”.

## Strony

| Strona | PL | EN |
|---|---|---|
| Start | `/` | `/en/` |
| Rozwiązanie | `/rozwiazanie/` | `/en/solution/` |
| Dla kogo | `/dla-kogo/` | `/en/who-it-is-for/` |
| Materiały | `/materialy/` | `/en/materials/` |
| Zespół | `/zespol/` | `/en/team/` |
| Kontakt | `/kontakt/` | `/en/contact/` |

## Pliki

| Ścieżka | Co |
|---|---|
| `src/pl.json`, `src/en.json` | Teksty wspólne: menu, adresy podstron, napisy interfejsu, tytuły i opisy (SEO) każdej podstrony, tytuł i opis obrazu do udostępnień, podtytuły podstron |
| `src/pl/*.html`, `src/en/*.html` | Treść każdej strony (home, solution, audience, materials, team, contact) |
| `src/og/card.html`, `src/og/icon.html` | Szablony obrazu do udostępnień (1200×630) i ikony `apple-touch-icon` (180×180), nie są publikowane |
| `build.ps1` | Generator (PowerShell): wspólny nagłówek, baner i stopka, ikony i flagi SVG, meta tagi SEO i JSON-LD. Składa strony z `src/` w `public/**/index.html` i tworzy `public/sitemap.xml` |
| `og.ps1` | Renderuje `public/assets/og-pl.png`, `og-en.png` i `apple-touch-icon.png` z `src/og/` przez Edge w trybie headless |
| `public/**/index.html`, `public/sitemap.xml` | Wynik `build.ps1` (commitowany, nie edytować ręcznie) |
| `public/404.html` | Strona błędu (ustawiona w `../deploy/stacks/httpd.conf`, ścieżki absolutne), ręczna |
| `public/styles.css` | Style: tokeny kolorów, tryb jasny i ciemny, nagłówek, menu, mobile-first |
| `public/assets/site.js` | Jedyny skrypt: przełącznik motywu, wyszukiwarka języków, zamykanie menu (Esc, kliknięcie obok) |
| `public/assets/hero.svg` | Ilustracja strony głównej |
| `public/assets/logo.svg`, `favicon.svg` | Sygnet z księgi znaku, kopiowany przez `../brand/build.ps1` (nie edytować ręcznie) |
| `public/assets/aed-radar-prezentacja.pdf` | Prezentacja projektu (PDF), link na stronie „Materiały”; wstawiana ręcznie |
| `public/assets/brand/` | Księga znaku (PDF) i ZIP z logo, do pobrania ze strony „Materiały”; tworzy je `../brand/build.ps1` |
| `public/assets/og-*.png`, `apple-touch-icon.png` | Wynik `og.ps1` (commitowany) |
| `public/robots.txt` | SEO: zezwala na wszystko, wskazuje `sitemap.xml` |
| `docker-compose.yml` | Szybki podgląd po HTTP (`http://localhost:8080`) |

## Zmiana treści

1. Zmień tekst w `site/src/` (UTF-8, polskie znaki dozwolone).
2. Z katalogu głównego repo uruchom:
   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File site/build.ps1     # Windows PowerShell 5.1
   pwsh site/build.ps1                                                     # albo PowerShell 7
   ```
3. Zacommituj `site/src` i `site/public`.

Placeholdery w `src/*/*.html`:
- `{href:<strona>}`: link do podstrony w tym samym języku, np. `{href:contact}`;
- `{asset:hero.svg}`: ilustracja;
- `{icon:<nazwa>}`: ikona z `build.ps1` (`pin`, `upload`, `check`, `mail`, `code`, `calendar`…);
- `{app_url}`: adres prototypu.

Nieznany placeholder zatrzymuje generator z błędem.

Nowa podstrona to:
1. klucz w `$Pages` w `build.ps1`;
2. wpisy w `slugs`, `nav`, `titles`, `descriptions` i `leads` w obu plikach JSON;
3. pliki `src/pl/<klucz>.html` i `src/en/<klucz>.html`;
4. ponowne uruchomienie generatora.

## SEO

`build.ps1` dodaje do `<head>` każdej strony:
- `title` (do ok. 60 znaków) i `description` (ok. 110–200 znaków) z `titles` i `descriptions` w JSON, osobne dla każdej podstrony;
- `robots` (`index, follow, max-image-preview:large`), `canonical`, `hreflang` PL/EN i `x-default` (PL);
- Open Graph (`og:title`, `og:description`, `og:locale` + `og:locale:alternate`, `og:image` 1200×630 z tekstem alternatywnym) i `twitter:card` `summary_large_image` (resztę Twitter/X bierze z Open Graph);
- JSON-LD (schema.org): `Organization`, `WebSite` (z tematem `about`: AED), `WebPage`, na podstronach `BreadcrumbList`, a na stronie z `{faq}` także `FAQPage`;
- znaczniki weryfikacji Google Search Console i Bing Webmaster Tools, gdy wpiszesz je w `$SiteVerification` w `build.ps1` (puste są pomijane).

FAQ na stronie głównej (`{faq}` w `src/<lang>/home.html`) pochodzi z listy `faq` w JSON. Ten sam tekst trafia na stronę i do `FAQPage`, więc nie da się ich rozjechać. Pytania odpowiadają frazom, które ludzie wpisują w Google („jak znaleźć najbliższy defibrylator”, „kto może użyć AED”).

Frazy, pod które pisane są tytuły i opisy: *AED Radar*, *defibrylator AED*, *mapa defibrylatorów*, *najbliższy defibrylator*, *otwarte dane o defibrylatorach*, *dane.gov.pl AED*, *Misja AED*, *HackNation 2026*.

`sitemap.xml` ma `lastmod` z daty ostatniego commita, który zmienił `src/<lang>/<strona>.html` lub `src/<lang>.json` (przy niezacommitowanych zmianach: dzisiejsza data). `404.html` ma `noindex`.

Obraz do udostępnień zmieniasz w `src/og/card.html`, potem:
```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File site/og.ps1
```
Podgląd udostępnienia po wdrożeniu: [opengraph.xyz](https://www.opengraph.xyz), dane strukturalne: [Rich Results Test](https://search.google.com/test/rich-results). 

Po wdrożeniu (jednorazowo, ręcznie):
1. **Google Search Console** → „Dodaj usługę” → *Domena* `hacknation-2026.pl` → rekord DNS TXT u rejestratora (obejmuje też `www` i `aed.`). Alternatywa: *Prefiks URL* i znacznik HTML w `$SiteVerification`.
2. W Search Console: *Mapy witryn* → `sitemap.xml`; *Sprawdzanie adresu URL* → „Poproś o zindeksowanie” dla `/` i `/en/`.
3. **Bing Webmaster Tools** → import z Search Console (Bing zasila też DuckDuckGo i Ecosia).
4. **Linki do strony** (najważniejsze dla nowej domeny): zgłoszenie na HackNation, README repozytorium, profile LinkedIn zespołu, opis zbioru w dane.gov.pl, posty o projekcie.

`build.ps1` jest celowo w czystym ASCII. Windows PowerShell 5.1 czyta plik `.ps1` bez BOM jako ANSI, więc polskie teksty trzymamy w `src/`.

## Podgląd lokalny

- **HTTPS jak na serwerze** (Traefik, `https://hacknation-2026.test`): [`../deploy/edge/local/README.md`](../deploy/edge/local/README.md), potem `./deploy/scripts/local.sh up`.
- **W Aspire:** AppHost (`apps/api/Aed/AppHost`) uruchamia zasób `site` (ten sam kontener busybox) → `http://localhost:8080`, link jest w dashboardzie obok `web` i `api`.
- **Szybko po HTTP:** `docker compose -f site/docker-compose.yml up` → `http://localhost:8080`. Port jest ten sam co w Aspire, więc naraz może działać tylko jedno z nich.
- **Bez Dockera:** otwórz `site/public/index.html` z dysku (ścieżki są względne).

## Zasady

- **WCAG 2.2 AA:** kontrast co najmniej 4.5:1 dla tekstu w obu trybach, skip-link, widoczny `:focus-visible`, jeden `h1` na stronie, `alt` przy obrazach, `lang` przy tekstach w innym języku, `aria-current` w menu.
- **Działa bez JS.** Menu i lista języków to `<details>`, a motyw idzie za ustawieniem systemu. `site.js` tylko dodaje przełącznik, wyszukiwarkę i wygodne zamykanie.
- **Ścieżki względne.** `build.ps1` liczy prefiks `../` według głębokości strony, a linki kończą się na `index.html`, więc strona działa z serwera i z dysku. Wyjątek: `404.html`.
- **Bez CDN, inline skryptów i inline styli.** CSP `script-src 'self'; style-src 'self'` na VPS (`../deploy/stacks/site.yml`) i lokalnie (`../deploy/edge/local/routes.yml`). Wyjątek: `<script type="application/ld+json">` to blok danych, przeglądarka go nie wykonuje i CSP go nie blokuje.
- **Wdrożenie:** commit + push do `main`, pipeline `deploy` wdraża w ok. 1 min.
