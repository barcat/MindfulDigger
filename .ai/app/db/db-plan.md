# Schemat Bazy Danych PostgreSQL dla Notatek AI (MVP)

## 1. Typy niestandardowe

### `feedback_rating`
Typ wyliczeniowy dla ocen podsumowań.
```sql
CREATE TYPE public.feedback_rating AS ENUM ('positive', 'negative');
```

## 2. Tabele

### `notes`
Przechowuje notatki użytkowników.
```sql
CREATE TABLE public.notes (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id uuid NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  content text NOT NULL CHECK (char_length(content) <= 1000), -- Limit 1000 znaków (dodatkowe zabezpieczenie, główna logika w backendzie)
  creation_date timestamptz NOT NULL DEFAULT now()
);
COMMENT ON TABLE public.notes IS 'Przechowuje notatki użytkowników.';
```

### `summaries`
Przechowuje podsumowania notatek generowane przez LLM.
```sql
CREATE TABLE public.summaries (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id uuid NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  content text NOT NULL,
  generation_date timestamptz NOT NULL DEFAULT now(),
  period_description text NOT NULL, -- Opis okresu (np. "Ostatnie 7 dni", "10 ostatnich notatek")
  period_start timestamptz NULL, -- Początek okresu (jeśli dotyczy)
  period_end timestamptz NULL, -- Koniec okresu (jeśli dotyczy)
  is_automatic boolean NOT NULL DEFAULT false -- Czy podsumowanie zostało wygenerowane automatycznie (true) czy ręcznie (false)
);
COMMENT ON TABLE public.summaries IS 'Przechowuje podsumowania notatek generowane przez LLM.';
```

### `feedback`
Przechowuje oceny podsumowań wystawione przez użytkowników.
```sql
CREATE TABLE public.feedback (
  summary_id uuid NOT NULL REFERENCES public.summaries(id) ON DELETE CASCADE,
  user_id uuid NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  rating public.feedback_rating NOT NULL,
  creation_date timestamptz NOT NULL DEFAULT now(),
  PRIMARY KEY (summary_id, user_id) -- Użytkownik może ocenić dane podsumowanie tylko raz
);
COMMENT ON TABLE public.feedback IS 'Przechowuje oceny podsumowań wystawione przez użytkowników.';
```

## 3. Relacje

*   `auth.users` (1) ---< `notes` (M): Jeden użytkownik może mieć wiele notatek. `ON DELETE CASCADE`.
*   `auth.users` (1) ---< `summaries` (M): Jeden użytkownik może mieć wiele podsumowań. `ON DELETE CASCADE`.
*   `auth.users` (1) ---< `feedback` (M): Jeden użytkownik może wystawić wiele ocen (dla różnych podsumowań). `ON DELETE CASCADE`.
*   `summaries` (1) ---< `feedback` (M): Jedno podsumowanie może mieć 1 ocene (od 1 użytkownia). `ON DELETE CASCADE`.

## 4. Indeksy

### `notes`
*   `notes_user_id_idx`: Na `user_id` (dla zapytań filtrujących wg użytkownika i dla FK).
*   `notes_creation_date_idx`: Na `creation_date` (dla sortowania).
```sql
CREATE INDEX notes_user_id_idx ON public.notes (user_id);
CREATE INDEX notes_creation_date_idx ON public.notes (creation_date DESC);
```

### `summaries`
*   `summaries_user_id_idx`: Na `user_id` (dla zapytań filtrujących wg użytkownika i dla FK).
*   `summaries_generation_date_idx`: Na `generation_date` (dla sortowania).
```sql
CREATE INDEX summaries_user_id_idx ON public.summaries (user_id);
CREATE INDEX summaries_generation_date_idx ON public.summaries (generation_date DESC);
```

