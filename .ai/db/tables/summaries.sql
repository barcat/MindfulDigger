-- ...new file...
CREATE TABLE public.summaries (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id uuid NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  content text NOT NULL,
  generation_date timestamptz NOT NULL DEFAULT now(),
  period_description text NOT NULL,
  period_start timestamptz NULL,
  period_end timestamptz NULL,
  is_automatic boolean NOT NULL DEFAULT false
);
COMMENT ON TABLE public.summaries IS 'Przechowuje podsumowania notatek generowane przez LLM.';
CREATE INDEX summaries_user_id_idx ON public.summaries (user_id);
CREATE INDEX summaries_generation_date_idx ON public.summaries (generation_date DESC);