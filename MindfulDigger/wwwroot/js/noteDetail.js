// Alpine.js component for note detail page
document.addEventListener('alpine:init', () => {
    Alpine.data('noteDetailPage', () => ({
        note: null,
        isLoading: true,
        error: '',

        async init() {
            const noteId = window.location.pathname.split('/').pop();
            await this.fetchNote(noteId);
        },

        async fetchNote(id) {
            try {
                this.isLoading = true;
                this.error = '';
                
                const response = await fetch(`/api/notes/${id}`, {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                        'Accept': 'application/json'
                    },
                    credentials: 'same-origin'
                });

                if (response.status === 401) {
                    window.location.href = '/Login';
                    return;
                }

                if (response.status === 404) {
                    this.error = 'Notatka nie istnieje';
                    return;
                }

                if (!response.ok) {
                    throw new Error('Wystąpił błąd podczas pobierania notatki');
                }                const data = await response.json();
                this.note = {
                    id: data.id,
                    content: data.content,
                    creationDate: data.creationDate
                };
                
                // Aktualizacja tytułu strony
                const snippet = data.content.length > 50 
                    ? data.content.substring(0, 50) + '...' 
                    : data.content;
                document.title = `${snippet} - Szczegóły notatki`;
            } catch (error) {
                console.error('Error fetching note:', error);
                this.error = 'Nie udało się pobrać notatki';
            } finally {
                this.isLoading = false;
            }
        },

        goBack() {
            window.history.back();
        },

        formatDate(dateString) {
            const date = new Date(dateString);
            return date.toLocaleDateString('pl-PL', {
                year: 'numeric',
                month: 'short',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        }
    }));
});
