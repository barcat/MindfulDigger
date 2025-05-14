document.addEventListener('alpine:init', () => {
    Alpine.data('registerForm', () => ({
        email: '',
        password: '',
        confirmPassword: '',
        emailError: '',
        passwordError: '',
        confirmPasswordError: '',
        error: '',
        success: '',
        async register() {
            this.emailError = '';
            this.passwordError = '';
            this.confirmPasswordError = '';
            this.error = '';
            this.success = '';
            if (!this.validateEmail(this.email)) {
                this.emailError = 'Nieprawidłowy format e-mail.';
                return;
            }
            if (!this.validatePassword(this.password)) {
                this.passwordError = 'Hasło musi mieć min. 8 znaków, 1 cyfrę i 1 znak specjalny.';
                return;
            }
            if (this.password !== this.confirmPassword) {
                this.confirmPasswordError = 'Hasła nie są zgodne.';
                return;
            }
            try {
                const res = await fetch('/api/auth/register', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ email: this.email, password: this.password, confirmPassword: this.confirmPassword })
                });
                const data = await res.json();
                if (!res.ok) {
                    this.error = data.message || 'Błąd rejestracji.';
                } else {
                    this.success = 'Rejestracja zakończona sukcesem. Sprawdź e-mail.';
                }
            } catch (e) {
                this.error = 'Błąd połączenia z serwerem.';
            }
        },
        validateEmail(email) {
            return /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email);
        },
        validatePassword(password) {
            return /^(?=.*[0-9])(?=.*[!@#$%^&*])[A-Za-z\d!@#$%^&*]{8,}$/.test(password);
        }
    }));
});
