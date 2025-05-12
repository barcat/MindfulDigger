# Plan implementacji widoku Listy Notatek

## 1. Przegląd
Widok ma umożliwiać użytkownikowi przegląd listy własnych notatek w układzie trzykolumnowym. Użytkownik może wyświetlić fragment treści notatek (do 120 znaków) wraz z datą, a także dodać nową notatkę przez modal. Widok powinien być responsywny, czytelny i przyjazny dla klawiatury.

## 2. Routing widoku
- **Ścieżka:** `/notes`
- Widok dostępny tylko dla zalogowanych użytkowników.
- Po wejściu na ścieżkę `/notes` ładowana jest lista notatek.

## 3. Struktura komponentów
```
NotesPage (kontener widoku)
└── NotesGrid (prezentacja listy notatek w gridzie trzykolumnowym)
    └── NoteCard (pojedyncza karta notatki)
└── CreateNoteModal (modal do tworzenia nowej notatki)
```

## 4. Szczegóły komponentów

### NotesPage
- **Opis:** Główny kontener widoku. Inicjuje pobranie notatek i zarządza stanem całej listy. Obsługuje logikę infinite scroll, otwieranie i zamykanie modala tworzenia notatek.
- **Główne elementy:**  
  - Przycisk „Dodaj notatkę”  
  - Obszar na listę notatek (NotesGrid)  
  - Modal tworzenia notatki (CreateNoteModal), otwierany warunkowo  
- **Obsługiwane interakcje:**  
  - Kliknięcie przycisku „Dodaj notatkę” → otwarcie modala  
  - Przewijanie listy → wczytywanie kolejnych notatek (infinite scroll)  
- **Obsługiwana walidacja:**  
  - Weryfikacja limitu 100 notatek (dezaktywowanie przycisku)  
- **Typy:**  
  - Brak nowych typów w samym komponencie (korzysta z tipi zdefiniowanych w sekcjach niżej)  
- **Propsy:** Brak (pełni rolę widoku/strony)

### NotesGrid
- **Opis:** Odpowiedzialny za prezentację notatek w gridzie trzykolumnowym. Przyjmuje kolekcję notatek i wyświetla je w pętli jako karty.
- **Główne elementy:**  
  - Kontener z klasami Tailwind (np. `grid grid-cols-1 md:grid-cols-3 gap-4`)  
  - Lista NoteCard  
- **Obsługiwane interakcje:**  
  - Kliknięcie na kartę notatki → przejście do widoku szczegółu (np. link do `/notes/{id}`)  
- **Walidacja:** Brak bezpośredniej walidacji (formatowanie i filtry danych realizowane wyżej).  
- **Typy:**  
  - `NoteListItem` (zdefiniowany w sekcji 5)  
- **Propsy:**  
  - `notes: NoteListItem[]`

### NoteCard
- **Opis:** Pojedyncza karta notatki z datą i fragmentem treści.  
- **Główne elementy:**  
  - `div` z metadanymi notatki (data, fragment treści)  
  - Link/klikany element prowadzący do szczegółu notatki  
- **Obsługiwane interakcje:**  
  - Kliknięcie → nawigacja do `/notes/{id}`  
- **Walidacja:** Brak samodzielnej walidacji.  
- **Typy:**  
  - Wykorzystuje `NoteListItem`.  
- **Propsy:**  
  - `note: NoteListItem`

### CreateNoteModal
- **Opis:** Modal umożliwiający utworzenie nowej notatki (zawiera textarea i przycisk „Zapisz”).  
- **Główne elementy:**  
  - `textarea` na treść notatki  
  - Przycisk „Zapisz” i przycisk „Anuluj”  
- **Obsługiwane interakcje:**  
  - Kliknięcie „Zapisz” → walidacja treści, wywołanie endpointu POST  
  - Kliknięcie „Anuluj” lub tło modala → zamknięcie modala  
- **Walidacja:**  
  - Treść nie może być pusta  
  - Treść nie przekracza 1000 znaków  
  - Limit 100 notatek  
- **Typy:**  
  - `CreateNotePayload` (zdefiniowany w sekcji 5)  
