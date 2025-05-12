export function fetchNotes(context) {
    if (context.isLoading || !context.hasMorePages) return;
    context.isLoading = true;
    const token = localStorage.getItem('jwt_token'); // Pobierz token JWT

    fetch(`/api/notes?page=${context.currentPage}&pageSize=${context.pageSize}`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            'Authorization': `Bearer ${token}` // Dodaj token JWT do nagłówka
        },
        credentials: 'same-origin'
    })
    .then(response => {
        if (response.status === 401) {
            window.location.href = '/Login'; // Zmieniono ścieżkę przekierowania
            throw new Error('Unauthorized');
        }
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        return response.json();
    })
    .then(data => {
        // Zapewnij, że context.notes i data.items są zawsze tablicami
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

export function loadMoreNotes(context) {
    if (context.currentPage < context.totalPages) {
        context.currentPage++;
        context.fetchNotes();
    }
}
