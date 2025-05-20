-- Allow users to delete their own notes
create policy "Allow users to delete their own notes"
on public.notes for delete
using (auth.uid() = user_id);
comment on policy "Allow users to delete their own notes" on public.notes is 'Authenticated users can delete their own notes.';
