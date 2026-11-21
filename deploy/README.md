# AED – deploy

Infrastruktura na VPS OVH `152.228.135.251`: kontenery za Traefikiem z Car Reminder (`~/car-reminder-deploy/edge`, porty 80/443, Let's Encrypt), skrypty i pipeline wdrożenia. Wzór układu: `C:\car-reminder\deploy`.

Krok po kroku (PL): [`WDROZENIE.md`](WDROZENIE.md).

| Ścieżka | Co |
|---|---|
| `stacks/site.yml` | Wizytówka `hacknation-2026.pl` (+ `www` → 301): busybox httpd serwuje `../site/public`, etykiety Traefika, nagłówki CSP |
| `stacks/httpd.conf` | Strona 404 i typy MIME dla busybox httpd |
| `stacks/app.yml` | (później) Prototyp `aed.hacknation-2026.pl`: postgres + api + web |
| `scripts/deploy.sh` | `deploy.sh up \| down \| health \| logs \| ps \| config` |
| `azure-pipelines.yml` | Uruchamiany ręcznie (Pipelines → Run): `git archive site deploy` → SSH → `deploy.sh up` (zawsze nowy kontener) / `health` |
| `edge/local/`, `scripts/local.sh` | Lokalny HTTPS: Traefik z własnym CA na `https://hacknation-2026.test` ([`edge/local/README.md`](edge/local/README.md)), wzór `C:\car-reminder\deploy\edge\local` |

- Sekretów w repo nie ma. Wizytówka ich nie potrzebuje, a przyszłe `stacks/*.env` będą istnieć tylko na VPS.
- Nowe hosty zgłaszają się Traefikowi etykietami w sieci `car-reminder-edge`, więc repo car-reminder `deploy` zostaje bez zmian.

Sprawdzenie przed commitem (bash z Dockerem):

```bash
docker compose -f deploy/stacks/site.yml config --quiet
docker compose -f deploy/edge/local/docker-compose.yml config --quiet
docker run --rm -v "$PWD:/mnt" -w /mnt koalaman/shellcheck:v0.10.0 deploy/scripts/*.sh
```
