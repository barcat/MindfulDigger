// login.supabase.js
// Logika logowania przez API backendu z Alpine.js
// Usunięto lokalne stałe SUPABASE_URL i SUPABASE_KEY

// Alpine.js store do logowania

document.addEventListener('alpine:init', () => {
    Alpine.data('loginForm', () => ({
        email: '',
        password: '',
        error: '',
        async login() {
            this.error = '';
            if (!this.email || !this.password) {
                this.error = 'Podaj email i hasło.';
                return;
            }

            try {
                const response = await fetch('/api/Auth/login', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ email: this.email, password: this.password })
                });

                if (response.ok) {
                    await this.handleSuccess(response);
                } else {
                    await this.handleError(response);
                }
            } catch (e) {
                this.handleUnexpectedError(e);
            }
        },
        async handleSuccess(response) {
            const data = await response.json();
            localStorage.setItem('jwt_token', data.token);
            window.location.href = '/notes';
        },
        async handleError(response) {
            const errorData = await response.json();
            this.error = errorData.message || `Błąd logowania: ${response.status}`;
        },
        handleUnexpectedError(e) {
            this.error = 'Wystąpił nieoczekiwany błąd. Spróbuj ponownie.';
            console.error('Login error:', e);
        }
    }));
});