### `feedback`
*   `feedback_user_id_idx`: Na `user_id` (dla zapytań filtrujących wg użytkownika i dla FK).
*   `feedback_summary_id_idx`: Na `summary_id` (dla FK).
```sql
CREATE INDEX feedback_user_id_idx ON public.feedback (user_id);
CREATE INDEX feedback_summary_id_idx ON public.feedback (summary_id);
```

### `auth.users`
*   Indeks na `last_sign_in_at` jest zarządzany przez Supabase Auth, ale jest kluczowy dla procesu retencji danych.

## 5. Zasady PostgreSQL (Row Level Security - RLS)

RLS zostanie włączone dla tabel `notes`, `summaries` i `feedback`.

### `notes`
```sql
-- Włączenie RLS
ALTER TABLE public.notes ENABLE ROW LEVEL SECURITY;

-- Polityka: Użytkownicy mogą widzieć tylko swoje notatki
CREATE POLICY "Allow users to view their own notes"
ON public.notes FOR SELECT
USING (auth.uid() = user_id);

-- Polityka: Użytkownicy mogą dodawać swoje notatki
CREATE POLICY "Allow users to insert their own notes"
ON public.notes FOR INSERT
WITH CHECK (auth.uid() = user_id);

```

### `summaries`
```sql
-- Włączenie RLS
ALTER TABLE public.summaries ENABLE ROW LEVEL SECURITY;

-- Polityka: Użytkownicy mogą widzieć tylko swoje podsumowania
CREATE POLICY "Allow users to view their own summaries"
ON public.summaries FOR SELECT
USING (auth.uid() = user_id);

-- Polityka: Użytkownicy (lub system w ich imieniu) mogą dodawać swoje podsumowania
CREATE POLICY "Allow users to insert their own summaries"
ON public.summaries FOR INSERT
WITH CHECK (auth.uid() = user_id);
```

### `feedback`
```sql
-- Włączenie RLS
ALTER TABLE public.feedback ENABLE ROW LEVEL SECURITY;

-- Polityka: Użytkownicy mogą widzieć tylko swoje oceny
CREATE POLICY "Allow users to view their own feedback"
ON public.feedback FOR SELECT
USING (auth.uid() = user_id);

-- Polityka: Użytkownicy mogą dodawać swoje oceny
CREATE POLICY "Allow users to insert their own feedback"
ON public.feedback FOR INSERT
WITH CHECK (auth.uid() = user_id);


```

## 6. Dodatkowe uwagi

*   **Limity:** Limit 100 notatek na użytkownika oraz limit 1000 znaków na notatkę będą egzekwowane głównie w logice aplikacji backendowej (.NET 8). Ograniczenie `CHECK` w tabeli `notes` jest dodatkowym zabezpieczeniem na poziomie bazy danych.
*   **Retencja danych:** Usuwanie nieaktywnych kont (po 3 miesiącach braku logowania, zgodnie z WF-016) będzie realizowane przez dedykowany proces backendowy, który będzie odpytywał tabelę `auth.users` (filtrując po `last_sign_in_at`) i usuwał odpowiednich użytkowników. Dzięki `ON DELETE CASCADE` powiązane dane w `notes`, `summaries` i `feedback` zostaną automatycznie usunięte.
*   **Supabase Auth:** Tabela `auth.users` jest zarządzana przez Supabase i zawiera dodatkowe kolumny (np. `email`, `encrypted_password`, `created_at`, `updated_at`, `last_sign_in_at`). Schemat skupia się na tabelach aplikacji.
*   **Normalizacja:** Schemat jest zgodny z 3NF.
*   **Przyszły rozwój:** Schemat jest zaprojektowany z myślą o MVP. Przyszłe funkcje (np. tagowanie, wyszukiwanie) mogą wymagać modyfikacji lub dodania nowych tabel/indeksów. Polityki RLS dla `UPDATE` i `DELETE` zostały zakomentowane, ponieważ nie są wymagane przez obecne historyjki użytkownika, ale mogą być potrzebne w przyszłości.
