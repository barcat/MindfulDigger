-- ...new file...
CREATE TABLE public.notes (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id uuid NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  content text NOT NULL CHECK (char_length(content) <= 1000),
  creation_date timestamptz NOT NULL DEFAULT now()
);
COMMENT ON TABLE public.notes IS 'Przechowuje notatki użytkowników.';
CREATE INDEX notes_user_id_idx ON public.notes (user_id);
CREATE INDEX notes_creation_date_idx ON public.notes (creation_date DESC);