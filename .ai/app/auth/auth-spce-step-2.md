# Kroki do uzupełnienia brakujących elementów modułu autoryzacji

## 1. Frontend

### 1.1. Strona rejestracji
- Utwórz widok Razor Pages (np. `Register.cshtml`) z formularzem rejestracji:
  - Pola: adres e-mail, hasło, powtórzenie hasła.
  - Zastosuj wspólny layout.
  - Dodaj walidację frontową (Alpine.js):
    - Sprawdzenie formatu e-mail.
    - Sprawdzenie zgodności haseł.
    - Sprawdzenie wymagań hasła (min. 8 znaków, 1 cyfra, 1 znak specjalny).
  - Wyświetlanie komunikatów błędów przy polach.
  - Obsłuż komunikaty o sukcesie/błędzie z backendu.

### 1.2. Strona resetowania hasła
- Utwórz widok Razor Pages (np. `ForgotPassword.cshtml`):
  - Formularz z polem e-mail do wysłania linku resetującego.
  - Komunikat o wysłaniu e-maila.
- Utwórz widok do ustawiania nowego hasła (`ResetPassword.cshtml`):
  - Pola: nowe hasło, powtórzenie hasła.
  - Walidacja wymagań hasła (jw.).
  - Obsługa tokenu z linku.
  - Komunikat o sukcesie.

### 1.3. Uzupełnienie strony logowania
- Dodaj link do strony resetu hasła.
- Upewnij się, że walidacja frontowa e-maila i hasła jest kompletna.

## 2. Backend

### 2.1. Endpoint rejestracji
- Dodaj endpoint `POST /api/auth/register` w `AuthController`:
  - Przyjmuj DTO z e-mailem, hasłem, powtórzeniem hasła.
  - Waliduj dane wejściowe (format, zgodność, wymagania, unikalność e-maila).
  - Wywołaj metodę rejestracji w `AuthService` (integracja z Supabase).
  - Obsłuż wyjątki (np. e-mail już istnieje).
  - Zwróć odpowiedni komunikat o sukcesie/błędzie.

### 2.2. Endpointy resetowania hasła
- Dodaj endpoint `POST /api/auth/forgot-password`:
  - Przyjmuj e-mail, wywołaj wysyłkę linku resetującego przez Supabase.
  - Zwróć komunikat o wysłaniu e-maila.
- Dodaj endpoint `POST /api/auth/reset-password`:
  - Przyjmuj token i nowe hasło.
  - Zweryfikuj token i ustaw nowe hasło przez Supabase.
  - Zwróć komunikat o sukcesie/błędzie.

### 2.3. Rozszerzenie `AuthService` i interfejsu
- Dodaj metody: `RegisterUser`, `SendForgotPasswordEmail`, `ResetPassword`.
- Zaimplementuj integrację z Supabase SDK dla powyższych operacji.

### 2.4. DTO
- Dodaj/uzupełnij klasy DTO:
  - `RegistrationDto`, `ForgotPasswordDto`, `ResetPasswordDto`.

### 2.5. Walidacja i obsługa wyjątków
- Upewnij się, że wszystkie endpointy korzystają z walidacji ModelState i atrybutów walidacyjnych.
- Dodaj globalny middleware do obsługi wyjątków (jeśli nie istnieje).
- Przekazuj czytelne komunikaty błędów do frontendu.

## 3. Testy
- Dodaj testy jednostkowe i integracyjne dla nowych endpointów oraz walidacji.

---

Po wykonaniu powyższych kroków moduł autoryzacji będzie zgodny ze specyfikacją.
