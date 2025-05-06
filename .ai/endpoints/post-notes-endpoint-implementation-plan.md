# API Endpoint Implementation Plan: POST /notes

## 1. Przegląd punktu końcowego
Ten punkt końcowy umożliwia uwierzytelnionym użytkownikom tworzenie nowej notatki w systemie. Przyjmuje treść notatki w ciele żądania i zwraca szczegóły utworzonej notatki, w tym jej unikalny identyfikator i datę utworzenia.

## 2. Szczegóły żądania
-   **Metoda HTTP:** `POST`
-   **Struktura URL:** `/notes`
-   **Parametry:** Brak parametrów ścieżki lub zapytania. `userId` zostanie wyodrębniony z kontekstu uwierzytelniania (token JWT Supabase).
-   **Request Body:**
    ```json
    {
      "content": "string (required, max 1000 characters)"
    }
    ```

## 3. Wykorzystywane typy
-   **`CreateNoteRequest` (DTO):** Do deserializacji ciała żądania.
    ```csharp
    public class CreateNoteRequest
    {
        [Required]
        [MaxLength(1000)] // Walidacja długości
        public string Content { get; set; }
    }
    ```
-   **`CreateNoteResponse` (DTO):** Do serializacji odpowiedzi dla punktu końcowego tworzenia notatki.
    ```csharp
    public class CreateNoteResponse
    {
        public required string Id { get; set; }
        public required string UserId { get; set; }
        public required string Content { get; set; }
        public DateTime CreationDate { get; set; }
        public required string ContentSnippet { get; set; } // Dodany snippet
    }
    ```
-   **`Note` (Model):** Istniejący model bazy danych (plik: `MindfulDigger/Models/Note.cs`). Będzie używany do interakcji z Supabase.

## 4. Szczegóły odpowiedzi
-   **Sukces (201 Created):**
    ```json
    {
      "id": "uuid",
      "userId": "uuid",
      "content": "string",
      "creationDate": "datetime (ISO 8601)",
      "contentSnippet": "string" // Pierwsze X znaków lub cała treść, jeśli krótsza
    }
    ```
-   **Błędy:**
    -   `400 Bad Request`: Nieprawidłowe dane wejściowe (np. brak `content`, `content` za długi, przekroczony limit notatek użytkownika). Odpowiedź może zawierać szczegóły błędu walidacji.
    -   `401 Unauthorized`: Użytkownik nie jest uwierzytelniony.
    -   `403 Forbidden`: Teoretycznie nie powinno wystąpić przy poprawnym RLS i pobieraniu `userId` z tokenu.
    -   `500 Internal Server Error`: Błąd serwera podczas przetwarzania żądania.

## 5. Przepływ danych
1.  Żądanie `POST /notes` trafia do kontrolera API w aplikacji .NET 8.
2.  Framework ASP.NET Core wykonuje powiązanie modelu, deserializując ciało żądania do obiektu `CreateNoteRequest`.
3.  Uruchamiana jest walidacja modelu (np. atrybuty `[Required]`, `[MaxLength]`). Jeśli walidacja nie powiodła się, zwracany jest błąd `400 Bad Request`.
4.  Kontroler wywołuje metodę w dedykowanej usłudze (np. `INoteService`).
5.  Usługa pobiera `userId` uwierzytelnionego użytkownika z `HttpContext.User` (dostarczonego przez integrację z Supabase Auth).
6.  **Walidacja limitu notatek:** Usługa sprawdza, czy użytkownik nie przekroczył limitu 100 notatek, wykonując zapytanie do Supabase (np. `client.From<Note>().Where(n => n.UserId == userId).Count()`). Jeśli limit został osiągnięty, zwracany jest błąd (który kontroler przetłumaczy na `400 Bad Request`).
7.  Usługa tworzy nową instancję modelu `Note`, ustawiając `UserId` i `Content`. `Id` i `CreationDate` zostaną wygenerowane przez bazę danych lub Supabase.
8.  Usługa używa klienta Supabase C# do wstawienia nowego rekordu `Note` do tabeli `public.notes`. Supabase RLS zapewnia, że operacja powiedzie się tylko wtedy, gdy `userId` wstawianego rekordu pasuje do `auth.uid()` użytkownika wykonującego żądanie.
9.  Jeśli wstawienie do bazy danych powiedzie się, Supabase zwraca utworzony obiekt `Note` (w tym wygenerowane `Id` i `CreationDate`).
10. Usługa mapuje zwrócony model `Note` na `CreateNoteResponse`, generując `ContentSnippet` (np. pierwsze 50 znaków `Content`).
11. Usługa zwraca `CreateNoteResponse` do kontrolera.
12. Kontroler zwraca odpowiedź `201 Created` z `CreateNoteResponse` w ciele.

