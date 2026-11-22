# Ustalenia mentorów – kanał #misja-aed-dane-na-ratunek (21–22.11.2026)

Najważniejsze odpowiedzi mentorów zadania i to, jak je uwzględniliśmy.

| Temat | Co powiedzieli mentorzy | Jak to uwzględniliśmy |
|---|---|---|
| Finał | 5 zespołów z każdego zadania | – |
| Instrukcja dla jury | Ponumerowane kroki: który plik otworzyć, jak odczytać zawartość, gdzie sprawdzić każde wymaganie | [`INSTRUKCJA-DLA-JURY.md`](../INSTRUKCJA-DLA-JURY.md): kroki 1–15 i tabela wymagań |
| Prototyp | Klikalny, bez konfigurowania środowiska przez jury; cenniejsze jest rozwiązanie do odczytu | Prototyp online https://aed.hacknation-2026.pl, konta demo wybierane jednym kliknięciem |
| Portal we własnym środowisku | Postawić dane.gov.pl lokalnie z kodu źródłowego, wejść do panelu administracyjnego, zrozumieć dodawanie zasobów i automatyczne zasilanie | Portal w [`portal/`](../portal/hacknation/README.md), panel `localhost:8001`, import naszych wykazów importerem XML portalu |
| Pierwszy commit | Wrzucić rozpakowany kod portalu jako pierwszy commit, potem własne przyrosty | Commit z `portal/` bez zmian, potem `portal/hacknation/` |
| Jaki jest problem | Dane są rozproszone i niezweryfikowane, nikt nie bierze za nie odpowiedzialności | Problem w README i na wizytówce: brak odpowiedzialnego dostawcy |
| Logotyp i hasło | Dla akcji promocyjnej skierowanej do potencjalnych dostawców danych, nie do prezentacji rozwiązania | Księga znaku i wizytówka: akcja dla dysponentów AED ([`brand/`](../brand/README.md)) |
| Kto publikuje | Nie można przebudować portalu, żeby każdy publikował. Potrzebny dostawca (instytucja, firma), który bierze odpowiedzialność za zbiór i ponosi konsekwencje za jakość | Każde źródło ma własny plik `aed-<id>.xml` do zarejestrowania przez dysponenta pod jego instytucją; prototyp nie publikuje w imieniu innych |
| Wielu dostawców, jedna lista | Może powstać wielu dostawców; portal ma zebrać ich zasoby i zagregować do jednej wspólnej listy. Podpowiedź: WYKAZ, w portalu są wykazy o podobnej logice, odtworzyć ją z kodu | Model jak „Wykaz rzeczy znalezionych” (starostwa jako dostawcy, wzorcowy zakres, szablon XML). Agregację robi dziś prototyp (`aed.xml`); scalanie wykazów w samym portalu nie jest zrobione |
| Najważniejsze | Nie tylko aplikacja zbierająca dane, ale dostosowanie logiki i metod portalu, żeby wykazy dało się pobierać i scalać w jedną standardową listę | Zrobione: import wykazów przez portal i poprawka zgodności (format daty). Do zrobienia: zmiana w kodzie portalu scalająca wykazy |
| Dane z rozproszonych źródeł | Dane mają być wystawione jako zasoby w dane.gov.pl; struktura jest rozproszona po wielu dostawcach, więc musi powstać zbiorcza tabela | Wzorcowy zakres danych i jeden format dla wszystkich dostawców |
| Licencja przy łączeniu źródeł | Portal pozwala wybrać licencję otwartą; dostawcom powinno zależeć, by dane dawać i proaktywnie wyrażać zgodę | Licencja przy każdym rekordzie i źródle; w XML warunek `source` (CC BY 4.0) lub CC0 według źródła |
| Struktura prezentacji | Przypomnienie o strukturze prezentacji z regulaminu | Prezentacja: do zrobienia |
