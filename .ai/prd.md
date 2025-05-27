# Dokument wymagań produktu (PRD) - Notatki AI
## 1. Przegląd produktu
Celem produktu jest stworzenie aplikacji webowej wspierającej użytkowników w procesie introspekcji poprzez regularne zapisywanie notatek (myśli, emocji, wydarzeń) oraz dostarczanie automatycznych, cotygodniowych podsumowań generowanych przez model językowy (LLM). Aplikacja ma na celu ułatwienie identyfikacji trendów, wzorców, powtarzających się tematów oraz powiązań między wydarzeniami a emocjami w zapiskach użytkownika, działając jak "bezstronny obserwator". Wersja MVP (Minimum Viable Product) skupia się na podstawowych funkcjach tworzenia notatek, bezpiecznym przechowywaniu danych, generowaniu podsumowań (automatycznych i ręcznych) oraz prostym systemie kont użytkowników.

## 2. Problem użytkownika
Użytkownicy, którzy regularnie prowadzą osobiste notatki, zwłaszcza te dotyczące przemyśleń i emocji, często napotykają trudności w systematycznej analizie swoich zapisków. Samodzielne wychwytywanie trendów, wzorców czy głębszych powiązań z rozproszonych notatek jest czasochłonne i wymaga wysiłku, przez co często jest pomijane. Prowadzi to do utraty cennych spostrzeżeń, które mogłyby wesprzeć rozwój osobisty, zrozumienie własnych emocji czy identyfikację powtarzających się schematów myślowych. Brak łatwego narzędzia do analizy tych danych ogranicza potencjał introspekcyjny płynący z prowadzenia dziennika czy notatek.

## 3. Wymagania funkcjonalne
-   WF-001: System rejestracji i logowania użytkowników oparty na adresie e-mail i haśle.
-   WF-002: Mechanizm resetowania zapomnianego hasła poprzez wysłanie linku aktywacyjnego na adres e-mail użytkownika.
-   WF-003: Polityka haseł: minimum 8 znaków, w tym co najmniej 1 cyfra i 1 znak specjalny.
-   WF-004: Możliwość tworzenia nowych notatek tekstowych za pomocą prostego formularza wyświetlanego w oknie modalnym, zawierającego wyłącznie pole treści.
-   WF-005: Limit długości pojedynczej notatki: 1000 znaków.
-   WF-006: Widok listy notatek użytkownika, posortowanych od najnowszej do najstarszej.
-   WF-007: Prezentacja notatek na liście: data utworzenia oraz fragment treści (do 120 znaków, ucinając słowa, które nie mieszczą się w całości). Układ 3-kolumnowy.
-   WF-008: Automatyczne generowanie cotygodniowych podsumowań notatek użytkownika przez LLM.
-   WF-009: Dostęp do podsumowań (np. lista) na dedykowanej stronie/sekcji w aplikacji. Wyświetlanie treści konkretnego podsumowania odbywa się w oknie modalnym.
-   WF-010: Format podsumowania LLM: podział na sekcje z nagłówkami, maksymalnie 3 akapity po 5 zdań każdy, ton profesjonalny, umiarkowana szczegółowość. Podsumowanie ma identyfikować trendy (sentyment, tematy, osoby, miejsca).
-   WF-011: Możliwość ręcznego wyzwolenia generowania podsumowania przez użytkownika poprzez interfejs w oknie modalnym.
-   WF-012: W oknie modalnym do ręcznego generowania podsumowania dostępne są opcje wyboru okresu: ostatnie 7 dni, 14 dni, 30 dni, ostatnie 10 notatek.
-   WF-013: Przycisk do otwarcia modala ręcznego wyzwalania podsumowania umieszczony w widocznym miejscu interfejsu (np. obok przycisku dodawania notatki).
-   WF-014: Mechanizm zbierania opinii użytkowników na temat przydatności wygenerowanych podsumowań (np. przyciski kciuk w górę / kciuk w dół pod każdym podsumowaniem).
-   WF-015: Limit przechowywania notatek: 100 notatek na użytkownika.
-   WF-016: Polityka retencji danych: automatyczne usuwanie danych użytkownika (w tym notatek i podsumowań) po 3 miesiącach braku aktywności konta.
-   WF-017: Backend aplikacji oparty na technologii ASP.NET Core 8.
-   WF-018: Frontend aplikacji oparty na technologii Razor Pages/MVC z wykorzystaniem biblioteki Alpine.js do interaktywności po stronie klienta oraz frameworka Tailwind CSS do stylizacji.
-   WF-019: Baza danych aplikacji oparta na platformie Supabase (PostgreSQL).

