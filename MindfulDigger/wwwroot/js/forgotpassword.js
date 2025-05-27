document.addEventListener('alpine:init', () => {
    Alpine.data('forgotPasswordForm', () => ({
        email: '',
        emailError: '',
        message: '',
        error: '',
        async sendReset() {
            this.emailError = '';
            this.message = '';
            this.error = '';
            if (!this.validateEmail(this.email)) {
                this.emailError = 'Nieprawidłowy format e-mail.';
                return;
            }
            try {
                const res = await fetch('/api/auth/forgot-password', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ email: this.email })
                });
                const data = await res.json();
                if (!res.ok) {
                    this.error = data.message || 'Błąd wysyłki e-maila.';
                } else {
                    this.message = 'Wysłano e-mail z linkiem do resetu hasła.';
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
