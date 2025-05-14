import { setupInfiniteScroll } from './notes.infiniteScroll.js';
import { fetchNotes, loadMoreNotes } from './notes.fetch.js';
import { openModal, closeModal, validateContent, handleSuccessfulSubmit } from './notes.modal.js';
import { saveNote } from './notes.save.js';
import { showSuccessNotification, goToNoteDetails, formatDate } from './notes.utils.js';

// Prosta funkcja logowania
function logEvent(event, details = null) {
    if (details) {
        console.log(`[Notes] ${event}:`, details);
    } else {
        console.log(`[Notes] ${event}`);
    }
}

// Alpine.js registration

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
        handleSuccessfulSubmit() {
            logEvent('handleSuccessfulSubmit');
            handleSuccessfulSubmit(this);
        },
        showSuccessNotification,
        goToNoteDetails,
        formatDate
    }));
});
