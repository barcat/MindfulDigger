# Specyfikacja modułu autoryzacji użytkowników

Niniejszy dokument opisuje architekturę oraz główne komponenty systemu odpowiedzialnego za rejestrację, logowanie oraz resetowanie hasła, wykorzystując ASP.NET Core 8, Razor Pages/MVC na frontendzie z Alpine.js i Tailwind CSS oraz Supabase Auth jako backendową usługę uwierzytelniania.

---

## 1. ARCHITEKTURA INTERFEJSU UŻYTKOWNIKA

### Strony i Layouty
- **Strona rejestracji:**  
  - Nowy widok zawierający formularz rejestracji z polami: adres e-mail, hasło oraz powtórzenie hasła.
  - Wykorzystanie szablonu wspólnego (layout) zgodnego z pozostałymi widokami aplikacji.
  - Elementy interaktywne (np. walidacja pól) będą realizowane przy użyciu Alpine.js.

- **Strona logowania:**  
  - Formularz logowania z polami: adres e-mail i hasło.
  - Przycisk logowania oraz link przekierowujący do strony resetu hasła.
  - W przypadku niepoprawnych danych – komunikat błędu ("Nieprawidłowy adres e-mail lub hasło").

- **Strona odzyskiwania hasła:**  
  - Formularz umożliwiający podanie adresu e-mail w celu wysłania linku resetującego.
  - Po wysłaniu linku użytkownik otrzymuje informację o wysłanym e-mailu.
  - Widok ustawiania nowego hasła, dostępny po kliknięciu w unikalny link z wiadomości.
  - Formularz zawiera pola: nowe hasło i potwierdzenie hasła, z walidacją wymagań (min. 8 znaków, co najmniej 1 cyfra, 1 znak specjalny).

### Walidacja i Komunikaty Błędów
- **Frontend:**  
  - Walidacja form (przy użyciu Alpine.js) wykonująca sprawdzenie formatu e-mail, zgodności haseł oraz spełnienia wymagań polityki haseł.
  - Komunikaty błędów pojawiają się bezpośrednio przy poszczególnych polach (np. "Nieprawidłowy format e-mail" lub "Hasło musi mieć przynajmniej 8 znaków, zawierać cyfrę i znak specjalny").
  - W przypadku błędów związanych z serwerem – wyświetlenie komunikatu w dedykowanej sekcji formularza.

- **Scenariusze obsługi:**
  - Pomyślna rejestracja powoduje automatyczne zalogowanie lub przekierowanie użytkownika do strony logowania z komunikatem o sukcesie.
  - Po pomyślnym zresetowaniu hasła, użytkownik otrzymuje informację o możliwości zalogowania się przy użyciu nowego hasła.
  - W przypadku próby podania już istniejącego adresu e-mail w rejestracji – wyświetlenie komunikatu o błędzie („Adres e-mail jest już zarejestrowany”).

---

## 2. LOGIKA BACKENDOWA

### Struktura Endpointów API
- **POST /api/auth/register:**  
  - Odbiera dane rejestracyjne (DTO z e-mailem, hasłem i potwierdzeniem hasła).
  - Walidacja danych wejściowych (sprawdzenie formatu, unikalności e-maila oraz wymagań dotyczących hasła).
  - Tworzenie rekordu użytkownika przy użyciu Supabase Auth.
  
- **POST /api/auth/login:**  
  - Autentykacja użytkownika na podstawie e-maila i hasła.
  - W przypadku poprawnych danych – generacja tokenu sesji lub wykorzystanie mechanizmu Supabase do utrzymania sesji.
  
- **POST /api/auth/forgot-password:**  
  - Przyjmuje e-mail i, jeżeli istnieje, wysyła e-mail z linkiem do resetowania hasła.
  
- **POST /api/auth/reset-password:**  
  - Umożliwia ustawienie nowego hasła po weryfikacji tokenu z linku resetującego.

### Modele Danych
- **User:**  
  - Dane użytkownika przechowywane w Supabase.
  - Pola: Id, Email, HashedPassword, DataUtworzenia, Status itp.
  
- **DTO:**  
  - RegistrationDto, LoginDto, ForgotPasswordDto, ResetPasswordDto – modelujące dane przesyłane przez formularze.

### Mechanizmy Walidacji i Obsługi Wyjątków
- Wykorzystanie wbudowanej walidacji ASP.NET Core (atrybuty walidacyjne, ModelState) do weryfikacji danych wejściowych.
- Globalny middleware obsługujący wyjątki, który loguje błędy i zwraca przyjazne komunikaty błędów do klienta.
- Szczegółowe sprawdzenie odpowiedzi Supabase (np. unikalność e-maila) i przekazywanie odpowiednich komunikatów do interfejsu.

---

## 3. SYSTEM AUTENTYKACJI

### Wykorzystanie Supabase Auth
- **Rejestracja i Logowanie:**  
  - Wykorzystanie Supabase jako centralnego systemu autoryzacji, integrując bibliotekę SDK dla języka C#.
  - Wywołania API Supabase z poziomu backendu ASP.NET Core, umożliwiające:
    - Rejestrację użytkowników – tworzenie kont i przechowywanie danych autoryzacyjnych (hasła w wersji zahaszowanej).
    - Logowanie użytkowników – autoryzacja przy użyciu e-maila i hasła oraz generowanie tokenów sesyjnych.
    - Weryfikację tożsamości i zabezpieczenie endpointów.
  
- **Reset Hasła:**  
  - Mechanizm resetowania hasła wykorzystuje funkcjonalność Supabase do wysyłania e-maili aktywacyjnych.
  - Wygenerowany token (z ograniczonym czasem ważności) jest przekazywany w linku do strony ustawiania nowego hasła.
  - Po poprawnej weryfikacji tokenu, użytkownik może ustawić nowe hasło poprzez dedykowany endpoint.

### Komponenty i Kontrakty
- **AuthService:**  
  - Moduł w warstwie backendu odpowiedzialny za komunikację z Supabase Auth.
  - Metody: RegisterUser, LoginUser, SendForgotPasswordEmail, ResetPassword.
  - Interfejs kontraktowy, który może być wykorzystywany przez kontrolery API.
  
- **Controller AuthController:**  
  - Odpowiada za mapowanie endpointów API na metody AuthService.
  - Umożliwia integrację logiki weryfikacyjnej z frontendem poprzez REST API.

- **Klient Supabase:**  
  - Biblioteka Supabase SDK, skonfigurowana w projekcie .NET, umożliwiająca bezpośrednią komunikację z bazą danych oraz systemem autoryzacji.

---

## Wnioski
Specyfikacja zakłada, że:
- Frontend, oparty na Razor Pages/MVC, zostanie rozszerzony o dedykowane widoki dla rejestracji, logowania i resetowania hasła, zapewniając spójną obsługę walidacji oraz przekazywanie komunikatów o błędach.
- Backend ASP.NET Core 8 będzie posiadał wyraźnie wydzielone endpointy API do autoryzacji i operacji na kontach użytkowników, zintegrowane z biblioteką Supabase SDK.
- System autoryzacji będzie wykorzystywał gotowe rozwiązania Supabase Auth, co zapewni bezpieczeństwo i skalowalność, jednocześnie upraszczając implementację logiki rejestracji i odzyskiwania hasła.

Niniejsza specyfikacja powinna stanowić podstawę do dalszego projektowania i implementacji modułu autoryzacji w istniejącej aplikacji, zachowując zgodność ze wszystkimi pozostałymi wymaganiami i nie zakłócając jej dotychczasowej funkcjonalności.