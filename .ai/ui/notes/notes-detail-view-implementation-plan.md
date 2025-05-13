# Plan implementacji widoku Szczegółu notatki

## 1. Przegląd
Widok Szczegółu notatki ma na celu prezentację pełnej treści wybranej notatki wraz z datą utworzenia oraz opcjami edycji i usunięcia. Kluczowym aspektem jest zapewnienie czytelnego interfejsu oraz łatwego powrotu do listy notatek.

## 2. Routing widoku
Widok będzie dostępny pod ścieżką:  
`/notes/{id}`

## 3. Struktura komponentów
- **NoteDetailView** – główny komponent widoku odpowiedzialny za pobranie i wyświetlenie danych notatki.
  - **Header** – zawiera przycisk powrotu oraz tytuł widoku.
  - **NoteContent** – sekcja wyświetlająca pełną treść notatki i datę utworzenia.

## 4. Szczegóły komponentów

### NoteDetailView
- **Opis:** Główny kontener widoku, odpowiedzialny za inicjalizację pobierania danych notatki z API oraz zarządzanie stanem ładowania i błędów.
- **Główne elementy:** 
  - Komponent Header.
  - Obszar z pełną treścią notatki (NoteContent).
  - Komponent ActionButtons.
- **Obsługiwane interakcje:** 
  - Automatyczne pobieranie danych przy inicjalizacji (useEffect).
  - Obsługa stanu ładowania (spinner) oraz błędów (wyświetlenie komunikatu).
- **Warunki walidacji:** 
  - Sprawdzenie, czy id notatki jest poprawne.
  - Walidacja otrzymanych danych z API.
- **Typy:** NoteDetailViewModel (opisany poniżej).
- **Propsy:** Parametr z routingu – id notatki.

### Header
- **Opis:** Pasek nagłówka zawierający przycisk powrotu oraz tytuł widoku.
- **Główne elementy:** 
  - Przycisk "Powrót" (nawigacja do listy notatek).
  - Tytuł (np. "Szczegóły notatki").
- **Obsługiwane interakcje:** 
  - Kliknięcie przycisku powrotu powoduje nawigację do poprzedniego widoku.
- **Propsy:** Callback dla przycisku powrotu.

### NoteContent
- **Opis:** Sekcja wyświetlająca dane notatki – pełny tekst i datę utworzenia.
- **Główne elementy:** 
  - Element prezentujący treść notatki.
  - Element prezentujący datę (w czytelnym formacie).
- **Obsługiwane interakcje:** Brak własnych zdarzeń.
- **Propsy:** NoteDetailViewModel zawierający treść oraz datę utworzenia.

## 5. Typy
### NoteDetailViewModel
- **id:** string lub Guid – identyfikator notatki.
- **content:** string – pełna treść notatki.
- **createdDate:** DateTime lub string – data utworzenia notatki.

(W razie potrzeby możliwe rozszerzenie o dodatkowe pola np. status notatki.)

## 6. Zarządzanie stanem
- **Stan lokalny:** 
  - note (NoteDetailViewModel) – przechowuje dane notatki.
  - loading (boolean) – określa czy dane są ładowane.
  - error (string) – przechowuje komunikat błędu w przypadku niepowodzenia.
- **Hooki:** 
  - customowy hook nie jest wymagany, wystarczą standardowe useState i useEffect do zarządzania stanem.
  - useEffect do wywołania API przy inicjalizacji widoku.

## 7. Integracja API
- **Endpoint:** GET `/api/notes/{id}` – wykorzystanie metody GetNoteById z backendu.
- **Integracja:** 
  - Przy inicjalizacji komponentu pobieramy id z parametrów routingu.
  - Wysyłamy żądanie GET do API.
  - W przypadku powodzenia mapujemy odpowiedź na NoteDetailViewModel.
  - W przypadku błędów (404 – notatka nie istnieje, 500 – błąd serwera) wyświetlamy komunikat użytkownikowi.

## 8. Interakcje użytkownika
- **Nawigacja:** 
  - Kliknięcie przycisku "Powrót" powoduje przejście do widoku listy notatek.

## 9. Warunki i walidacja
- **Walidacja id:** Sprawdzenie formatu i niepustej wartości id pobranego z routingu.
- **Weryfikacja danych z API:** Upewnienie się, że otrzymany NoteDetailViewModel zawiera wymaganą treść i datę.
- **Obsługa błędów API:** W przypadku braku danych lub błędnego statusu odpowiedzi, widok wyświetla komunikat o błędzie i możliwość powrotu.

## 10. Obsługa błędów
- **Błąd pobierania danych:** Wyświetlenie komunikatu „Nie udało się pobrać notatki” oraz przycisku powrotu.
- **Błąd walidacji id:** Informacja użytkownika, że podane id notatki jest nieprawidłowe.
- **Loading:** Pokaż spinner lub komunikat „Ładowanie...” podczas pobierania danych.

## 11. Kroki implementacji
1. **Routing:**  
   - Skonfigurować trasę w routerze (np. w pliku routingu) dla ścieżki `/notes/:id`.
2. **Komponent NoteDetailView:**  
   - Utworzyć komponent NoteDetailView, który pobierze parametr id z URL.
   - Zaimplementować useEffect do wywołania API z id.
3. **Stany ładowania i błędów:**  
   - Ustawić stany loading, error oraz note.
   - Implementować logikę wyświetlania spinnera lub komunikatu błędu.
4. **Podział widoku:**  
   - Utworzyć podkomponent Header z przyciskiem powrotu.
   - Utworzyć komponent NoteContent do wyświetlania pełnej treści i daty.
5. **Integracja API:**  
   - Wykorzystać fetch lub axios do wykonania żądania GET `/api/notes/{id}`.
   - Zmapować odpowiedź API na NoteDetailViewModel.
6. **Obsługa interakcji użytkownika:**  
   - Dodać funkcje obsługujące kliknięcia przycisków: powrót, edycję oraz usunięcie.
7. **Walidacja i testowanie:**  
   - Przeprowadzić testy interfejsu na różnych rozmiarach ekranu.
   - Zweryfikować obsługę błędów i stany loading.
8. **Dokumentacja:**  
   - Upewnić się, że kod jest odpowiednio skomentowany i zgodny z wytycznymi projektowymi.
