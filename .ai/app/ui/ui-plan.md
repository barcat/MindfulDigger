# Architektura UI dla MindfulDigger

## 1. Przegląd struktury UI

MindfulDigger to aplikacja webowa wspierająca prowadzenie notatek i generowanie automatycznych oraz ręcznych podsumowań. 
Interfejs jest zbudowany w oparciu o .NET 8 (Razor Pages/MVC), Alpine.js i Tailwind CSS, a jego projekt opiera się na stylistyce Material Design z kolorystyką inspirowaną starym papierem. 
Struktura UI dzieli się na kilka głównych widoków, a centralnym punktem nawigacji jest górny pasek z sekcjami „Notatki”, „Podsumowania” oraz „Konto”. 
Dodatkowo wykorzystywane są okna modalne do operacji takich jak tworzenie notatki czy generowanie podsumowania.

## 2. Lista widoków

- **Logowanie / Rejestracja / Resetowanie hasła**
  - **Ścieżka widoku:** `/login`, `/register`, `/reset-password`
  - **Główny cel:** Umożliwić użytkownikom autoryzację dostępu do aplikacji.
  - **Kluczowe informacje do wyświetlenia:** Formularze logowania, rejestracji, resetowania hasła; komunikaty walidacyjne.
  - **Kluczowe komponenty widoku:** Formularze, przyciski, linki nawigacyjne między widokami.
  - **UX, dostępność i bezpieczeństwo:** Intuicyjne etykiety pól, walidacja błędów w czasie rzeczywistym, ochrona przed atakami (np. zabezpieczenia brute force) oraz dostępność zgodna z WCAG.

- **Widok listy notatek**
  - **Ścieżka widoku:** `/notes`
  - **Główny cel:** Prezentacja listy notatek użytkownika w układzie trzykolumnowym, posortowanej od najnowszej.
  - **Kluczowe informacje do wyświetlenia:** Data utworzenia notatki, fragment treści (do 120 znaków, word-aware), komunikat w przypadku braku notatek.
  - **Kluczowe komponenty widoku:** Siatka (grid) notatek, przycisk dodawania notatki (dezaktywowany po osiągnięciu limitu 100), mechanizm infinite scroll.
  - **UX, dostępność i bezpieczeństwo:** Responsywny układ, elementy klikalne powinny być dobrze oznaczone i dostępne z klawiatury, komunikaty przy błędach walidacji.

- **Modal tworzenia notatki**
  - **Ścieżka widoku:** Okno modalne wywoływane z widoku listy notatek
  - **Główny cel:** Umożliwić szybkie wprowadzenie nowej notatki tekstowej.
  - **Kluczowe informacje do wyświetlenia:** Pole tekstowe, komunikat przy przekroczeniu limitu 1000 znaków (bez dynamicznego licznika).
  - **Kluczowe komponenty widoku:** Textarea, przycisk „Zapisz”, przycisk anulowania.
  - **UX, dostępność i bezpieczeństwo:** Modal z fokusem na pierwszym elemencie, walidacja po stronie klienta, przyciski dostępne klawiaturowo.

- **Widok szczegółu notatki**
  - **Ścieżka widoku:** `/notes/{id}`
  - **Główny cel:** Prezentacja pełnej treści wybranej notatki.
  - **Kluczowe informacje do wyświetlenia:** Pełny tekst notatki, data utworzenia, opcje edycji lub usunięcia.
  - **Kluczowe komponenty widoku:** Sekcja tekstu, przyciski nawigacyjne.
  - **UX, dostępność i bezpieczeństwo:** Czytelny układ, duże kontrastujące czcionki, możliwość powrotu do listy notatek.

- **Widok listy podsumowań**
  - **Ścieżka widoku:** `/summaries`
  - **Główny cel:** Wyświetlenie historii podsumowań notatek użytkownika.
  - **Kluczowe informacje do wyświetlenia:** Data generowania, krótki opis okresu, komunikat zachęcający do wygenerowania pierwszego podsumowania w przypadku pustej listy.
  - **Kluczowe komponenty widoku:** Lista podsumowań, przycisk ręcznego generowania podsumowania, licznik nowych podsumowań w nagłówku sekcji.
  - **UX, dostępność i bezpieczeństwo:** Prostota prezentacji, informacje o błędach pobierania danych z API, responsywność, możliwość sortowania/paginacji.

- **Modal ręcznego generowania podsumowania**
  - **Ścieżka widoku:** Modal dostępny z widoku podsumowań (aktywowany przy kliknięciu przycisku generowania)
  - **Główny cel:** Umożliwić użytkownikowi wybranie okresu (ostatnie 7/14/30 dni lub ostatnie 10 notatek) do wygenerowania podsumowania.
  - **Kluczowe informacje do wyświetlenia:** Dostępne opcje okresów, komunikat potwierdzający (np. „Czy na pewno chcesz wygenerować podsumowanie dla wybranego okresu?”).
  - **Kluczowe komponenty widoku:** Lista opcji okresowych, przycisk potwierdzenia, spinner podczas generowania.
  - **UX, dostępność i bezpieczeństwo:** Informacja zwrotna o stanie operacji, przyciski nieaktywne podczas operacji, zgodność z zasadami dostępności modali.

