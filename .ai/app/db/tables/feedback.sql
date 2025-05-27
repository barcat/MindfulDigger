-- add enum type for feedback_rating
CREATE TYPE public.feedback_rating AS ENUM ('positive', 'negative');

-- ...new file...
CREATE TABLE public.feedback (
  summary_id uuid NOT NULL REFERENCES public.summaries(id) ON DELETE CASCADE,
  user_id uuid NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  rating public.feedback_rating NOT NULL,
  creation_date timestamptz NOT NULL DEFAULT now(),
  PRIMARY KEY (summary_id, user_id)
);
COMMENT ON TABLE public.feedback IS 'Przechowuje oceny podsumowań wystawione przez użytkowników.';
CREATE INDEX feedback_user_id_idx ON public.feedback (user_id);
CREATE INDEX feedback_summary_id_idx ON public.feedback (summary_id);