## 4. Granice produktu
Następujące funkcjonalności nie wchodzą w zakres MVP:
-   Tagowanie i kategoryzowanie notatek.
-   Zaawansowany edytor tekstu (formatowanie, osadzanie mediów).
-   Import notatek z zewnętrznych aplikacji (np. Evernote, OneNote).
-   Współdzielenie notatek lub podsumowań między użytkownikami.
-   Zaawansowana, jawna analiza sentymentu i emocji (poza wnioskami w podsumowaniu LLM).
-   Aplikacje mobilne (tylko wersja webowa).
-   Integracje z kalendarzami i systemami zarządzania zadaniami.
-   Szczegółowe ustawienia konta użytkownika (poza zmianą hasła).
-   Wyszukiwanie w notatkach.
-   Powiadomienia (np. o nowym podsumowaniu, o zbliżającym się usunięciu konta).

## 5. Historyjki użytkowników

-   ID: US-001
-   Tytuł: Rejestracja nowego użytkownika
-   Opis: Jako nowy użytkownik chcę móc założyć konto w aplikacji używając mojego adresu e-mail i hasła, abym mógł zacząć zapisywać notatki.
-   Kryteria akceptacji:
    -   Istnieje formularz rejestracji z polami na adres e-mail i hasło (oraz potwierdzenie hasła).
    -   Walidacja sprawdza, czy podany e-mail ma poprawny format.
    -   Walidacja sprawdza, czy podany e-mail nie jest już zarejestrowany.
    -   Walidacja sprawdza, czy hasło spełnia wymagania polityki (min. 8 znaków, 1 cyfra, 1 znak specjalny).
    -   Walidacja sprawdza, czy hasło i jego potwierdzenie są identyczne.
    -   Po pomyślnej walidacji i przesłaniu formularza, konto użytkownika jest tworzone w systemie.
    -   Użytkownik jest automatycznie zalogowany lub przekierowany na stronę logowania.
    -   Wyświetlany jest komunikat o pomyślnej rejestracji.
    -   W przypadku błędów walidacji, użytkownik widzi czytelne komunikaty przy odpowiednich polach.

-   ID: US-002
-   Tytuł: Logowanie do aplikacji
-   Opis: Jako zarejestrowany użytkownik chcę móc zalogować się do aplikacji używając mojego adresu e-mail i hasła, abym uzyskał dostęp do moich notatek i podsumowań.
-   Kryteria akceptacji:
    -   Istnieje formularz logowania z polami na adres e-mail i hasło.
    -   Po wprowadzeniu poprawnych danych uwierzytelniających i przesłaniu formularza, użytkownik jest zalogowany.
    -   Użytkownik jest przekierowany do głównego widoku aplikacji (np. listy notatek).
    -   W przypadku wprowadzenia niepoprawnego adresu e-mail lub hasła, wyświetlany jest ogólny komunikat błędu (np. "Nieprawidłowy e-mail lub hasło").
    -   Istnieje link do strony resetowania hasła.

