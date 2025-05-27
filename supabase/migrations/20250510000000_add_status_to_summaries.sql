-- migration: 20250510000000_add_status_to_summaries.sql
-- description: Adds status column to summaries table to track summary generation status
-- affected tables: summaries
-- special considerations: None

-- Add status column to summaries table
ALTER TABLE public.summaries
ADD COLUMN IF NOT EXISTS status TEXT NULL;

COMMENT ON COLUMN public.summaries.status IS 'Status of the summary generation (e.g., "Pending", "Completed", "Failed")';
