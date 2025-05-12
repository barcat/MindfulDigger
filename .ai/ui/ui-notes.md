# Podsumowanie planowania architektury UI dla MindfulDigger

<conversation_summary>
<decisions>
1. Nawigacja: Górna nawigacja z sekcjami "Notatki", "Podsumowania", "Konto"
2. Modal tworzenia notatki: 50% szerokości ekranu
3. Licznik znaków: Brak - wyświetlanie komunikatu tylko gdy użytkownik dojdzie do limitu
4. Infinite scroll: Automatyczne ładowanie po dotarciu do końca
5. Animacja ładowania: Prosty spinner
6. Układ notatek: Trzykolumnowy grid (Tailwind CSS grid-cols-3) z kartami o jednakowej wysokości
7. Wyświetlanie podsumowań: W formie listy jednowymiarowej
8. Informacje o podsumowaniu: Data generowania oraz krótki opis okresu
9. Przeglądanie podsumowań: Na osobnej stronie, nie w modalu
10. Potwierdzenie generowania: "Czy na pewno chcesz wygenerować podsumowanie dla wybranego okresu?"
11. System powiadomień: Ikona z liczbą nowych podsumowań obok nazwy karty Podsumowania
12. Przyciski oceny: Na dole strony podsumowania, bez szczególnego wyróżnienia
13. Schemat kolorów: Kolory starego papieru
14. Styl wizualny: Minimalistyczny, Material Design
15. Wyświetlanie treści notatki: W całości na osobnych stronach
16. Notatki i podsumowania jako oddzielne strony
17. Limit notatek: Wyłączenie przycisku dodawania po osiągnięciu 100 notatek
18. Brak rozróżnienia wizualnego między automatycznymi a ręcznymi podsumowaniami
19. Walidacja formularzy: Po stronie klienta
20. Stany puste: Komunikaty zachęcające (np. "Proszę dodaj 1 notatkę")
</decisions>

<matched_recommendations>
1. Górna nawigacja z trzema głównymi linkami: "Notatki", "Podsumowania", "Konto"
2. Modal średniej wielkości (50% szerokości) do tworzenia notatek
3. Automatyczne ładowanie kolejnych notatek przy dojściu do końca widocznej zawartości (infinite scroll)
4. Prosty spinner podczas generowania podsumowania
5. Układ trzykolumnowy dla notatek przy użyciu Tailwind CSS grid, z kartami o jednakowej wysokości
6. Lista podsumowań z datą generowania oraz krótkim opisem okresu
7. Dialog potwierdzenia przed generowaniem ręcznego podsumowania
8. Alpine.js do obsługi stanów UI, modali, walidacji i interakcji infinite scroll
9. Komunikaty zachęcające na pustych stronach
10. Ikona z licznikiem nowych podsumowań przy nazwie sekcji Podsumowania
11. Konsekwentny, minimalistyczny styl przycisków i interfejsu w stylu Material Design
12. Kolory interfejsu inspirowane starym papierem
13. Walidacja formularzy po stronie klienta z wykorzystaniem Alpine.js
14. Wyłączanie przycisku dodawania notatki po osiągnięciu limitu 100 notatek
15. Przyciski oceny podsumowania (kciuk góra/dół) umieszczone na dole strony
</matched_recommendations>

<ui_architecture_planning_summary>
## Ogólna struktura aplikacji

MindfulDigger będzie aplikacją webową opartą na .NET 8 (Razor Pages/MVC) z Alpine.js do interaktywności i Tailwind CSS do stylizacji. Interfejs użytkownika będzie minimalistyczny, inspirowany Material Design, z kolorystyką starego papieru, co współgra z koncepcją aplikacji do prowadzenia notatek i introspekcji.

## Nawigacja i główne widoki

1. **Główna nawigacja** - górny pasek z trzema sekcjami:
   - Notatki
   - Podsumowania (z ikoną licznika nowych podsumowań)
   - Konto

