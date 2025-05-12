// d:\git\MindfulDigger\MindfulDigger\wwwroot\js\notes.bundle.js
// --- BEGIN: notes.infiniteScroll.js ---
function setupInfiniteScroll(context) {
    const options = {
        root: null,
        rootMargin: '0px',
        threshold: 0.1
    };
    context.observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting && !context.isLoading && context.hasMorePages) {
                context.loadMoreNotes();
            }
        });
    }, options);
    const trigger = document.getElementById('scroll-trigger');
    if (trigger) {
        context.observer.observe(trigger);
    }
}
// --- END: notes.infiniteScroll.js ---

// --- BEGIN: notes.fetch.js ---
function fetchNotes(context) {
    if (context.isLoading || !context.hasMorePages) return;
    context.isLoading = true;
    const token = localStorage.getItem('jwt_token');
    fetch(`/api/notes?page=${context.currentPage}&pageSize=${context.pageSize}`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        credentials: 'same-origin'
    })
    .then(response => {
        if (response.status === 401) {
            window.location.href = '/Login';
            throw new Error('Unauthorized');
        }
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        return response.json();
    })
    .then(data => {
        const safeNotes = Array.isArray(context.notes) ? context.notes : [];
        const safeItems = Array.isArray(data.items) ? data.items : [];
        context.notes = [...safeNotes, ...safeItems];
        context.totalCount = data.pagination.totalCount;
        context.totalPages = data.pagination.totalPages;
        context.hasMorePages = context.currentPage < context.totalPages;
    })
    .catch(error => {
        console.error('Error fetching notes:', error);
    })
    .finally(() => {
        context.isLoading = false;
    });
}
function loadMoreNotes(context) {
    if (context.currentPage < context.totalPages) {
        context.currentPage++;
        context.fetchNotes();
    }
}
// --- END: notes.fetch.js ---

// --- BEGIN: notes.modal.js ---
function openModal(context) {
    context.isModalOpen = true;
    context.newNoteContent = '';
    context.errorMessage = '';
    context.isValidContent = false;
    setTimeout(() => {
        const textarea = document.querySelector('textarea');
        if (textarea) textarea.focus();
    }, 100);
}
function closeModal(context) {
    if (context.isSubmitting) return;
    context.isModalOpen = false;
    context.newNoteContent = '';
    context.errorMessage = '';
}
function validateContent(context) {
    context.isValidContent = context.newNoteContent.trim().length > 0 && 
                             context.newNoteContent.length <= 1000;
    if (context.newNoteContent.length > 1000) {
        context.errorMessage = 'Treść notatki nie może przekraczać 1000 znaków';
    } else if (context.newNoteContent.trim().length === 0) {
        context.errorMessage = 'Treść notatki jest wymagana';
    } else {
        context.errorMessage = '';
    }
}
// --- END: notes.modal.js ---

// --- BEGIN: notes.save.js ---
function saveNote(context) {
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
// --- END: notes.save.js ---

// --- BEGIN: notes.utils.js ---
function showSuccessNotification(message) {
    window.dispatchEvent(new CustomEvent('notify', {
        detail: {
            message: message,
            type: 'success',
            duration: 3000
        }
    }));
}
function goToNoteDetails(noteId) {
    window.location.href = `/notes/${noteId}`;
}
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('pl-PL', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}
// --- END: notes.utils.js ---

// --- BEGIN: notes.js (Alpine registration) ---
function logEvent(event, details = null) {
    if (details) {
        console.log(`[Notes] ${event}:`, details);
    } else {
        console.log(`[Notes] ${event}`);
    }
}

document.addEventListener('alpine:init', () => {
    Alpine.data('notesPage', (totalCount) => ({
        notes: [],
        currentPage: 1,
        pageSize: 15,
        totalCount: totalCount || 0,
        totalPages: 0,
        isLoading: false,
        isModalOpen: false,
        newNoteContent: '',
        isValidContent: false,
        isSubmitting: false,
        errorMessage: '',
        hasMorePages: true,
        observer: null,

        init() {
            logEvent('init');
            if (this.notes.length === 0) {
                this.fetchNotes();
            }
            this.setupInfiniteScroll();
        },
        setupInfiniteScroll() {
            logEvent('setupInfiniteScroll');
            setupInfiniteScroll(this);
        },
        fetchNotes() {
            logEvent('fetchNotes', { page: this.currentPage, pageSize: this.pageSize });
            fetchNotes(this);
        },
        loadMoreNotes() {
            logEvent('loadMoreNotes', { nextPage: this.currentPage + 1 });
            loadMoreNotes(this);
        },
        openModal() {
            logEvent('openModal');
            openModal(this);
        },
        closeModal() {
            logEvent('closeModal');
            closeModal(this);
        },
        validateContent() {
            logEvent('validateContent', { content: this.newNoteContent });
            validateContent(this);
        },
        saveNote() {
            logEvent('saveNote', { content: this.newNoteContent });
            saveNote(this);
        },
        showSuccessNotification,
        goToNoteDetails,
        formatDate
    }));
});
// --- END: notes.js ---