## 6. Względy bezpieczeństwa
-   **Uwierzytelnianie:** Punkt końcowy musi być chroniony za pomocą mechanizmu uwierzytelniania opartego na tokenach JWT Supabase. Należy zastosować atrybut `[Authorize]` na poziomie kontrolera lub akcji.
-   **Autoryzacja:** Logika biznesowa musi pobierać `userId` wyłącznie z tokenu uwierzytelniającego (`HttpContext.User`). Polityki RLS w Supabase zapewniają, że użytkownicy mogą tworzyć notatki tylko dla siebie (`WITH CHECK (auth.uid() = user_id)`).
-   **Walidacja danych wejściowych:** Należy rygorystycznie walidować `content` (wymagany, maksymalna długość 1000 znaków), aby zapobiec błędom i potencjalnym atakom (chociaż ryzyko XSS jest niskie w API zwracającym JSON, walidacja jest nadal kluczowa). Walidacja limitu notatek zapobiega nadużyciom zasobów.
-   **Ochrona przed CSRF:** Chociaż mniej istotne dla API bezstanowych używanych przez SPA lub aplikacje mobilne, jeśli API jest wywoływane z tradycyjnych aplikacji webowych opartych na sesjach/ciasteczkach, należy wdrożyć standardowe mechanizmy ochrony przed CSRF w .NET.
-   **Rate Limiting:** Rozważyć dodanie rate limitingu, aby zapobiec nadmiernemu tworzeniu notatek przez pojedynczego użytkownika lub bota.

## 7. Obsługa błędów
-   **Błędy walidacji (400):** Framework ASP.NET Core automatycznie obsługuje błędy walidacji modelu. Niestandardowe błędy walidacji (np. limit notatek) powinny być zgłaszane z usługi (np. przez wyjątek) i przechwytywane w kontrolerze lub globalnym filtrze wyjątków, aby zwrócić `400 Bad Request` z odpowiednim komunikatem.
-   **Brak uwierzytelnienia (401):** Obsługiwane przez middleware uwierzytelniania ASP.NET Core.
-   **Błędy Supabase/Bazy Danych (500):** Wyjątki zgłaszane przez klienta Supabase (np. błędy połączenia, naruszenia ograniczeń RLS/CHECK, które nie zostały przechwycone wcześniej) powinny być przechwytywane. Należy zalogować szczegóły błędu i zwrócić generyczną odpowiedź `500 Internal Server Error`, aby nie ujawniać wrażliwych informacji.
-   **Logowanie:** Wszystkie błędy (zwłaszcza 5xx) powinny być szczegółowo logowane za pomocą standardowego mechanizmu `ILogger` w .NET, zawierając jak najwięcej kontekstu (bez danych wrażliwych użytkownika).

