# AED – wizytówka (site)

Strona `https://hacknation-2026.pl` (+ `www` → 301). Statyczny HTML i CSS, bez WordPressa. Na serwer trafia tylko `public/`, a busybox httpd w Dockerze serwuje te pliki bez zmian.

Wygląd:
- **Nagłówek** jak na stronie marketingowej Car Reminder: logo, menu podstron, zielony przycisk „Zaloguj” (→ `https://aed.hacknation-2026.pl/sign-in`), język z flagą.
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
| `src/pl.json`, `src/en.json` | Teksty wspólne: menu, adresy podstron, napisy interfejsu, tytuły, opis, podtytuły podstron |
| `src/pl/*.html`, `src/en/*.html` | Treść każdej strony (home, solution, audience, materials, team, contact) |
| `build.ps1` | Generator (PowerShell): wspólny nagłówek, baner i stopka, ikony i flagi SVG. Składa strony z `src/` w `public/**/index.html` i tworzy `public/sitemap.xml` |
| `public/**/index.html`, `public/sitemap.xml` | Wynik `build.ps1` (commitowany, nie edytować ręcznie) |
| `public/404.html` | Strona błędu (ustawiona w `../deploy/stacks/httpd.conf`, ścieżki absolutne), ręczna |
| `public/styles.css` | Style: tokeny kolorów, tryb jasny i ciemny, nagłówek, menu, mobile-first |
| `public/assets/site.js` | Jedyny skrypt: przełącznik motywu, wyszukiwarka języków, zamykanie menu (Esc, kliknięcie obok) |
| `public/assets/hero.svg` | Ilustracja strony głównej |
| `public/assets/logo.svg`, `favicon.svg` | Placeholder, do podmiany na logo z księgi znaku |
| `public/robots.txt` | SEO |
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
2. wpisy w `slugs`, `nav`, `titles` i `leads` w obu plikach JSON;
3. pliki `src/pl/<klucz>.html` i `src/en/<klucz>.html`;
4. ponowne uruchomienie generatora.

`build.ps1` jest celowo w czystym ASCII. Windows PowerShell 5.1 czyta plik `.ps1` bez BOM jako ANSI, więc polskie teksty trzymamy w `src/`.

## Podgląd lokalny

- **HTTPS jak na serwerze** (Traefik, `https://hacknation-2026.test`): [`../deploy/edge/local/README.md`](../deploy/edge/local/README.md), potem `./deploy/scripts/local.sh up`.
- **Szybko po HTTP:** `docker compose -f site/docker-compose.yml up` → `http://localhost:8080`.
- **Bez Dockera:** otwórz `site/public/index.html` z dysku (ścieżki są względne).

## Zasady

- **WCAG 2.2 AA:** kontrast co najmniej 4.5:1 dla tekstu w obu trybach, skip-link, widoczny `:focus-visible`, jeden `h1` na stronie, `alt` przy obrazach, `lang` przy tekstach w innym języku, `aria-current` w menu.
- **Działa bez JS.** Menu i lista języków to `<details>`, a motyw idzie za ustawieniem systemu. `site.js` tylko dodaje przełącznik, wyszukiwarkę i wygodne zamykanie.
- **Ścieżki względne.** `build.ps1` liczy prefiks `../` według głębokości strony, a linki kończą się na `index.html`, więc strona działa z serwera i z dysku. Wyjątek: `404.html`.
- **Bez CDN, inline skryptów i inline styli.** CSP `script-src 'self'; style-src 'self'` na VPS (`../deploy/stacks/site.yml`) i lokalnie (`../deploy/edge/local/routes.yml`).
- **Wdrożenie:** commit + push do `main`, pipeline `deploy` wdraża w ok. 1 min.