- **Widok szczegółu podsumowania**
  - **Ścieżka widoku:** `/summaries/{id}`
  - **Główny cel:** Prezentacja pełnej treści wybranego podsumowania w formacie markdown.
  - **Kluczowe informacje do wyświetlenia:** Pełny tekst podsumowania, data generowania, okres generacji, przyciski do oceny (kciuk w górę/dół).
  - **Kluczowe komponenty widoku:** Sekcja renderująca markdown, komponenty do zbierania feedbacku.
  - **UX, dostępność i bezpieczeństwo:** Czytelny format tekstu, wsparcie dla użytkowników korzystających z czytników ekranu, zabezpieczenie przed manipulacją ocenami.

- **Widok strony konta użytkownika**
  - **Ścieżka widoku:** `/account`
  - **Główny cel:** Zarządzanie ustawieniami konta, włączając możliwość zmiany hasła i przegląd aktywności.
  - **Kluczowe informacje do wyświetlenia:** Dane użytkownika, opcje zmiany hasła, historia logowań lub innych aktywności.
  - **Kluczowe komponenty widoku:** Formularze, przyciski, tabele/sekcje z informacjami.
  - **UX, dostępność i bezpieczeństwo:** Bezpieczne przetwarzanie danych, jasne komunikaty walidacji, zgodność z zasadami prywatności.

## 3. Mapa podróży użytkownika

1. **Rejestracja i logowanie:**
   - Użytkownik rozpoczyna podróż od widoku rejestracji/logowania.
   - Po udanym logowaniu użytkownik trafia do widoku listy notatek.

2. **Przeglądanie notatek:**
   - Użytkownik widzi listę swoich notatek, z możliwością przewijania (infinite scroll).
   - Kliknięcie na notatkę przenosi do widoku szczegółu notatki.
   - W widoku listy notatek dostępny jest przycisk otwierający modal tworzenia nowej notatki.

3. **Tworzenie nowej notatki:**
   - Użytkownik otwiera modal, wpisuje treść notatki i zapisuje ją.
   - Po zapisaniu modal zamyka się, a nowa notatka pojawia się na górze listy, jeśli limit nie został osiągnięty.

4. **Przeglądanie podsumowań:**
   - Użytkownik przełącza się do sekcji „Podsumowania” z górnego menu.
   - W widoku listy podsumowań widoczna jest historia generowanych podsumowań oraz licznik nowych podsumowań.
   - Kliknięcie przycisku generowania otwiera modal, w którym użytkownik wybiera zakres podsumowania.
   - Po potwierdzeniu, użytkownik obserwuje spinner, po czym nowe podsumowanie pojawia się w liście.

5. **Szczegóły podsumowania i ocena:**
   - Kliknięcie na konkretne podsumowanie wyświetla jego pełną treść.
   - Użytkownik może ocenić podsumowanie, wybierając pozytywną lub negatywną opinię.
   
6. **Zarządzanie kontem:**
   - Użytkownik przechodzi do widoku konta za pomocą górnej nawigacji.
   - Użytkownik może zmieniać ustawienia, w tym hasło oraz przeglądać historię aktywności.

## 4. Układ i struktura nawigacji

Górna nawigacja będzie stałym elementem interfejsu i zawiera:
- **Notatki:** Dostęp do widoku listy notatek oraz przycisk otwierający modal do tworzenia nowej notatki.
- **Podsumowania:** Lista podsumowań oraz licznik informujący o nowych wygenerowanych podsumowaniach; przycisk umożliwiający ręczne generowanie.
- **Konto:** Widok ustawień i informacji o koncie użytkownika.

Dodatkowo, w poszczególnych widokach zadbano o intuicyjne przyciski powrotu, jasne komunikaty o błędach (np. przy nieudanych operacjach z API) oraz responsywny design.

## 5. Kluczowe komponenty

- **Siatka notatek (grid) i karty notatek:** Widoczne w widoku listy notatek; karty są ujednolicone, pokazują datę i fragment treści.
- **Okna modalne:** Do tworzenia nowych notatek oraz generowania podsumowań; zapewniają izolowaną przestrzeń akcji z odpowiednimi komunikatami walidacyjnymi i wizualnym feedbackiem.
- **Spinner/indicator:** Wykorzystywany przy operacjach asynchronicznych, takich jak generowanie podsumowań, aby informować użytkownika o postępie.
- **Komponent feedbacku:** Przyciski ocen (kciuk w górę/dół) umieszczone przy podsumowaniach umożliwiające zbieranie opinii.
- **Elementy formularzy:** Standaryzowane formularze logowania, rejestracji i zarządzania kontem, z wbudowaną walidacją i obsługą błędów.

Ta architektura UI została zaprojektowana tak, aby spełnić wymagania z PRD, korzystać z odpowiednich endpointów API i odzwierciedlać ustalenia z sesji planowania interfejsu. Dzięki jasnemu przepływowi użytkownika, intuicyjnej nawigacji oraz spójnemu podejściu do obsługi błędów i stanów asynchronicznych, MindfulDigger zapewnia komfortowe i bezpieczne środowisko pracy z notatkami oraz podsumowaniami.