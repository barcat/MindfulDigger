-- migration: 20250509120000_add_feedback_rls.sql
-- description: Adds Row Level Security (RLS) policies for feedback table
-- affected tables: feedback
-- special considerations: Updates existing RLS policies for feedback table

-- Drop existing RLS policies if they exist
DROP POLICY IF EXISTS "Allow users to view their own feedback" ON public.feedback;
DROP POLICY IF EXISTS "Allow users to insert their own feedback" ON public.feedback;

-- Create updated RLS policies
CREATE POLICY "Allow users to view their own feedback"
ON public.feedback FOR SELECT
USING (auth.uid() = user_id);
COMMENT ON POLICY "Allow users to view their own feedback" ON public.feedback IS 'Authenticated users can select their own feedback records.';

CREATE POLICY "Allow users to insert their own feedback"
ON public.feedback FOR INSERT
WITH CHECK (auth.uid() = user_id);
COMMENT ON POLICY "Allow users to insert their own feedback" ON public.feedback IS 'Authenticated users can insert their own feedback records.';

-- Add policy to allow users to update their feedback
CREATE POLICY "Allow users to update their own feedback"
ON public.feedback FOR UPDATE
USING (auth.uid() = user_id)
WITH CHECK (auth.uid() = user_id);
COMMENT ON POLICY "Allow users to update their own feedback" ON public.feedback IS 'Authenticated users can update their own feedback records.';

-- Add policy to allow users to delete their feedback
CREATE POLICY "Allow users to delete their own feedback"
ON public.feedback FOR DELETE
USING (auth.uid() = user_id);
COMMENT ON POLICY "Allow users to delete their own feedback" ON public.feedback IS 'Authenticated users can delete their own feedback records.';