-   ID: US-003
-   Tytuł: Resetowanie zapomnianego hasła
-   Opis: Jako zarejestrowany użytkownik, który zapomniał hasła, chcę móc je zresetować, abym mógł odzyskać dostęp do mojego konta.
-   Kryteria akceptacji:
    -   Na stronie logowania znajduje się link "Zapomniałem hasła".
    -   Po kliknięciu linku, użytkownik jest przekierowany na stronę, gdzie może podać swój adres e-mail.
    -   Jeśli podany e-mail istnieje w systemie, na ten adres wysyłana jest wiadomość z unikalnym linkiem do resetowania hasła.
    -   Link w e-mailu prowadzi do strony, na której użytkownik może ustawić nowe hasło.
    -   Formularz ustawiania nowego hasła zawiera pola na nowe hasło i jego potwierdzenie.
    -   Walidacja sprawdza, czy nowe hasło spełnia wymagania polityki (min. 8 znaków, 1 cyfra, 1 znak specjalny).
    -   Walidacja sprawdza, czy nowe hasło i jego potwierdzenie są identyczne.
    -   Po pomyślnym ustawieniu nowego hasła, użytkownik jest o tym informowany i może się zalogować używając nowego hasła.
    -   Link do resetowania hasła ma ograniczony czas ważności.
    -   Jeśli podany e-mail nie istnieje w systemie, użytkownik widzi stosowny komunikat (lub nie otrzymuje żadnej informacji zwrotnej ze względów bezpieczeństwa - do decyzji).

-   ID: US-004
-   Tytuł: Dodawanie nowej notatki
-   Opis: Jako zalogowany użytkownik chcę móc szybko dodać nową notatkę tekstową, aby zapisać swoje myśli, emocje lub wydarzenia.
-   Kryteria akceptacji:
    -   W interfejsie aplikacji znajduje się przycisk do dodawania nowej notatki, który otwiera formularz w oknie modalnym.
    -   Formularz dodawania notatki jest wyświetlany w oknie modalnym.
    -   Formularz zawiera jedno pole tekstowe (textarea) na treść notatki.
    -   Istnieje przycisk "Zapisz" lub podobny do zatwierdzenia notatki.
    -   Po zapisaniu, notatka jest dodawana do listy notatek użytkownika, a okno modalne jest zamykane.
    -   Nowo dodana notatka pojawia się na górze listy notatek.
    -   System zapisuje treść notatki oraz datę i czas jej utworzenia.
    -   Walidacja uniemożliwia zapisanie pustej notatki.
    -   Walidacja uniemożliwia zapisanie notatki przekraczającej limit 1000 znaków (wyświetlany jest komunikat błędu w modalu).
    -   Użytkownik nie może dodać nowej notatki, jeśli osiągnął limit 100 notatek (przycisk dodawania jest nieaktywny).

-   ID: US-005
-   Tytuł: Przeglądanie listy notatek
-   Opis: Jako zalogowany użytkownik chcę móc przeglądać listę moich zapisanych notatek, posortowanych od najnowszej, abym mógł przypomnieć sobie przeszłe zapiski.
-   Kryteria akceptacji:
    -   Po zalogowaniu użytkownik widzi listę swoich notatek.
    -   Notatki są wyświetlane w układzie 3-kolumnowym.
    -   Notatki są posortowane malejąco według daty utworzenia (najnowsze na górze).
    -   Każda pozycja na liście pokazuje datę utworzenia notatki.
    -   Każda pozycja na liście pokazuje fragment treści notatki (maksymalnie 120 znaków, bez ucinania słów w połowie).
    -   Jeśli użytkownik nie ma jeszcze żadnych notatek, wyświetlany jest odpowiedni komunikat (np. "Nie masz jeszcze żadnych notatek. Dodaj pierwszą!").

-   ID: US-006
-   Tytuł: Przeglądanie cotygodniowego podsumowania
-   Opis: Jako zalogowany użytkownik chcę móc przeglądać automatycznie generowane cotygodniowe podsumowanie moich notatek, abym mógł zobaczyć zidentyfikowane trendy i wzorce.
-   Kryteria akceptacji:
    -   Istnieje dedykowana sekcja/strona w aplikacji, gdzie użytkownik może uzyskać dostęp do podsumowań (np. lista podsumowań).
    -   Co tydzień (np. w poniedziałek rano) generowane jest nowe podsumowanie na podstawie notatek z ostatniego tygodnia.
    -   Po wybraniu konkretnego podsumowania (np. kliknięciu na liście), jego treść jest wyświetlana w oknie modalnym.
    -   Podsumowanie jest prezentowane w ustalonym formacie (sekcje, nagłówki, akapity, zdania) wewnątrz okna modalnego.
    -   Podsumowanie zawiera wnioski dotyczące trendów (sentyment, tematy, osoby, miejsca) zidentyfikowanych przez LLM.
    -   Jeśli w danym tygodniu nie było wystarczającej liczby notatek do wygenerowania podsumowania, wyświetlany jest odpowiedni komunikat (np. na liście podsumowań).
    -   Użytkownik widzi datę, której dotyczy podsumowanie (np. na liście lub w modalu).

