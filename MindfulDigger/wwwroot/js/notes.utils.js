export function showSuccessNotification(message) {
    window.dispatchEvent(new CustomEvent('notify', {
        detail: {
            message: message,
            type: 'success',
            duration: 3000
        }
    }));
}

export function goToNoteDetails(noteId) {
    window.location.href = `/notes/${noteId}`;
}

export function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('pl-PL', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}
