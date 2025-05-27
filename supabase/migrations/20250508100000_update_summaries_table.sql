-- Add created_at column to summaries table
ALTER TABLE public.summaries
ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ NOT NULL DEFAULT now();