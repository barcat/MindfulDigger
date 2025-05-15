<plan_testów>

# Plan testów

## 1. Wprowadzenie i cele testowania

- **Cel testów:** Zapewnienie jakości, stabilności i bezpieczeństwa aplikacji poprzez systematyczne i kompleksowe testowanie kluczowych funkcjonalności zarówno na froncie, jak i na backendzie.
- **Główne założenia:** Wykrywanie błędów już na etapie developmentu, szybka identyfikacja regresji oraz zapewnienie wysokiej jakości oprogramowania, dostosowanego do specyfiki stosu technologicznego.

## 2. Zakres testów

- Testy jednostkowe dla logiki biznesowej i usług backendowych.
- Testy integracyjne, obejmujące interakcje pomiędzy komponentami oraz komunikację z bazą danych (Supabase) i usługami AI.
- Testy interfejsu użytkownika (frontend) obejmujące statyczne i dynamiczne elementy z .NET 8, Alpine.js oraz Tailwind CSS.
- Testy funkcjonalne, weryfikujące scenariusze użytkownika i interakcje pomiędzy warstwami aplikacji.
- Testy wydajnościowe (opcjonalnie), aby sprawdzić reakcje aplikacji w warunkach obciążenia.
- Testy bezpieczeństwa, weryfikujące autentykację użytkowników i zabezpieczenia API.

## 3. Typy testów

- **Testy jednostkowe:** Weryfikacja funkcjonalności poszczególnych metod i komponentów.
- **Testy integracyjne:** Sprawdzenie komunikacji między modułami, w tym między Web API, Supabase oraz interfejsem użytkownika.
- **Testy end-to-end:** Symulacja pełnych scenariuszy użytkownika, szczególnie w kontekście interakcji w środowisku UI.
- **Testy wydajnościowe:** Ocena szybkości i stabilności systemu przy zwiększonym obciążeniu.
- **Testy bezpieczeństwa:** Weryfikacja poprawności mechanizmów autoryzacji i autentykacji.

## 4. Scenariusze testowe dla kluczowych funkcjonalności

- **Scenariusze front-end:**
  - Weryfikacja poprawności renderowania widoków generowanych przez Razor Pages/MVC.
  - Testy interakcji użytkownika za pomocą Alpine.js – np. dynamiczne aktualizacje treści.
  - Weryfikacja responsywności i poprawności stylizacji przy wykorzystaniu Tailwind CSS.

- **Scenariusze backend:**
  - Weryfikacja poprawności wywołań API w środowisku .NET 8 Web API.
  - Testy integracyjne z Supabase - sprawdzanie poprawności operacji CRUD w bazie danych PostgreSQL.
  - Weryfikacja poprawności logiki związanej z autentykacją użytkowników.

- **Scenariusze AI:**
  - Testy integracyjne kontrolujące komunikację z usługą Openrouter.ai.
  - Weryfikacja ustawień limitów finansowych na klucze API.

## 5. Środowisko testowe

- **Lokalne środowisko deweloperskie:** Maszyna Windows z Visual Studio Code.
- **Środowisko CI/CD:** Github Actions użyte do automatycznego uruchamiania testów przy commitach.
- **Wirtualizacja:** W przypadku testów wydajnościowych możliwe wykorzystanie kontenerów Docker.
- **Baza danych:** Instancja Supabase skonfigurowana w środowisku testowym.

## 6. Narzędzia do testowania

- **Framework testowy:** NUnit (z wykorzystaniem NUnit3TestAdapter).
- **Mockowanie:** NSubstitute dla symulacji zależności.
- **Pokrycie kodu:** coverlet.collector.
- **Testy integracyjne:** Microsoft.AspNetCore.Mvc.Testing dla symulacji wywołań API.
- **Monitoring CI/CD:** Github Actions do automatycznego uruchamiania testów.

## 7. Harmonogram testów

- **Faza 1 – Przygotowanie:**
  - Konfiguracja środowisk testowych.
  - Przygotowanie danych testowych oraz konfiguracja pipeline’u CI/CD.
- **Faza 2 – Wdrożenie testów jednostkowych i integracyjnych:**
  - Implementacja i uruchamianie testów przy każdym commitcie.
- **Faza 3 – Testy end-to-end i wydajnościowe:**
  - Uruchomienie testów symulujących realne scenariusze użytkownika.
  - Wykonanie testów obciążeniowych przed wdrożeniem na produkcję.
- **Faza 4 – Retrospektywa i analiza wyników:**
  - Raportowanie błędów, analiza pokrycia testowego i ewentualne poprawki.

## 8. Kryteria akceptacji testów

- Testy jednostkowe i integracyjne muszą osiągać minimalne pokrycie kodu na poziomie 80%.
- Wszystkie kluczowe scenariusze użytkownika muszą przejść bez regresji.
- Testy wydajnościowe uzyskają wyniki w ramach przyjętych parametrów obciążenia.
- Wykryte krytyczne błędy muszą być naprawione przed wdrożeniem na produkcję.

## 9. Role i odpowiedzialności w procesie testowania

- **Inżynierowie QA:** Odpowiedzialni za tworzenie i utrzymanie testów automatycznych oraz analizę wyników.
- **Deweloperzy:** Współpraca przy pisaniu testów jednostkowych oraz poprawianiu wykrytych błędów.
- **DevOps:** Konfiguracja i utrzymanie środowisk testowych oraz automatyzacja pipeline’u w Github Actions.
- **Menedżer projektu:** Koordynacja harmonogramu testów i zatwierdzanie końcowych rezultatów.

## 10. Procedury raportowania błędów

- **Zgłaszanie:** Wszystkie błędy należy rejestrować w systemie do zarządzania issue (np. GitHub Issues) z przypisaniem priorytetu.
- **Opis:** Każdy zgłoszony błąd powinien zawierać kroki reprodukcji, oczekiwany rezultat oraz zrzuty ekranu/logi.
- **Weryfikacja:** Inżynierowie QA weryfikują zgłoszenia, a następnie przekazują deweloperom do analizy i naprawy.
- **Retest:** Po poprawkach konieczne jest przeprowadzenie regresyjnych testów potwierdzających naprawę błędu.

</plan_testów>