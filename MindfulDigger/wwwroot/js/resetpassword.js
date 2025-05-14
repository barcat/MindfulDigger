document.addEventListener('alpine:init', () => {
    Alpine.data('resetPasswordForm', () => ({
        password: '',
        confirmPassword: '',
        passwordError: '',
        confirmPasswordError: '',
        error: '',
        success: '',
        async resetPassword() {
            this.passwordError = '';
            this.confirmPasswordError = '';
            this.error = '';
            this.success = '';
            if (!this.validatePassword(this.password)) {
                this.passwordError = 'Hasło musi mieć min. 8 znaków, 1 cyfrę i 1 znak specjalny.';
                return;
            }
            if (this.password !== this.confirmPassword) {
                this.confirmPasswordError = 'Hasła nie są zgodne.';
                return;
            }
            try {
                // Pobierz token z query string
                const urlParams = new URLSearchParams(window.location.search);
                const token = urlParams.get('token');
                if (!token) {
                    this.error = 'Brak tokenu resetującego.';
                    return;
                }
                const res = await fetch('/api/auth/reset-password', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ token, password: this.password, confirmPassword: this.confirmPassword })
                });
                const data = await res.json();
                if (!res.ok) {
                    this.error = data.message || 'Błąd resetowania hasła.';
                } else {
                    this.success = 'Hasło zostało zmienione.';
                }
            } catch (e) {
                this.error = 'Błąd połączenia z serwerem.';
            }
        },
        validatePassword(password) {
            return /^(?=.*[0-9])(?=.*[!@#$%^&*])[A-Za-z\d!@#$%^&*]{8,}$/.test(password);
        }
    }));
});
