# Znak akcji AED Radar

Logotyp i księga znaku akcji promocyjnej dla dysponentów AED (dostawców danych do dane.gov.pl). Hasło: **„Udostępnij dane. Daj komuś szansę.”**

| Plik | Co |
|---|---|
| [`ksiega-znaku.pdf`](ksiega-znaku.pdf) | Księga znaku (A4 poziomo, 8 stron): koncepcja, warianty, kolory, typografia, pole ochronne, minimalna wielkość, niedozwolone użycia, zastosowania |
| [`src/*.svg`](src/) | Logo w formacie otwartym (SVG), napis zamieniony na krzywe: `logo-poziome`, `logo-pionowe`, `logo-z-haslem`, `sygnet`, `sygnet-bez-tla`, `logo-mono-czarne`, `logo-mono-biale`, `favicon` |
| [`logo/*.png`](logo/) | Każdy wariant w szerokości 512 i 2048 px, przezroczyste tło |
| [`logo/*.jpg`](logo/) | Każdy wariant w 2048 px na białym tle (wersja biała na granatowym) |
| [`logo/logo.pdf`](logo/logo.pdf) | Wszystkie warianty wektorowo, jeden na stronę |
| `ksiega-znaku.html` | Źródło księgi znaku |
| `tools/generate_svg.py` | Generator SVG: geometria sygnetu i napis z fontu Inter (SIL OFL 1.1) zamieniony na krzywe |
| `build.ps1` | Render PNG, JPG i PDF przez Edge w trybie headless. Kopiuje też sygnet, favicon, księgę i ZIP z logo do wizytówki (`site/public/assets/`) |

Po zmianie znaku:

```powershell
python -m pip install fonttools
python brand/tools/generate_svg.py <katalog z plikami inter-latin(-ext)-600/800.woff z @fontsource/inter 5.0.18>
powershell -NoProfile -ExecutionPolicy Bypass -File brand/build.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File site/og.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File site/build.ps1
```

Kolory: zieleń AED `#00843A`, czerwień serca `#C8102E`, granat `#0B1F3A`, biel. Wartości CMYK w księdze są przeliczone z RGB; przed drukiem trzeba je potwierdzić z drukarnią.
