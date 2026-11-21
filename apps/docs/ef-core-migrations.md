# EF Core: migracje

Ściąga do pracy z migracjami API (`api/Aed`). Wzór: `C:\car-reminder\apps\docs\ef-core-migrations.md`.

## Narzędzie `dotnet-ef`

Instalowane globalnie, w tej samej wersji co pakiety EF Core w `.csproj` (obecnie `10.0.12`):

```powershell
dotnet ef --version
dotnet tool install --global dotnet-ef --version 10.0.12   # pierwsza instalacja
dotnet tool update --global dotnet-ef --version 10.0.12    # aktualizacja
```

## Kontekst i baza

| Kontekst | Baza | Folder migracji |
|----------|------|-----------------|
| `ApplicationDbContext` | `aed` (PostgreSQL 16 + PostGIS, lokalnie port 5433) | `Infrastructure/Database/Migrations` |

Rozszerzenie `postgis` deklaruje model (`HasPostgresExtension`), więc tworzy je migracja `InitialCreate`. Lokalnie zakłada je też skrypt `docker/postgres/init/01-postgis.sql`.

## Nowa migracja

Z katalogu `api/Aed`:

```powershell
dotnet ef migrations add <Nazwa> --project Infrastructure --startup-project Api --output-dir Database/Migrations
```

Migracje i seedery uruchamia API przy starcie (`Api/Extensions/MigrationExtensions.cs`). `/health` zwraca 200 dopiero, gdy się zakończą.

## Reset lokalnej bazy

- `Seed:ResetDatabase=true` (tylko Development) usuwa i tworzy bazę przy starcie API.
- Albo usunięcie wolumenu: `docker compose down -v`, a potem `docker compose up -d` (z katalogu `apps`).