## 8. Rozważania dotyczące wydajności
-   **Zapytanie o limit notatek:** Zapytanie `COUNT(*)` do tabeli `notes` przed każdym wstawieniem może wpłynąć na wydajność przy dużej liczbie notatek na użytkownika. Upewnij się, że istnieje indeks na `user_id` (`notes_user_id_idx` już zdefiniowany w planie DB).
-   **Operacja wstawiania:** Wstawienie pojedynczego rekordu jest zazwyczaj szybkie.
-   **Połączenia z Supabase:** Upewnij się, że klient Supabase jest zarządzany efektywnie (np. jako singleton lub usługa o zasięgu żądania, zgodnie z zaleceniami biblioteki Supabase C#).
-   **Rozmiar odpowiedzi:** Odpowiedź jest mała i nie powinna stanowić problemu.

## 9. Etapy wdrożenia
1.  **Utworzenie DTO:** Zdefiniuj klasy `CreateNoteRequest` i `CreateNoteResponse` w odpowiednim folderze projektu (np. `MindfulDigger/DTOs`).
2.  **Utworzenie/Aktualizacja Usługi:**
    *   Zdefiniuj interfejs `INoteService` z metodą np. `Task<CreateNoteResponse> CreateNoteAsync(CreateNoteRequest request, string userId)`.
    *   Zaimplementuj `NoteService`, wstrzykując klienta Supabase (`Supabase.Client`) i `ILogger`.
    *   Zaimplementuj logikę pobierania `userId` (zostanie przekazany z kontrolera).
    *   Zaimplementuj logikę sprawdzania limitu notatek użytkownika za pomocą klienta Supabase. Zgłoś wyjątek w przypadku przekroczenia limitu.
    *   Zaimplementuj logikę tworzenia i wstawiania obiektu `Note` za pomocą klienta Supabase.
    *   Zaimplementuj mapowanie wyniku z Supabase na `CreateNoteResponse`, w tym generowanie `ContentSnippet`.
    *   Dodaj obsługę wyjątków i logowanie dla operacji Supabase.
    *   Zarejestruj usługę w kontenerze DI (np. `builder.Services.AddScoped<INoteService, NoteService>();` w `Program.cs`).
3.  **Utworzenie Kontrolera API:**
    *   Utwórz nowy kontroler API (np. `NotesController`) dziedziczący po `ControllerBase`.
    *   Dodaj atrybuty `[ApiController]`, `[Route("api/[controller]")]` (lub bezpośrednio `[Route("notes")]` dla tego punktu końcowego) i `[Authorize]`.
    *   Wstrzyknij `INoteService` i `ILogger`.
    *   Zaimplementuj metodę akcji `POST`:
        ```csharp
        [HttpPost]
        [ProducesResponseType(typeof(CreateNoteResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateNote([FromBody] CreateNoteRequest request)
        {
            // Walidacja modelu jest obsługiwana automatycznie przez [ApiController]

            try
            {
                // Pobierz userId z tokenu JWT
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(); // Lub BadRequest, jeśli oczekujemy, że [Authorize] to złapie
                }

                var createdNoteDto = await _noteService.CreateNoteAsync(request, userId);
                // Zwróć 201 Created z lokalizacją (opcjonalnie) i obiektem DTO
                return CreatedAtAction(nameof(GetNoteById), new { id = createdNoteDto.Id }, createdNoteDto); // Zakładając, że istnieje GetNoteById
                // Lub prościej: return StatusCode(StatusCodes.Status201Created, createdNoteDto);
            }
            catch (UserNoteLimitExceededException ex) // Przykład niestandardowego wyjątku
            {
                _logger.LogWarning(ex, "User {UserId} exceeded note limit.", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating note for user {UserId}.", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred.");
            }
        }

        // Placeholder dla CreatedAtAction - wymaga endpointu GET /notes/{id}
        // [HttpGet("{id}")]
        // public async Task<IActionResult> GetNoteById(string id)
        // {
        //     // Implementacja pobierania notatki
        //     return Ok();
        // }
        ```
4.  **Konfiguracja Uwierzytelniania:** Upewnij się, że uwierzytelnianie Supabase JWT jest poprawnie skonfigurowane w `Program.cs`.
5.  **Testowanie:**
    *   Napisz testy jednostkowe dla `NoteService` (mockując klienta Supabase).
    *   Napisz testy integracyjne dla `NotesController`, używając `WebApplicationFactory` i/lub narzędzi takich jak Postman/curl, aby przetestować:
        *   Pomyślne utworzenie notatki (201, sprawdź ciało odpowiedzi `CreateNoteResponse`).
        *   Błędy walidacji (brak `content`, za długi `content`) (400).
        *   Przekroczenie limitu notatek (400).
        *   Żądanie bez tokenu (401).
        *   Żądanie z nieprawidłowym tokenem (401).
        *   Symulacja błędów bazy danych (500) (jeśli to możliwe w środowisku testowym).
6.  **Dokumentacja:** Zaktualizuj dokumentację API (np. Swagger/OpenAPI), jeśli jest używana.
