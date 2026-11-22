# Portal dane.gov.pl lokalnie + import wykazów AED (HackNation 2026)

Kod portalu (backend 2.63.0) jest w folderze `portal/` repo AED, dodany jednym commitem bez zmian. Ten katalog (`portal/hacknation/`) to nasze przyrosty w kolejnych commitach: uruchomienie na localhost i import wykazów AED z prototypu AED Radar własnym importerem XML portalu.

## Uruchomienie (Docker, ok. 30–40 min za pierwszym razem)

```bash
cd portal
bash hacknation/setup.sh                      # .env i mcod/settings/local.py
C="docker compose -f docker-compose.local.yml -f hacknation/docker-compose.hacknation.yml"
$C build mcod-local-admin mcod-db mcod-rabbitmq mcod-redis mcod-elasticsearch mcod-rdfdb mcod-nginx
$C up -d mcod-db mcod-rabbitmq mcod-redis mcod-elasticsearch mcod-rdfdb mcod-smtp
$C run --rm --no-deps --entrypoint "" mcod-local-admin python manage.py init_mcod_db
$C up -d --no-deps mcod-local-admin mcod-local-api mcod-local-celery mcod-geocoder-stub
docker exec -i mcod-admin python manage.py shell < hacknation/aed_import.py
```

| Co | Adres |
|---|---|
| Panel administracyjny | http://localhost:8001 (`admin@mcod.local` / `testadmin123!`, konto z README portalu) |
| API | http://localhost:8000/1.4/datasets |
| Zbiory AED w panelu | http://localhost:8001/datasets/dataset/ |
| Źródła importu XML | http://localhost:8001/harvester/datasource/ |
| Tabela z CSV (API) | http://localhost:8000/1.4/resources/{id}/data |

## Co zmieniliśmy w środowisku i dlaczego

| Zmiana | Powód |
|---|---|
| Podsieć `172.28.18.0/24` zamiast `172.18.18.0/24` | Kolizja z inną siecią Dockera na tej maszynie |
| Panel `:8001`, API `:8000` na localhost, puste domeny ciasteczek | Bez nazw `*.mcod.local`, wpisów w `hosts` i nginx |
| Elasticsearch 512 MB | Docker ma 4 GB |
| `mcod-geocoder-stub` (`geocoder_stub.py`) | Importer sprawdza `<regions>` (TERYT) w wewnętrznym geokoderze Pelias/Placeholder, którego nie ma w publicznym kodzie; zamiennik zna 16 województw |
| `ELASTICSEARCH_*_INDEX_SETTINGS` bez `timeout` | `local.py.sample` podaje `timeout` jako ustawienie indeksu, Elasticsearch 6.8 go odrzuca i żadna tabela się nie indeksuje |
| Właściciel `.erlang.cookie` w wolumenie RabbitMQ | Plik powstał z właścicielem root, RabbitMQ (uid 100) nie startował |

## Wynik (22.11.2026)

- Importer XML portalu przyjął `aed-<id źródła>.xml` z https://aed.hacknation-2026.pl dla dwóch instytucji: status `ok`, 2 opublikowane zbiory z naszym `extIdent`, po 3 zasoby (CSV, GeoJSON, JSON), licencja z `<conditions>`, region „woj. dolnośląskie”.
- CSV: 4★, tabela w API portalu: 12 z 12 wierszy (Wrocław) i 6 z 6 (OSP).
- GeoJSON i JSON: 3★, bez tabeli (portal indeksuje tylko formaty tabelaryczne).
- Znaleziony błąd zgodności po stronie AED Radar: `data_aktualizacji` w ISO 8601 z `Z` (`2026-11-21T19:17:07Z`) nie mieści się w formatach dat portalu (`DATE_FORMATS` w `mcod/settings/base.py`), więc Elasticsearch odrzucał każdy wiersz. Poprawione w AED Radar na `RRRR-MM-DD gg:mm:ss` (UTC).
- Ponowne przetworzenie zasobu po zmianie pliku: `process_resource_from_url_task(id, forced_file_changed=True)`, potem `process_resource_file_data_task(id)`.

## Zrzuty ekranu (`zrzuty/`, 22.11.2026)

| Plik | Co pokazuje |
|---|---|
| `01-panel-start.png` | Panel administracyjny portalu na http://localhost:8001 po zalogowaniu |
| `02-zrodla-importu-xml.png` | Źródła importu XML: Wrocław i OSP, status „Aktywny”, ostatni import „OK” |
| `03-zrodlo-xml-wroclaw.png` | Źródło XML Wrocławia: adres `aed-<id>.xml` z AED Radar, częstotliwość, instytucja |
| `04-zbiory-danych.png` | Dwa opublikowane wykazy AED utworzone przez `automatic_import` |
| `05-zbior-wykaz-aed-wroclaw.png` | Zbiór Wrocławia: opis z odpowiedzialnością dostawcy, kategorie Zdrowie i Ludność, sposób udostępnienia XML |
| `06-zasob-csv-wroclaw.png` | Zasób CSV w panelu |
| `07-instytucje.png` | Instytucje (dostawcy danych) |
| `08-api-tabela-csv-wroclaw.png` | API portalu: 12 wierszy tabeli z CSV (wybrane kolumny, `data_aktualizacji` po poprawce) |
| `09-api-zasob-csv-wroclaw.png` | API portalu: zasób CSV z formatem, oceną 4★, regionami i adresem źródłowym |

Zrzuty 01–07 to strony panelu. Zrzuty 08–09 to odpowiedzi API portalu, pokazane w uproszczonym widoku (wybrane pola). Skrypt do zrzutów: Playwright z `apps/e2e`.
