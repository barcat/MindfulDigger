// Alpine.js component for note detail page
document.addEventListener('alpine:init', () => {
    Alpine.data('noteDetailPage', () => ({
        note: null,
        isLoading: true,
        error: '',

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

        getRequestOptions() {
            return {
                method: 'GET',
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
        },

        formatDate(dateString) {
            const date = new Date(dateString);
            return date.toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'short',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        }
    }));
});
