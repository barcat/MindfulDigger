document.addEventListener('alpine:init', () => {
    Alpine.data('loginForm', () => ({
        email: '',
        password: '',
        emailError: '',
        passwordError: '',
        error: '',
        async login() {
            this.emailError = '';
            this.passwordError = '';
            this.error = '';
            if (!this.validateEmail(this.email)) {
                this.emailError = 'Nieprawidłowy format e-mail.';
                return;
            }
            if (!this.password) {
                this.passwordError = 'Hasło jest wymagane.';
                return;
            }
            try {
                const res = await fetch('/api/auth/login', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ email: this.email, password: this.password })
                });
                const data = await res.json();
                if (!res.ok) {
                    this.error = data.message || 'Błąd logowania.';
                } else {
                    window.location.href = '/';
                }
            } catch (e) {
                this.error = 'Błąd połączenia z serwerem.';
            }
        },
        validateEmail(email) {
            return /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email);
        }
    }));
});
