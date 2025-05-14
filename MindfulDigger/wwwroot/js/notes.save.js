import { resetModalState } from './notes.modal.js';

export async function saveNote(context) {
    // Validate the content before saving
    context.validateContent();
    
    if (!context.isValidContent) {
        return;
    }
    
    context.isSubmitting = true;
    
    try {
        const response = await fetch('/api/notes', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-TOKEN': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify({
                content: context.newNoteContent
            })
        });
        
        if (response.ok) {
            const data = await response.json();
            
            // Add the new note to the beginning of the notes array
            context.notes.unshift(data);
            context.totalCount++;
            
            // Show success notification
            context.showSuccessNotification('Notatka została dodana!');
            
            // Call handleSuccessfulSubmit which will close the modal
            context.handleSuccessfulSubmit();
        } else {
            const errorData = await response.json();
            context.errorMessage = errorData.message || 'Wystąpił błąd podczas zapisywania notatki.';
            context.isSubmitting = false;
        }
    } catch (error) {
        console.error('Error saving note:', error);
        context.errorMessage = 'Wystąpił błąd podczas zapisywania notatki.';
        context.isSubmitting = false;
    }
}
