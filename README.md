# AED Radar – Misja AED: Dane na ratunek

**Udostępnij dane. Daj komuś szansę.**

Projekt zespołu na HackNation 2026, zadanie Ministerstwa Cyfryzacji „Misja AED – Dane na ratunek”. Łączymy rozproszone dane o defibrylatorach AED w jeden otwarty, maszynowo czytelny zbiór gotowy do publikacji w portalu [dane.gov.pl](https://dane.gov.pl). Dla kogo: służby ratownicze, dysponenci AED (samorządy, szkoły, firmy, OSP) i każdy, kto w nagłej sytuacji szuka najbliższego defibrylatora.

## Linki

| Co | Adres |
|---|---|
| Wizytówka | https://hacknation-2026.pl |
| Prototyp | https://aed.hacknation-2026.pl (wkrótce) |
| Demo (wideo, do 2 min) | wkrótce |
| Zadanie | https://hacknation.pl/zadania |

## Struktura repo

Jedno repo, trzy części, jak w Car Reminder (tam to osobne repo `apps`, `deploy` i `wordpress`):

| Ścieżka | Co | Pipeline |
|---|---|---|
| [`apps/`](apps/README.md) | Prototyp na `aed.hacknation-2026.pl`: API .NET, web Next.js, dane, testy e2e (szkielet) | `apps/azure-pipelines.yml` (build obrazów, później) |
| [`deploy/`](deploy/README.md) | Infrastruktura VPS: compose za Traefikiem, skrypty, [instrukcja wdrożenia](deploy/WDROZENIE.md) | `deploy/azure-pipelines.yml` (wdrożenie po pushu do `main`) |
| [`site/`](site/README.md) | Wizytówka `hacknation-2026.pl`: statyczny HTML i CSS (PL/EN) w `site/public/` | wdrażana przez pipeline `deploy` |
| `docs/` | Opis zadania i ustalenia z Discorda | – |

## Jak uruchomić lokalnie

Wizytówka (wymaga Dockera):

```bash
git clone <adres-repo>
cd AED
docker compose -f site/docker-compose.yml up
```

Potem otwórz http://localhost:8080. To ten sam kontener (busybox httpd) co na serwerze. Bez Dockera wystarczy otwarcie `site/public/index.html` z dysku.

HTTPS lokalnie, jak na serwerze (`https://hacknation-2026.test`, Traefik z własnym CA): [`deploy/edge/local/README.md`](deploy/edge/local/README.md).

Treści stron są w `site/src/`. Po zmianie uruchom `powershell -NoProfile -ExecutionPolicy Bypass -File site/build.ps1` ([`site/README.md`](site/README.md)).

Prototyp: [`apps/README.md`](apps/README.md).

## Co działa

- Wizytówka PL/EN w stylu Car Reminder:
  - menu podstron (Start, Rozwiązanie, Dla kogo, Materiały, Zespół, Kontakt) i przycisk „Zaloguj” do aplikacji;
  - wybór języka z wyszukiwarką, tryb jasny i ciemny;
  - responsywna i zgodna z WCAG 2.2 AA (kontrast, skip-link, focus, semantyka).
- Lokalny HTTPS przez Traefik (`https://hacknation-2026.test`).
- Automatyczne wdrożenie z Azure DevOps na VPS z certyfikatem Let's Encrypt.

## Technologie

- HTML5 i CSS bez frameworków, busybox httpd, Traefik v3, Docker Compose, Azure Pipelines.

## Wykorzystanie AI

Zgodnie z regulaminem zadania (§2 ust. 8) opisujemy zakres użycia AI. Zespół korzystał z asystenta AI (Claude Code) przy szkicu kodu wizytówki, konfiguracji wdrożenia i dokumentacji. Koncepcję, treści, decyzje projektowe i weryfikację wykonał zespół.