- **Propsy:**  
  - `isOpen: boolean`  
  - `onClose: () => void`  
  - `onNoteCreated: (note: NoteListItem) => void` (służy np. do odświeżenia listy)

## 5. Typy

### NoteListItem
```csharp
public class NoteListItem
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public string ContentSnippet { get; set; } = string.Empty;
}
```
- Pola zgodne z odpowiedzią z endpointu GET `/notes`.

### CreateNotePayload
```csharp
public class CreateNotePayload
{
    public string Content { get; set; } = string.Empty;
}  content: string; // Treść nowej notatki
```
- Wykorzystywane do wysłania żądania POST `/notes`.

## 6. Zarządzanie stanem
- **NotesPage** przechowuje:  
  - tablicę `notes` (NoteListItem)  
  - aktualną stronę `currentPage`  
  - stan modala `isModalOpen`  
- **Metody**:  
  - `fetchNotes` – pobiera kolejną stronę notatek  
  - `handleCreateNote` – wywołuje POST, a po sukcesie dodaje nową notatkę na górę listy (o ile limit nie został przekroczony)

## 7. Integracja API
- **GET /notes**:  
  - Parametry query: `page`, `pageSize`  
  - Używane do pobierania listy notatek przy uruchomieniu widoku i przy infinite scroll.  
- **POST /notes**:  
  - Tworzy nową notatkę, dane w formacie `CreateNotePayload`.  
  - Po pomyślnym utworzeniu zwraca nowoutworzoną notatkę z identyfikatorem i snippetem.

## 8. Interakcje użytkownika
1. Użytkownik wchodzi na `/notes` → widzi listę notatek (lub komunikat o pustej liście).  
2. Przewija w dół listy → ładowane są kolejne notatki (infinite scroll).  
3. Klika „Dodaj notatkę” → otwiera się modal.  
4. Wpisuje treść i klika „Zapisz” → notatka wysyłana do API, po sukcesie modal się zamyka i lista się aktualizuje.  
5. Kliknięcie na notatkę → przejście do widoku szczegółowego notatki.

## 9. Warunki i walidacja
- **Limit 100 notatek**: po przekroczeniu przycisk „Dodaj notatkę” nieaktywny, w modalu pojawia się komunikat o braku możliwości dodania.  
- **Treść notatki**:  
  - Wymagana, do 1000 znaków.  
  - Weryfikacja w kliencie i ponownie w API.  
- **Stronicowanie**: `page > 0, pageSize <= 100`.

## 10. Obsługa błędów
- Błędy walidacji: wyświetlać komunikat w modalu (np. "Notatka przekracza 1000 znaków").  
- Błędy api (4xx, 5xx): wyświetlić komunikat o błędzie i ewentualnie logi w konsoli.  
- Brak autoryzacji (401): przekierowanie do logowania.  
- Limit notatek (400 z API): wyświetlić powiadomienie i zamknąć modal.

## 11. Kroki implementacji
1. Utworzyć/zmodyfikować plik widoku `NotesPage` (Razor Page lub komponent) i skonfigurować routing na `/notes`.  
2. Zaimplementować w `NotesPage` logikę pobierania listy notatek z użyciem GET `/notes` (z parametrami stronicowania).  
3. Zaimplementować mechanizm infinite scroll (np. zdarzenie scroll w Alpine.js).  
4. Stworzyć komponent `NotesGrid`, renderujący listę notatek jako karty.  
5. Dodać komponent `NoteCard`, prezentujący pojedynczą notatkę (data + snippet).  
6. Utworzyć `CreateNoteModal` z polami i przyciskami. Obsłużyć walidację 1000 znaków i wywołanie POST `/notes`.  
7. Dodać obsługę limitu 100 notatek (wyłączony przycisk „Dodaj notatkę” oraz obsługa błędu 400).  
8. Związać modal z logiką stanu w `NotesPage` tak, aby po sukcesie dodać nową notatkę na początku listy.  
9. Przetestować na różnych rozdzielczościach ekranu (responsywność).  
10. Zweryfikować dostępność (nawigacja klawiaturą, focus na elemencie w modalu).  
11. Dodać ewentualne komunikaty błędów i obsługi wyjątków.  
12. Finalna weryfikacja z PRD i wymaganiami.  