2. **Strona Notatek** zawiera:
   - Trzykolumnową siatkę (grid-cols-3) z kartami notatek o jednakowej wysokości
   - Każda karta wyświetla datę utworzenia i fragment treści (do 120 znaków)
   - Infinite scroll ładujący więcej notatek po dotarciu do końca
   - Przycisk do tworzenia nowej notatki (dezaktywowany po osiągnięciu limitu 100)
   - Modal tworzenia notatki zajmujący 50% szerokości ekranu
   - Komunikat "Proszę dodaj 1 notatkę" przy pustej liście

3. **Strona Podsumowań** zawiera:
   - Listę jednowymiarową podsumowań
   - Dla każdego podsumowania: data generowania i krótki opis okresu
   - Komunikat "Proszę wygeneruj pierwsze podsumowanie" przy pustej liście
   - Przycisk do ręcznego generowania podsumowania
   - Dialog potwierdzenia przed generowaniem

4. **Strona szczegółów Podsumowania**:
   - Pełna treść podsumowania w formacie markdown
   - Przyciski oceny (kciuk góra/dół) na dole strony
   - Bez wizualnego rozróżnienia między automatycznymi a ręcznymi podsumowaniami

5. **Strona szczegółów Notatki**:
   - Pełna treść notatki

## Interakcje i przepływy

1. **Tworzenie notatki**:
   - Kliknięcie przycisku otwiera modal zajmujący 50% szerokości ekranu
   - Brak licznika znaków, komunikat pojawia się tylko przy przekroczeniu limitu 1000 znaków
   - Walidacja po stronie klienta przed wysłaniem
   - Po zapisaniu, notatka pojawia się na górze listy

2. **Przeglądanie notatek**:
   - Infinite scroll automatycznie ładuje więcej notatek
   - Kliknięcie w notatkę przenosi do strony z pełną treścią

3. **Generowanie podsumowania**:
   - Przycisk otwiera dialog z opcjami wyboru okresu
   - Dialog potwierdzenia "Czy na pewno chcesz wygenerować podsumowanie dla wybranego okresu?"
   - Prosty spinner podczas generowania

4. **Przeglądanie podsumowań**:
   - Kliknięcie w podsumowanie przenosi do osobnej strony (nie do modalu)
   - Możliwość oceny podsumowania (kciuk góra/dół)

5. **Powiadomienia**:
   - Licznik przy nazwie sekcji "Podsumowania" informujący o nowych podsumowaniach

## Integracja z API

1. Pobieranie notatek przez endpoint GET /notes z infinite scroll
2. Tworzenie notatek przez endpoint POST /notes z formularza w modalu
3. Pobieranie podsumowań przez endpoint GET /summaries
4. Generowanie podsumowań przez endpoint POST /summaries/generate
5. Przekazywanie ocen podsumowań przez endpoint POST /feedback

## Zarządzanie stanem

Alpine.js będzie wykorzystywany do zarządzania stanem UI, w tym:
- Otwieranie/zamykanie modali
- Walidacja formularzy
- Zarządzanie infinite scroll
- Wyświetlanie animacji ładowania (spinner)
- Obsługa powiadomień o nowych podsumowaniach
</ui_architecture_planning_summary>

<unresolved_issues>
1. Szczegółowy sposób implementacji powiadomień o nowych automatycznych podsumowaniach (tylko licznik czy również dodatkowe powiadomienia)
2. Dokładna kolorystyka "starego papieru" i jej implementacja w Tailwind CSS
3. Sposób prezentacji błędów API (poza ogólnym stwierdzeniem "pokazuj tylko informację, że był błąd")
4. Szczegóły implementacji walidacji po stronie klienta z użyciem Alpine.js
5. Dokładny układ i projekt strony konta użytkownika
6. Sposób obsługi animacji przejść między widokami (poza modalnymi)
7. Dokładna implementacja renderowania markdown dla podsumowań
</unresolved_issues>
</conversation_summary>