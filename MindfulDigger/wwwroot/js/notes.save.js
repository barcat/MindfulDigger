import { resetModalState } from './notes.modal.js';

export function saveNote(context) {
    if (!context.isValidContent || context.isSubmitting || context.totalCount >= 100) return;
    context.isSubmitting = true;
    context.errorMessage = '';
    fetch('/api/notes', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        },
        credentials: 'same-origin',
        body: JSON.stringify({ content: context.newNoteContent })
    })
    .then(response => {
        if (response.status === 401) {
            window.location.href = '/Identity/Account/Login';
            throw new Error('Unauthorized');
        }
        if (response.status === 400) {
            return response.json().then(data => {
                throw new Error(data.message || 'Walidacja nie powiodła się');
            });
        }
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        return response.json();
    })
    .then(data => {
        context.notes.unshift({
            id: data.id,
            contentSnippet: data.contentSnippet,
            creationDate: data.creationDate
        });
        context.totalCount++;
        context.closeModal();
        resetModalState(context);
        context.showSuccessNotification('Notatka została dodana');
    })
    .catch(error => {
        console.error('Error creating note:', error);
        context.errorMessage = error.message || 'Wystąpił błąd podczas zapisywania notatki';
    })
    .finally(() => {
        context.isSubmitting = false;
    });
}
