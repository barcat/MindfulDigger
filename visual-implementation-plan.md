## 2. Konfiguracja Stylów CSS
- Utworzono plik `main.css`:
  - Zaimportowano Google Fonts:
    - Nagłówki: `Raleway`
    - Treść: `Georgia`
  - Ustalono schemat kolorów przez zmienne CSS:
    - Tło: `#F7EAD3`
    - Akcenty: `#E3B778` (terracotta) oraz `#A3BFD9` (delikatny błękit)
    - Tekst: `#3D3D3D`

## 3. Refaktoryzacja Układu Interfejsu
- Zmodyfikować pliki Razor (_Layout.cshtml, Index.cshtml, Notes.cshtml, itp.) aby:
  - Utrzymać minimalistyczny design – czyste linie, duże odstępy, responsywny layout.
  - Używać Tailwind CSS do budowania gridów, responsywnych przycisków i form.
  - Dopasować styl modali i powiadomień do nowych wytycznych.

## 4. Ulepszenie Typografii
- W plikach CSS (np. `site.css`, `notes.css`):
  - Dodać reguły ustawiające fonty, np.:
    ```css
    /* ...existing code... */
    h1, h2, h3, h4, h5, h6 {
      font-family: 'Raleway', sans-serif;
    }
    body, p, span, div {
      font-family: 'Georgia', serif;
      color: #3D3D3D;
    }
    /* ...existing code... */
    ```
    
## 5. Dodanie Efektu Faktury Tła
- Zaimplementowano subtelny efekt faktury używając CSS gradientu:
  ```css
  .texture-bg {
    background-image: linear-gradient(0deg, 
      rgba(255,255,255,0.05) 1px, 
      transparent 1px);
    background-size: 100% 2px;
  }
  ```

## 6. Sprawdzenie Integracji Alpine.js
- Upewnić się, że dynamiczne elementy (modal, paginacja, infinite scroll) działają poprawnie w nowym designie.
- Przetestować interakcje użytkownika.
