# Wdrożenie wizytówki hacknation-2026.pl – krok po kroku

Wizytówka działa na VPS OVH `152.228.135.251` (ten sam co car-reminder), za Traefikiem z `~/car-reminder-deploy/edge`. Traefik sam wystawia certyfikaty Let's Encrypt dla `hacknation-2026.pl` i `www.hacknation-2026.pl`, a `www` przekierowuje na apex (301). W repo car-reminder `deploy` nic się nie zmienia: kontener zgłasza się etykietami w sieci `car-reminder-edge`.

| Host | Co |
|---|---|
| `hacknation-2026.pl` | wizytówka (`site/public/`) |
| `www.hacknation-2026.pl` | 301 → `https://hacknation-2026.pl` |
| `aed.hacknation-2026.pl` | prototyp (kolejny etap) |

Na serwerze: `~/hacknation-2026/` (paczka repo z `git archive`), kontener `hacknation-site` (busybox httpd, 16 MB RAM).

## Krok 1: test lokalny (Windows)

```powershell
cd C:\hacknation-2026\AED
docker compose -f site/docker-compose.yml up
```

- http://localhost:8080, http://localhost:8080/en/ i http://localhost:8080/nie-ma (strona 404);
- DevTools: szerokości 360, 768 i 1280 px, tryb ciemny (przełącznik ze słońcem albo Rendering → prefers-color-scheme), nawigacja klawiszem Tab, menu języka;
- Lighthouse: Accessibility i Best Practices ≥ 95.

## Krok 2: DNS (OVH → Web Cloud → Nazwy domen → hacknation-2026.pl → Strefa DNS)

1. Rekord A `@` (pusta subdomena) → `152.228.135.251`. Zastąp istniejący, jeśli wskazuje na `213.186.33.5`.
2. Rekord A `www` → `152.228.135.251`. Jeśli `www` jest dziś rekordem CNAME, usuń go i dodaj rekord A.
3. Rekord A `aed` → `152.228.135.251` (pod prototyp).
4. Usuń rekordy AAAA dla `@` i `www`, jeśli wskazują na hosting OVH: Let's Encrypt sprawdza IPv6 jako pierwsze.
5. Sprawdź (propagacja zwykle trwa do 1 h):
   ```powershell
   nslookup hacknation-2026.pl
   nslookup www.hacknation-2026.pl
   ```
   Oba mają zwracać `152.228.135.251`.

## Krok 3: Azure DevOps (jednorazowo)

Projekt i repo już istnieją: `https://xite-organization.visualstudio.com/AED/_git/AED` (remote `origin` w `C:\hacknation-2026\AED`).

1. **Projekt publiczny** (wymóg jury: repo publiczne przed niedzielą 11:00):
   - **Organization settings → Policies → Allow public projects: On**;
   - **projekt AED → Project settings → Overview → Visibility: Public**.
2. **Push:**
   ```powershell
   cd C:\hacknation-2026\AED
   git push -u origin main
   ```
3. Repo nie przechowuje sekretów, więc może być publiczne od razu.
4. **Project settings → Service connections → New → SSH:**
   - nazwa `vps-hacknation`, host `152.228.135.251`, port 22, użytkownik `ubuntu`;
   - klucz prywatny ten sam co w `vps-production` (projekt CarReminder) albo osobny klucz dopisany do `~/.ssh/authorized_keys` na VPS.

   Połączenia z innego projektu nie da się użyć, więc trzeba utworzyć nowe.
5. **Pipelines → Environments → New environment** `hacknation` (Resource: None), bez checków.
6. **Pipelines → New pipeline → Azure Repos Git → AED → Existing YAML → `/deploy/azure-pipelines.yml` → Run.**
7. Pierwszy run zatrzyma się na „needs permission”: **View → Permit** dla `vps-hacknation` i `hacknation`.
8. Sprawdź publiczny dostęp: link do repo w trybie incognito otwiera się bez logowania.

Projekt publiczny w Azure DevOps pokazuje bez logowania repo, pipeline'y i logi runów. Sekretów w repo nie ma, a klucz SSH jest tylko w service connection.

## Krok 4: pierwsze wdrożenie

Zwykle: commit + push do `main`, potem Azure DevOps → **Pipelines** → pipeline wizytówki → **Run**. Pipeline nie startuje sam. Ręcznie, bez pipeline'u:

```powershell
cd C:\hacknation-2026\AED
git archive --format=tar.gz -o $env:TEMP\hacknation.tar.gz HEAD site deploy
```

WinSCP: `hacknation.tar.gz` → `/home/ubuntu/deploy-in/`. Na VPS:

```bash
mkdir -p ~/hacknation-2026 && tar -xzf ~/deploy-in/hacknation.tar.gz -C ~/hacknation-2026
cd ~/hacknation-2026 && chmod +x deploy/scripts/*.sh
./deploy/scripts/deploy.sh up
./deploy/scripts/deploy.sh health
```

## Krok 5: weryfikacja

