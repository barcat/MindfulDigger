<database_planning_summary>
Na podstawie wymagań produktu (PRD) i dyskusji zaplanowano schemat bazy danych PostgreSQL dla MVP aplikacji Notatki AI, wykorzystując Supabase.

**Główne wymagania dotyczące schematu:**
*   **Użytkownicy:** Zarządzani przez `auth.users` Supabase. Kluczowe pole: `id (UUID)`, `last_sign_in_at (TIMESTAMPTZ)`.
*   **Notatki (`notes`):** Przechowują zapiski użytkowników. Kluczowe pola: `id (UUID PK)`, `user_id (UUID FK > auth.users)`, `content (TEXT)`, `creation_date (TIMESTAMPTZ DEFAULT NOW())`. Limit 100 notatek i 1000 znaków na notatkę egzekwowany w backendzie.
*   **Podsumowania (`summaries`):** Przechowują podsumowania LLM. Kluczowe pola: `id (UUID PK)`, `user_id (UUID FK > auth.users)`, `content (TEXT)`, `generation_date (TIMESTAMPTZ DEFAULT NOW())`, `period_description (TEXT)`, `period_start (TIMESTAMPTZ NULL)`, `period_end (TIMESTAMPTZ NULL)`, `is_automatic (BOOLEAN NOT NULL DEFAULT FALSE)`.
*   **Opinie (`feedback`):** Przechowują oceny podsumowań udzielone przez użytkowników. Rekord jest tworzony tylko wtedy, gdy użytkownik aktywnie oceni podsumowanie. Kluczowe pola: `summary_id (UUID FK > summaries)`, `user_id (UUID FK > auth.users)`, `rating (feedback_rating NOT NULL)`, `creation_date (TIMESTAMPTZ DEFAULT NOW())`. Klucz główny złożony: `(summary_id, user_id)`. Typ `feedback_rating` to `ENUM ('positive', 'negative')`.

**Kluczowe encje i ich relacje:**
*   `auth.users` (1) <-> `notes` (M)
*   `auth.users` (1) <-> `summaries` (M)
*   `auth.users` (1) <-> `feedback` (M)
*   `summaries` (1) <-> `feedback` (M)
Wszystkie relacje z `auth.users` i `summaries` używają `ON DELETE CASCADE` dla zapewnienia integralności danych przy usuwaniu użytkownika lub podsumowania.

**Bezpieczeństwo i Skalowalność:**
*   **RLS:** Polityki Row Level Security zostaną włączone dla tabel `notes`, `summaries` i `feedback`, ograniczając dostęp (`SELECT`, `INSERT`, `UPDATE`, `DELETE`) każdego użytkownika wyłącznie do jego własnych rekordów (`USING (auth.uid() = user_id)`).
*   **Indeksowanie:** Zostaną utworzone indeksy na kluczach obcych, datach tworzenia/generowania (dla sortowania) oraz na `auth.users.last_sign_in_at` (dla procesu retencji).
*   **Retencja:** Polityka usuwania nieaktywnych kont (po 3 miesiącach braku logowania) będzie realizowana przez dedykowany proces backendowy, wykorzystujący indeks na `last_sign_in_at`.
*   **Limity:** Limity liczby notatek i długości treści będą zarządzane w logice aplikacji backendowej.

</database_planning_summary>
