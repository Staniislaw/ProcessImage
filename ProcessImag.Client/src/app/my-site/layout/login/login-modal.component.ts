import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-gray-100 px-4">
      <div class="bg-white shadow-2xl rounded-2xl p-8 w-full max-w-md border border-gray-100">
        <h2 class="text-3xl font-bold text-center text-gray-800 mb-6">Bine ai revenit 👋</h2>

        <form (ngSubmit)="login()" class="flex flex-col gap-4">
          <input 
            [(ngModel)]="email" name="email" type="email" placeholder="Email" required
            class="border border-gray-300 p-3 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition" />

          <input 
            [(ngModel)]="parola" name="parola" type="password" placeholder="Parolă" required
            class="border border-gray-300 p-3 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition" />

          <button 
            type="submit"
            class="bg-blue-600 text-white py-2.5 rounded-lg font-semibold hover:bg-blue-700 transition-colors">
            Autentificare
          </button>

          <p *ngIf="eroare" class="text-red-500 text-center text-sm mt-2">{{ eroare }}</p>
        </form>

        <p class="text-center text-sm text-gray-600 mt-6">
          Nu ai cont?
          <button (click)="goToRegisterPage()"
            class="inline-flex items-center justify-center rounded-md text-sm font-medium bg-primary text-primary-foreground hover:bg-primary/90 h-9 px-4 py-2">
            Înregistrează-te
          </button>
        </p>
      </div>
    </div>
  `
})
export class LoginPageComponent {
  private authService = inject(AuthService);
  private router = inject(Router);
  
  email = '';
  parola = '';
  eroare = '';

  login() {
    this.authService.login({ email: this.email, parola: this.parola }).subscribe({
      next: () => {
        this.eroare = '';
        this.router.navigate(['/']); // după autentificare mergem la home
      },
      error: err => {
        this.eroare = err.error?.message ?? 'Eroare la autentificare';
      }
    });
  }
  goToRegisterPage() {
    this.router.navigate(['/register']);
  }
}