- `https://hacknation-2026.pl`: strona z kłódką;
- `https://www.hacknation-2026.pl` i `http://hacknation-2026.pl`: przekierowanie na `https://hacknation-2026.pl/`;
- `https://hacknation-2026.pl/en/` oraz `/nie-ma` (strona 404);
- nagłówki: `curl -sI https://hacknation-2026.pl` pokazuje `content-security-policy`, `strict-transport-security`, `x-content-type-options`;
- certyfikaty: `cd ~/car-reminder-deploy && ./scripts/edge.sh logs traefik`, bez błędów `Unable to obtain ACME certificate` dla nowych hostów (Ctrl+C);
- car-reminder działa bez zmian: `https://car-reminder.com`, `https://api.car-reminder.com/health`.

## Prototyp aed.hacknation-2026.pl

Prototyp ma trzy kontenery: `aed-postgres` (PostGIS), `aed-api` (.NET) i `aed-web` (Next.js), opisane w `deploy/stacks/app.yml`. Stoją za tym samym Traefikiem, z własnym certyfikatem dla `aed.hacknation-2026.pl`. Ścieżki `/api/v1`, `/scalar` i `/openapi` trafiają do API, reszta do web, który obsługuje też pobieranie otwartych danych (`/api/open-data/...`). Obrazy buduje pipeline, więc serwer nie ma kodu źródłowego.

### Pierwsze wdrożenie

1. **DNS:** rekord A `aed` → `152.228.135.251` (krok 2, punkt 3). Sprawdź: `nslookup aed.hacknation-2026.pl`.
2. **Commit + push** do `main`.
3. **Azure DevOps → Pipelines → pipeline `deploy` → Run.** Stage `Build` buduje obrazy `aed/api` i `aed/web` (pierwszy raz ok. 10 min), a stage `Deploy` kopiuje je na VPS i uruchamia wizytówkę oraz prototyp. W Azure DevOps nic nie trzeba konfigurować na nowo: to ten sam service connection `vps-hacknation` i environment `hacknation`.
4. Przy pierwszym `app up` skrypt tworzy `~/hacknation-2026/aed.env` z losowym hasłem bazy i kluczem JWT (`chmod 600`). Zapisz kopię w menedżerze haseł:

   ```bash
   cat ~/hacknation-2026/aed.env
   ```

   Konta panelu (`admin@aed.local` i konta demo) powstaną dopiero po wpisaniu `SEED_ADMIN_PASSWORD` / `SEED_DEMO_PASSWORD` w tym pliku i ponownym `app up`. Hasło musi mieć min. 10 znaków, w tym wielką i małą literę oraz cyfrę. Opis zmiennych jest w `deploy/stacks/aed.env.example`.

   Strona logowania (`/pl/sign-in`) ma wtedy „Szybkie wypełnienie”: Dysponent – Wrocław, Dysponent – OSP, Administrator. To te same hasła, więc jury loguje się jednym kliknięciem. Przy włączonym przełączniku hasła kont demo są widoczne dla każdego. Wyłącza go `DEMO_LOGIN=false` w `aed.env` i ponowne `app up`.

### Wdrożenie ręczne (bez pipeline'u)

```powershell
cd C:\hacknation-2026\AED\apps
.\tools\build-release.ps1
```

WinSCP: `apps\release\<wersja>\images.tar.gz` → `/home/ubuntu/deploy-in/`, a paczka `site` + `deploy` jak w kroku 4. Na VPS:

```bash
cd ~/hacknation-2026
./deploy/scripts/deploy.sh app load ~/deploy-in/images.tar.gz
./deploy/scripts/deploy.sh app up
./deploy/scripts/deploy.sh app health
```

### Weryfikacja

- `https://aed.hacknation-2026.pl/pl`: mapa z kłódką i pinezkami, „Najbliższe AED” (przeglądarka pyta o lokalizację);
- `https://aed.hacknation-2026.pl/pl/open-data`: pobieranie CSV, GeoJSON, JSON i XLSX;
- `https://aed.hacknation-2026.pl/api/open-data/catalog.rdf`: katalog DCAT-AP-PL z adresami `https://aed.hacknation-2026.pl/...`;
- `https://aed.hacknation-2026.pl/scalar`: dokumentacja API;
- wizytówka i car-reminder działają bez zmian.

## Obsługa

```bash
cd ~/hacknation-2026
./deploy/scripts/deploy.sh ps | logs | down | config                  # wizytówka
./deploy/scripts/deploy.sh app ps | app logs | app down | app config  # prototyp
./deploy/scripts/deploy.sh prune                                      # stare obrazy aed/*
```

- **Dane prototypu:** są w wolumenie `aed-postgres-data`. `app down` i kolejne wdrożenia go nie usuwają. Dane demo wgrywają się tylko do pustej bazy.
- **Zmiana kodu prototypu:** commit + push, potem **Run** pipeline'u. Pipeline buduje nowe obrazy i zostawia na serwerze tylko bieżącą wersję.
- **Zmiana treści wizytówki:** commit + push do `main`, potem **Run** pipeline'u. `deploy.sh up` za każdym razem tworzy kontener od nowa. Pipeline podmienia katalog `site/`, a działający kontener widziałby wtedy usunięty, pusty katalog (404 wszędzie).
- Gdy certyfikat się nie wystawia, najczęściej DNS jeszcze nie wskazuje na VPS albo jest stary rekord AAAA. Let's Encrypt pozwala na 5 nieudanych prób na godzinę dla hosta.
- Po hackathonie: `./deploy/scripts/deploy.sh down` i `./deploy/scripts/deploy.sh app down` zwalniają zasoby.
