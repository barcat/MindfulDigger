# Kolejne kroki wdrożenia modułu autoryzacji (step 3)

## 1. Backend

- Implementacja endpointów API w `AuthController`:
  - POST `/api/auth/register` (rejestracja)
  - POST `/api/auth/forgot-password` (wysyłka linku resetującego)
  - POST `/api/auth/reset-password` (ustawienie nowego hasła)
- Rozszerzenie `AuthService` i `IAuthService` o metody:
  - `RegisterUser`
  - `SendForgotPasswordEmail`
  - `ResetPassword`
- Integracja z Supabase po stronie serwera dla powyższych operacji.
- Utworzenie i podpięcie odpowiednich DTO:
  - `RegistrationDto`, `ForgotPasswordDto`, `ResetPasswordDto`
- Walidacja danych wejściowych i obsługa wyjątków w endpointach.
- Przekazywanie czytelnych komunikatów błędów do frontendu.

## 2. Frontend

- Testy działania formularzy z backendem (poprawność komunikacji, obsługa błędów/sukcesów).
- Ewentualne poprawki UI/UX na podstawie testów.

## 3. Testy

- Dodanie testów jednostkowych i integracyjnych dla nowych endpointów oraz walidacji.

---
Po wykonaniu powyższych kroków backendowa obsługa rejestracji i resetowania hasła będzie kompletna, a frontend w pełni zintegrowany z API.
