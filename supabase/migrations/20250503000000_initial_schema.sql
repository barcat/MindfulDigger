-- migration: 20250503000000_initial_schema.sql
-- description: Creates the initial database schema including types, tables, indexes, and RLS policies for notes, summaries, and feedback.
-- affected tables: notes, summaries, feedback
-- special considerations: Enables Row Level Security (RLS) for all created tables.

-- create custom types

-- create feedback_rating enum type
create type public.feedback_rating as enum ('positive', 'negative');
comment on type public.feedback_rating is 'Enum type for feedback ratings (positive or negative).';

-- create tables

-- create notes table
create table public.notes (
  id uuid primary key default gen_random_uuid(),
  user_id uuid not null references auth.users(id) on delete cascade,
  content text not null check (char_length(content) <= 1000),
  creation_date timestamptz not null default now()
);
comment on table public.notes is 'Stores user notes.';

-- create summaries table
create table public.summaries (
  id uuid primary key default gen_random_uuid(),
  user_id uuid not null references auth.users(id) on delete cascade,
  content text not null,
  generation_date timestamptz not null default now(),
  period_description text not null, -- description of the period (e.g., "last 7 days", "last 10 notes")
  period_start timestamptz null, -- start of the period (if applicable)
  period_end timestamptz null, -- end of the period (if applicable)
  is_automatic boolean not null default false -- whether the summary was generated automatically (true) or manually (false)
);
comment on table public.summaries is 'Stores summaries of notes generated by llm.';

-- create feedback table
create table public.feedback (
  summary_id uuid not null references public.summaries(id) on delete cascade,
  user_id uuid not null references auth.users(id) on delete cascade,
  rating public.feedback_rating not null,
  creation_date timestamptz not null default now(),
  primary key (summary_id, user_id) -- a user can rate a given summary only once
);
comment on table public.feedback is 'Stores user feedback on summaries.';

-- create indexes

-- indexes for notes table
create index notes_user_id_idx on public.notes (user_id);
create index notes_creation_date_idx on public.notes (creation_date desc);

-- indexes for summaries table
create index summaries_user_id_idx on public.summaries (user_id);
create index summaries_generation_date_idx on public.summaries (generation_date desc);

-- indexes for feedback table
create index feedback_user_id_idx on public.feedback (user_id);
create index feedback_summary_id_idx on public.feedback (summary_id);

-- enable row level security (rls)

-- enable rls for notes table
alter table public.notes enable row level security;

create policy "Allow users to view their own notes"
on public.notes for select
using (auth.uid() = user_id);
comment on policy "Allow users to view their own notes" on public.notes is 'Authenticated users can select their own notes.';

create policy "Allow users to insert their own notes"
on public.notes for insert
with check (auth.uid() = user_id);
comment on policy "Allow users to insert their own notes" on public.notes is 'Authenticated users can insert their own notes.';

-- enable rls for summaries table
alter table public.summaries enable row level security;

create policy "Allow users to view their own summaries"
on public.summaries for select
using (auth.uid() = user_id);
comment on policy "Allow users to view their own summaries" on public.summaries is 'Authenticated users can select their own summaries.';

create policy "Allow users to insert their own summaries"
on public.summaries for insert
with check (auth.uid() = user_id);
comment on policy "Allow users to insert their own summaries" on public.summaries is 'Authenticated users can insert their own summaries.';

-- enable rls for feedback table
alter table public.feedback enable row level security;

create policy "Allow users to view their own feedback"
on public.feedback for select
using (auth.uid() = user_id);
comment on policy "Allow users to view their own feedback" on public.feedback is 'Authenticated users can select their own feedback.';

create policy "Allow users to insert their own feedback"
on public.feedback for insert
with check (auth.uid() = user_id);
comment on policy "Allow users to insert their own feedback" on public.feedback is 'Authenticated users can insert their own feedback.';
