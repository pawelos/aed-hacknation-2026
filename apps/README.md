# AED – prototyp (apps)

Prototyp mechanizmu na `https://aed.hacknation-2026.pl`. Mapa i wyszukiwarka AED, panel dysponenta, import z walidacją i deduplikacją oraz eksport otwartych danych (CSV, GeoJSON) z metadanymi dla dane.gov.pl. Wzór układu: `C:\car-reminder\apps`.

> Status: szkielet katalogów. Kod powstaje w kolejnych krokach.

## Stos

| Warstwa | Technologia |
|---|---|
| **API** | ASP.NET Core, EF Core, PostgreSQL + PostGIS, Scalar |
| **Web** | Next.js (szablon Eyris), next-intl (PL/EN), mapa Leaflet + OpenStreetMap |
| **Lokalnie** | .NET Aspire AppHost + `docker compose` (PostgreSQL) |
| **Produkcja** | Docker Compose za Traefikiem na VPS OVH (`../deploy/`) |

## Układ

```
apps/
├── api/Aed/                # Solution: Api, Application, Domain, Infrastructure, AppHost
├── web/                    # Next.js: mapa, wyszukiwarka, karta AED, panel dysponenta
├── data/                   # Przykładowe CSV/GeoJSON, wzorcowy zakres danych (XLSX), słowniki
├── docker/postgres/init/   # Init lokalnej bazy (rozszerzenie PostGIS)
├── e2e/                    # Testy Playwright na działającej aplikacji
├── tools/                  # build-release.ps1 (obrazy na VPS)
├── docs/                   # Dokumentacja techniczna (schemat danych, walidacja, API)
└── azure-pipelines.yml     # Build obrazów api/web (artefakt release dla ../deploy)
```

## Zasady (z Car Reminder)

- Błędy walidacji pokazujemy pod polem, którego dotyczą. Dotyczy to błędów z formularza i z API (`error.fields`).
- Każde pole tekstowe ma `maxLength` równy `MaximumLength` walidatora API.
- Dane udostępniamy tylko w formatach maszynowych (CSV, JSON, GeoJSON). XLSX służy wyłącznie jako wzorzec zakresu danych.
- Interfejs spełnia WCAG 2.2 AA i działa na komputerach i telefonach.
