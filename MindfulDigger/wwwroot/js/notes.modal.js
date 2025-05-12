export function openModal(context) {
    context.isModalOpen = true;
    resetModalState(context);
    setTimeout(() => {
        const textarea = document.querySelector('textarea');
        if (textarea) textarea.focus();
    }, 100);
}

export function closeModal(context) {
    if (context.isSubmitting) return;
    context.isModalOpen = false;
    context.newNoteContent = '';
    context.errorMessage = '';
}

export function validateContent(context) {
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

export function resetModalState(context) {
    context.newNoteContent = '';
    context.errorMessage = '';
    context.isValidContent = false;
}
