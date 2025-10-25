import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService, RegisterRequest } from '../../services/auth.service';

@Component({
  selector: 'app-register-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './register-page.component.html',
  styleUrls: ['./register-page.component.css']
})
export class RegisterPageComponent {
  private router = inject(Router);
  private authService = inject(AuthService);

  name = '';
  email = '';
  password = '';
  confirmPassword = '';
  subscriptieId = 1; // poți lega la un select în UI dacă vrei
  errorMessage = '';
  successMessage = '';

  register() {
    this.errorMessage = '';
    this.successMessage = '';

    // Validări locale
    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Parolele nu coincid.';
      return;
    }
    if (this.password.length < 6) {
      this.errorMessage = 'Parola trebuie să aibă cel puțin 6 caractere.';
      return;
    }

    // Payload pentru backend
    const payload: RegisterRequest = {
      nume: this.name,
      email: this.email,
      parola: this.password,
      subscriptieId: this.subscriptieId
    };

    // Apel serviciu
    this.authService.register(payload).subscribe({
      next: (res) => {
        this.successMessage = res.message;
        setTimeout(() => this.router.navigate(['/login']), 1500);
      },
      error: (err) => {
        this.errorMessage = err.error?.message ?? 'Eroare la înregistrare';
      }
    });
  }
}
