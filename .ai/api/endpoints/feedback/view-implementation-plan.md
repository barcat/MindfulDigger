# API Endpoint Implementation Plan: POST /feedback

## 1. Przegląd punktu końcowego
Endpoint umożliwia uwierzytelnionym użytkownikom tworzenie lub aktualizowanie feedbacku (oceny "positive"/"negative") dla danego podsumowania, przy zastosowaniu logiki UPSERT w tabeli feedback.

## 2. Szczegóły żądania
- **Metoda HTTP**: POST
- **URL**: /feedback
- **Headers**:
  - Authorization: Bearer <token>
  - Content-Type: application/json
- **Request Body**:
  ```json
  {
    "summaryId": "c3d4e5f6-a7b8-9012-cdef-0123456789ab",
    "rating": "positive"
  }
  ```
- **Walidacja**:
  - summaryId musi być poprawnym UUID istniejacego podsumowania właściciela.
  - rating musi mieć wartość "positive" lub "negative".

## 3. Wykorzystywane typy i modele
- Enum FeedbackRating:
  ```csharp
  public enum FeedbackRating
  {
      positive,
      negative
  }
  ```
- DTO wejściowy FeedbackRequestDto, DTO wyjściowy FeedbackResponseDto, oraz Command Model CreateOrUpdateFeedbackCommand.
- Model encji FeedbackDbo zgodny z definicją tabeli w feedback.sql.

## 4. Szczegóły odpowiedzi
- **201 Created lub 200 OK**: Zwracane przy utworzeniu lub aktualizacji feedbacku.
- Odpowiedź zawiera: summaryId, userId, rating oraz creationDate.

## 5. Przepływ danych
1. Klient wysyła żądanie wraz z JWT w nagłówku.
2. Middleware ASP.NET Core weryfikuje token; w razie błędu zwracany jest 401.
3. Kontroler:
   - Waliduje dane wejściowe.
   - Pozyskuje userId z tokena.
   - Mapuje dane na CreateOrUpdateFeedbackCommand.
   - Wywołuje metodę CreateOrUpdateFeedbackAsync w FeedbackService.
4. FeedbackService:
   - Sprawdza istnienie i własność podsumowania (zwraca 403/404 w razie niezgodności).
   - Wykonuje operację UPSERT przy użyciu Supabase SDK.
   - Mapuje wynik na FeedbackResponseDto.
5. Kontroler zwraca odpowiedni kod statusu (201 lub 200).

## 6. Względy bezpieczeństwa
- Uwierzytelnianie za pomocą JWT (nagłówek Authorization).
- Autoryzacja sprawdzająca własność podsumowania (porównanie userId).
- RLS ustawione w Supabase tak, by użytkownicy mogli modyfikować tylko swoje dane.

## 7. Obsługa błędów
- 400 Bad Request: niepoprawne dane, błędy walidacji.
- 401 Unauthorized: brak lub nieważny token.
- 403 Forbidden: użytkownik nie jest właścicielem podsumowania.
- 404 Not Found: brak podsumowania.
- 500 Internal Server Error: błędy po stronie serwera (np. baza danych).

## 8. Rozważenia dotyczące wydajności
- Operacja UPSERT jest zoptymalizowana poprzez indeksy na (summary_id, user_id).
- Monitorowanie zapytań do Supabase i skuteczne zarządzanie połączeniami (singleton Supabase.Client).

## 9. Etapy wdrożenia
1. Zdefiniowanie modeli: FeedbackRating, FeedbackRequestDto, FeedbackResponseDto, CreateOrUpdateFeedbackCommand, FeedbackDbo.
2. Konfiguracja polityk RLS w Supabase (feedback.sql).
3. Implementacja FeedbackService (logika weryfikacji podsumowania, operacja UPSERT).
4. Implementacja metody POST /feedback w FeedbackController:
   - Walidacja żądania.
   - Mapowanie na command.
   - Wywołanie logiki serwisowej.
5. Rejestracja FeedbackService w DI (.NET).
6. Przeprowadzenie testów jednostkowych i integracyjnych.
7. Aktualizacja dokumentacji API (Swagger/OpenAPI) wraz z feedback-post.md.
8. Code Review, wdrożenie na środowisku testowym, a następnie produkcyjnym.
