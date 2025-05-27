# Plan implementacji widoku listy podsumowań

## 1. Przegląd
Widok listy podsumowań ma na celu wyświetlenie historii podsumowań notatek użytkownika. Użytkownik widzi daty generowania, krótki opis okresu oraz informacje o liczbie nowych podsumowań. W przypadku pustej listy, wyświetlany jest komunikat zachęcający do wygenerowania pierwszego podsumowania. Widok umożliwia także otwarcie modala do ręcznego generowania nowego podsumowania.

## 2. Routing widoku
Widok powinien być dostępny pod ścieżką: `/summaries`

## 3. Struktura komponentów
- **SummaryPage** – główny komponent widoku.
  - **HeaderSummary** – nagłówek z tytułem widoku oraz licznikiem nowych podsumowań.
  - **SummaryList** – komponent wyświetlający listę podsumowań.
    - **SummaryItem** – pojedynczy element listy, prezentujący datę, opis okresu i możliwość wyboru szczegółów.
  - **GenerateSummaryButton** – przycisk otwierający modal do ręcznego generowania podsumowania.
  - **GenerateSummaryModal** – modal umożliwiający wybór zakresu (ostatnie 7/14/30 dni lub ostatnie 10 notatek) i potwierdzenie generowania.

## 4. Szczegóły komponentów

### SummaryPage
- **Opis:** Główny widok podsumowań, koordynujący pobieranie danych oraz obsługę interakcji.
- **Elementy:** 
  - HeaderSummary
  - SummaryList
  - GenerateSummaryButton oraz GenerateSummaryModal (renderowany warunkowo)
- **Obsługiwane interakcje:** Inicjacja pobierania listy podsumowań, otwarcie modala do generowania.
- **Walidacja:** Weryfikacja poprawności statusu pobierania danych z API oraz obsługa stanu pustej listy.
- **Typy:** SummaryPageViewModel z polami: lista podsumowań, stan ładowania, informacje o paginacji, licznik nowych.
- **Propsy:** Brak – komponent nadrzędny do zarządzania stanem widoku.

### HeaderSummary
- **Opis:** Nagłówek widoku prezentujący tytuł oraz licznik nowych podsumowań.
- **Elementy:** 
  - Tytuł widoku (np. "Podsumowania")
  - Licznik nowych podsumowań
- **Obsługiwane interakcje:** Brak, wyłącznie informacyjny.
- **Walidacja:** Wyświetlanie poprawnej wartości licznika (0 lub więcej).
- **Typy:** Prosty model liczbowy.
- **Propsy:** Liczba nowych podsumowań przekazywana z SummaryPage.

### SummaryList
- **Opis:** Lista elementów podsumowań.
- **Elementy:** 
  - Iteracja po kolekcji SummaryItem
  - Informacja o pustej liście
- **Obsługiwane interakcje:** Kliknięcie elementu prowadzące do wyświetlenia pełnych szczegółów (np. otwarcie modala ze szczegółami podsumowania).
- **Walidacja:** Jeśli lista jest pusta – komunikat zachęcający do wygenerowania pierwszego podsumowania.
- **Typy:** Array of SummaryDto.
- **Propsy:** Lista podsumowań, ewentualnie funkcja obsługująca kliknięcie elementu.

### SummaryItem
- **Opis:** Pojedynczy element listy podsumowań.
- **Elementy:** 
  - Data generowania
  - Krótki opis okresu
- **Obsługiwane interakcje:** Kliknięcie powodujące przejście do szczegółów lub otwarcie modala z pełną treścią.
- **Walidacja:** Walidacja istnienia wymaganych pól (data, opis).
- **Typy:** SummaryDto (z polami: id, generationDate, periodDescription, itd.).
- **Propsy:** Obiekt SummaryDto, callback do obsługi kliknięcia.

### GenerateSummaryButton
- **Opis:** Przycisk inicjujący otwarcie modala do ręcznego generowania podsumowania.
- **Elementy:** 
  - Guzik z etykietą ("Generuj podsumowanie")
- **Obsługiwane interakcje:** Kliknięcie – otwarcie GenerateSummaryModal.
- **Walidacja:** Aktywność przycisku zależna od stanu (nieaktywny podczas operacji generowania).
- **Typy:** Prosty state flagowy.
- **Propsy:** Callback otwierający modal.

### GenerateSummaryModal
- **Opis:** Modal umożliwiający wybór zakresu i potwierdzenie generowania podsumowania.
- **Elementy:** 
  - Lista opcji okresowych (radio button/y lub select): ostatnie 7 dni, 14 dni, 30 dni, ostatnie 10 notatek.
  - Przycisk potwierdzenia
  - Spinner podczas operacji generowania
  - Komunikat potwierdzający operację
- **Obsługiwane interakcje:** Wybór opcji, kliknięcie przycisku potwierdzającego, zamknięcie modala.
- **Walidacja:** Sprawdzenie, czy użytkownik wybrał jedną z dostępnych opcji; dezaktywacja przycisku podczas trwania operacji.
- **Typy:** GenerateSummaryRequestViewModel (pole: wybrany okres z enum lub string).
- **Propsy:** Funkcja callback wywoływana po potwierdzeniu, funkcja do zamykania modala.

