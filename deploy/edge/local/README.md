# Lokalny HTTPS (Traefik na Windows)

Traefik z HTTPS przed wizytówką, na adresach podobnych do serwera. Wzór: `C:\car-reminder\deploy\edge\local`.

| Adres | Dokąd |
|---|---|
| `https://hacknation-2026.test` | wizytówka (`site/public`, ten sam kontener busybox co na VPS) |
| `https://www.hacknation-2026.test` | 301 → `https://hacknation-2026.test` |
| `https://aed.hacknation-2026.test` | prototyp `apps/web` uruchomiony na tym komputerze przez Aspire (port 3060; bez niego 502) |

Certyfikat wystawia własne lokalne CA. Traefik wysyła te same nagłówki CSP co produkcja, więc zablokowany skrypt albo styl widać od razu lokalnie.

## Jednorazowo

1. Certyfikat. Pliki trafiają do `deploy/edge/local/certs/` (w `.gitignore`). W Git Bash z katalogu głównego repo:
   ```bash
   ./deploy/scripts/local.sh certs
   ```
2. Zaufaj CA dla swojego użytkownika Windows. Windows poprosi o potwierdzenie; Chrome, Edge i Firefox korzystają z tego magazynu. W PowerShell:
   ```powershell
   cd C:\hacknation-2026\AED
   certutil -user -addstore Root deploy\edge\local\certs\ca.crt
   ```
3. Plik hosts `C:\Windows\System32\drivers\etc\hosts` (Notatnik jako Administrator), jedna linia:
   ```
   127.0.0.1 hacknation-2026.test www.hacknation-2026.test aed.hacknation-2026.test
   ```

## Codziennie

```bash
./deploy/scripts/local.sh up        # start (Docker Desktop musi działać)
./deploy/scripts/local.sh down      # stop
./deploy/scripts/local.sh logs      # logi Traefika
```

Potem otwórz `https://hacknation-2026.test`. Zmiany w `site/public` widać po odświeżeniu strony, bez restartu.

**Porty 80/443** zajmuje też lokalny Traefik Car Reminder (`car-reminder-local-traefik`). Działać może jeden naraz:
- zatrzymaj tamten: `cd C:\car-reminder\deploy && ./scripts/local.sh down`;
- albo uruchom ten na innych portach: `HTTP_PORT=8080 HTTPS_PORT=8443 ./deploy/scripts/local.sh up`, potem `https://hacknation-2026.test:8443`.

## Zmiany

- Trasy i nagłówki: `routes.yml`. Po zmianie: `./deploy/scripts/local.sh restart` (montowania z Windows nie wysyłają zdarzeń o zmianie plików).
- Nowy certyfikat (ważny 825 dni) robisz przez `./deploy/scripts/local.sh certs` i `restart`. CA zostaje to samo.
- Przycisk „Zaloguj” (i inne linki do aplikacji) prowadzi tu do lokalnego `https://aed.hacknation-2026.test/<język>`; podmienia go `site/public/assets/site.js`. Na produkcji: `https://aed.hacknation-2026.pl/<język>`.