-   ID: US-007
-   Tytuł: Ręczne generowanie podsumowania
-   Opis: Jako zalogowany użytkownik chcę móc ręcznie wygenerować podsumowanie dla wybranego okresu lub liczby notatek, abym mógł przeanalizować konkretny zestaw danych.
-   Kryteria akceptacji:
    -   W interfejsie znajduje się przycisk "Generuj podsumowanie" (lub podobny), który otwiera okno modalne.
    -   W oknie modalnym użytkownik ma możliwość wyboru zakresu podsumowania: ostatnie 7 dni, 14 dni, 30 dni, ostatnie 10 notatek.
    -   W oknie modalnym znajduje się przycisk do potwierdzenia wyboru i rozpoczęcia generowania.
    -   Po wybraniu zakresu i kliknięciu przycisku potwierdzenia, system rozpoczyna proces generowania podsumowania przez LLM, a okno modalne może zostać zamknięte lub wyświetlić informację o trwającym procesie.
    -   Użytkownik jest informowany, że proces generowania jest w toku (np. przez wskaźnik postępu lub komunikat w głównym interfejsie lub w nowym modalu po zakończeniu).
    -   Po zakończeniu generowania, podsumowanie jest dostępne do wyświetlenia (np. na liście podsumowań, a jego treść otwierana w modalu zgodnie z US-006).
    -   Format wygenerowanego podsumowania jest zgodny z WF-010.
    -   Jeśli dla wybranego zakresu nie ma wystarczającej liczby notatek, użytkownik widzi odpowiedni komunikat (np. w modalu po próbie generowania lub jako informacja zwrotna po zamknięciu modala).
    -   Przycisk potwierdzenia w modalu jest nieaktywny podczas trwania procesu generowania (jeśli modal pozostaje otwarty) lub główny przycisk "Generuj podsumowanie" jest nieaktywny.

-   ID: US-008
-   Tytuł: Ocena przydatności podsumowania
-   Opis: Jako zalogowany użytkownik chcę móc ocenić, czy wygenerowane podsumowanie (automatyczne lub ręczne) było dla mnie przydatne, aby dostarczyć feedback twórcom aplikacji.
-   Kryteria akceptacji:
    -   Pod każdym wyświetlonym podsumowaniem znajdują się przyciski "Kciuk w górę" i "Kciuk w dół".
    -   Użytkownik może kliknąć jeden z przycisków, aby wyrazić swoją opinię.
    -   Po kliknięciu, wybór użytkownika jest rejestrowany w systemie.
    -   Użytkownik może zmienić swoją ocenę.
    -   Interfejs wizualnie wskazuje, która opcja została wybrana (jeśli została).


## 6. Metryki sukcesu
-   MS-001: Przydatność podsumowań: Co najmniej 80% ocen podsumowań (automatycznych i ręcznych) to oceny pozytywne ("kciuk w górę"). Mierzone jako (liczba ocen "kciuk w górę") / (łączna liczba ocen "kciuk w górę" + "kciuk w dół") w danym okresie.
-   MS-002: Zaangażowanie użytkowników: Średnia liczba notatek dodawanych przez aktywnego użytkownika wynosi co najmniej 4 tygodniowo. Mierzone jako (łączna liczba notatek dodanych przez wszystkich użytkowników w tygodniu) / (liczba użytkowników, którzy zalogowali się przynajmniej raz w danym tygodniu).