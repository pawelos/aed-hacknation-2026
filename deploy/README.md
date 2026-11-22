# AED – deploy

Infrastruktura na VPS OVH `152.228.135.251`: kontenery za Traefikiem z Car Reminder (`~/car-reminder-deploy/edge`, porty 80/443, Let's Encrypt), skrypty i pipeline wdrożenia. Wzór układu: `C:\car-reminder\deploy`.

Krok po kroku (PL): [`WDROZENIE.md`](WDROZENIE.md).

| Ścieżka | Co |
|---|---|
| `stacks/site.yml` | Wizytówka `hacknation-2026.pl` (+ `www` → 301): busybox httpd serwuje `../site/public`, etykiety Traefika, nagłówki CSP |
| `stacks/httpd.conf` | Strona 404 i typy MIME dla busybox httpd |
| `stacks/app.yml` | Prototyp `aed.hacknation-2026.pl`: PostGIS + API + web, etykiety Traefika (`/api/v1`, `/scalar`, `/openapi` → API, reszta → web) |
| `stacks/aed.env.example` | Opis zmiennych prototypu. Prawdziwy plik `~/hacknation-2026/aed.env` istnieje tylko na VPS: tworzy go `deploy.sh` z losowymi sekretami |
| `scripts/deploy.sh` | Wizytówka: `up \| down \| health \| logs \| ps \| config`. Prototyp: `app load <images.tar.gz> \| app up \| app down \| app health \| app logs \| app ps \| app config`. Do tego `prune` |
| `azure-pipelines.yml` | Uruchamiany ręcznie (Pipelines → Run). Stage `Build`: `apps/tools/build-release.ps1` (obrazy `aed/api`, `aed/web`). Stage `Deploy`: `git archive site deploy` + obrazy → SSH → `deploy.sh up` / `health`, `app load` / `app up` / `app health` |
| `edge/local/`, `scripts/local.sh` | Lokalny HTTPS: Traefik z własnym CA na `https://hacknation-2026.test` ([`edge/local/README.md`](edge/local/README.md)), wzór `C:\car-reminder\deploy\edge\local` |

- Sekretów w repo nie ma. Wizytówka ich nie potrzebuje, a sekrety prototypu są tylko w `~/hacknation-2026/aed.env` na VPS.
- Nowe hosty zgłaszają się Traefikowi etykietami w sieci `car-reminder-edge`, więc repo car-reminder `deploy` zostaje bez zmian.

Sprawdzenie przed commitem (bash z Dockerem):

```bash
docker compose -f deploy/stacks/site.yml config --quiet
printf 'POSTGRES_PASSWORD=x\nJWT_SECRET=y\n' > /tmp/aed.env && APP_VERSION=test docker compose --env-file /tmp/aed.env -f deploy/stacks/app.yml config --quiet
docker compose -f deploy/edge/local/docker-compose.yml config --quiet
docker run --rm -v "$PWD:/mnt" -w /mnt koalaman/shellcheck:v0.10.0 deploy/scripts/*.sh
```