## 5. Typy
- **SummaryDto:** 
  - id: string (UUID)
  - userId: string (UUID)
  - content: string
  - generationDate: string (ISO date)
  - periodDescription: string
  - periodStart: string | null
  - periodEnd: string | null
  - isAutomatic: boolean
- **PaginationDto:**
  - currentPage: number
  - pageSize: number
  - totalCount: number
  - totalPages: number
- **SummaryPageViewModel:** 
  - summaries: SummaryDto[]
  - loading: boolean
  - error: string | null
  - pagination: PaginationDto
  - newSummariesCount: number
- **GenerateSummaryRequestViewModel:**
  - period: "7days" | "14days" | "30days" | "10notes"

## 6. Zarządzanie stanem
Widok będzie zarządzany przy użyciu mechanizmu reaktywności Alpine.js. Główne zmienne stanu:
- Lista podsumowań oraz obiekt paginacji.
- Flagi stanu: loading (pobieranie danych), generating (trwa generowanie podsumowania), error (komunikat błędu).
- State modala (czy jest otwarty).
Można stworzyć customowy hook (lub wykorzystać Alpine.js store) do centralnego zarządzania stanem podsumowań.

## 7. Integracja API
- **GET /api/Summaries?** – pobieranie listy podsumowań. Otrzymujemy strukturę:
  - summaries: SummaryDto[]
  - pagination: PaginationDto
- **GET /api/Summaries/{summaryId}** – wywołanie przy kliknięciu konkretnego podsumowania dla pobrania pełnej treści.
- **POST /api/Summaries/generate** – wywoływane z modala po potwierdzeniu generowania. Wysyłamy GenerateSummaryRequestViewModel. Obsługujemy odpowiedzi statusów 201, 400, 500.
Integracja będzie realizowana przy użyciu fetch lub axios, w zależności od preferencji projektu, z uwzględnieniem odpowiednich nagłówków (w szczególności Authorization).

## 8. Interakcje użytkownika
- Użytkownik trafia do widoku listy podsumowań.
- Na nagłówku wyświetlany jest licznik nowych podsumowań.
- W przypadku pustej listy wyświetlany jest komunikat zachęcający do wygenerowania podsumowania.
- Kliknięcie w element listy otwiera modal ze szczegółami podsumowania lub przechodzi do dedykowanego widoku.
- Kliknięcie przycisku "Generuj podsumowanie" otwiera modal.
- W modal, użytkownik wybiera interesującą opcję okresu i klika przycisk potwierdzenia.
- Podczas generowania pojawia się spinner, a przycisk jest dezaktywowany.
- Po zakończeniu operacji, nowo wygenerowane podsumowanie pojawia się w liście, a licznik zostaje zaktualizowany.

## 9. Warunki i walidacja
- Walidacja poprawności danych pobieranych z API (np. status HTTP, struktura odpowiedzi).
- Walidacja wybranej opcji w modalu (musi być wybrana jedna z dostępnych).
- Sprawdzenie uprawnień użytkownika – błędy 401/403 są obsługiwane globalnie.
- Weryfikacja i odpowiednie komunikaty w przypadku pustej listy lub błędów podczas pobierania danych.

## 10. Obsługa błędów
- W przypadku błędów podczas pobierania listy podsumowań, wyświetlany jest komunikat w widoku.
- W modalu generowania, komunikat o błędzie zostaje wyświetlony na wypadek odpowiedzi 400 lub 500 z API.
- Globalny interceptor błędów (jeśli istnieje) może obsługiwać błędy 401/403 i przekierować użytkownika na stronę logowania.
- Spinner i dezaktywacja przycisku zapobiegają wielokrotnym wywołaniom.

## 11. Kroki implementacji
1. Utworzyć strukturę szkieletową pod widok `/summaries` w odpowiednim pliku szablonu Razor lub komponencie Alpine.js.
2. Zaimplementować komponent **SummaryPage** wraz z logiką pobierania danych z GET /api/Summaries.
3. Zbudować komponent **HeaderSummary** z licznikem nowych podsumowań.
4. Zaimplementować komponent **SummaryList** iterujący po liście SummaryDto oraz warunkowy komunikat w przypadku pustej listy.
5. Utworzyć komponent **SummaryItem**, który wyświetla datę i opis okresu, wraz z obsługą akcji kliknięcia.
6. Dodać **GenerateSummaryButton** oraz logikę otwierania **GenerateSummaryModal**.
7. Zaimplementować **GenerateSummaryModal** z wyborem opcji okresu, walidacją wyboru oraz logiką wywołania POST /api/Summaries/generate.
8. Skonfigurować mechanizm zarządzania stanem przy użyciu Alpine.js (store lub custom hook) do obsługi zmiennych stanu.
9. Przetestować integrację API oraz walidację odpowiedzi, implementując obsługę błędów i komunikatów dla użytkownika.
10. Przeprowadzić testy responsywności i dostępności widoku oraz modala.