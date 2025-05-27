// Alpine.js component for note detail page
import { showSuccessNotification } from './notes.utils.js';

document.addEventListener('alpine:init', () => {
    Alpine.data('noteDetailPage', () => ({
        note: null,
        isLoading: true,
        error: '',
        isDeleting: false,

        async init() {
            const noteId = this.getNoteIdFromUrl();
            await this.fetchNote(noteId);
        },

        getNoteIdFromUrl() {
            const id = window.location.pathname.split('/').pop();
            // Validate if ID is in correct GUID format
            const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
            if (!guidRegex.test(id)) {
                this.error = 'Invalid note ID format';
                return null;
            }
            return id;
        },

        getRequestOptions(method = 'GET') {
            return {
                method: method,
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                credentials: 'same-origin'
            };
        },

        handleResponseError(response) {
            if (response.status === 401) {
                window.location.href = '/Login';
                return true;
            }
            
            if (response.status === 404) {
                this.error = 'Note does not exist';
                return true;
            }

            if (!response.ok) {
                throw new Error('An error occurred while fetching the note');
            }
            
            return false;
        },

        createNoteFromResponse(data) {
            // Validate required fields
            if (!data || !data.id || !data.content || !data.creationDate) {
                throw new Error('Invalid note data received from server');
            }
            
            return {
                id: data.id,
                content: data.content,
                creationDate: data.creationDate
            };
        },

        getContentSnippet(content) {
            const maxLength = 50;
            return content.length > maxLength 
                ? content.substring(0, maxLength) + '...' 
                : content;
        },

        updatePageTitle(content) {
            const snippet = this.getContentSnippet(content);
            document.title = `${snippet} - Note Details`;
        },

        async fetchNote(id) {
            if (!id) {
                this.error = 'Invalid note ID';
                this.isLoading = false;
                return;
            }

            try {
                this.isLoading = true;
                this.error = '';
                
                const response = await fetch(`/api/notes/${id}`, this.getRequestOptions());
                
                if (this.handleResponseError(response)) {
                    return;
                }
                
                const data = await response.json();
                
                try {
                    this.note = this.createNoteFromResponse(data);
                    this.updatePageTitle(data.content);                } catch (parseError) {
                    console.error('Error parsing note data:', parseError);
                    console.error('Received data:', data);
                    this.error = 'Error processing note data';
                }
            } catch (error) {
                console.error('Error fetching note:', error);
                this.error = 'Failed to fetch note. Please try again later.';
            } finally {
                this.isLoading = false;
            }
        },

        goBack() {
            window.history.back();
        },        formatDate(dateString) {
            const date = new Date(dateString);
            return date.toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'short',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        },        
        
        async deleteNote() {
            const noteId = this.getNoteIdFromUrl();
            if (!noteId) {
                this.error = 'Nieprawidłowy identyfikator notatki';
                return;
            }
            
            if (!confirm('Czy na pewno chcesz usunąć tę notatkę?')) return;

            this.isDeleting = true;
            this.error = '';

            try {                const response = await fetch(`/api/notes/${noteId}`, this.getRequestOptions('DELETE'));

                // Status 400 - Nieprawidłowy format ID
                if (response.status === 400) {
                    const data = await response.json();
                    this.error = data.message || 'Nieprawidłowy format identyfikatora notatki';
                    return;
                }

                // Status 401 - Nieautoryzowany dostęp
                if (response.status === 401) {
                    window.location.href = '/Login';
                    return;
                }

                // Status 404 - Notatka nie istnieje
                if (response.status === 404) {
                    this.error = 'Notatka nie istnieje lub została już usunięta';
                    setTimeout(() => {
                        window.location.href = '/notes';
                    }, 2000); // Daj użytkownikowi czas na przeczytanie komunikatu
                    return;
                }

                // Status 204 - Pomyślnie usunięto
                if (response.status === 204) {
                    showSuccessNotification('Notatka została pomyślnie usunięta');
                    setTimeout(() => {
                        window.location.href = '/notes';
                    }, 2000);
                    return;
                }

                // Inne błędy
                if (!response.ok) {
                    const data = await response.json();
                    throw new Error(data.message || 'Wystąpił błąd podczas usuwania notatki');
                }
            } catch (error) {
                console.error('Error deleting note:', error);
                this.error = error.message || 'Wystąpił nieoczekiwany błąd podczas usuwania notatki';
            } finally {
                this.isDeleting = false;
            }
        }
    }));
});
