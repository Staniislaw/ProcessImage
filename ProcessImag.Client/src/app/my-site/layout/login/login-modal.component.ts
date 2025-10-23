import { Component, HostListener, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <!-- Backdrop -->
    <div class="fixed inset-0 bg-black/50 z-40"></div>

    <!-- Modal -->
    <div class="fixed inset-0 flex items-center justify-center z-50">
      <div id="modal" class="bg-white rounded shadow-lg p-6 w-full max-w-sm relative">
        <h2 class="text-xl font-semibold mb-4 text-center">Autentificare</h2>
        <form (ngSubmit)="login()" class="flex flex-col gap-3">
          <input [(ngModel)]="email" name="email" type="email" placeholder="Email" required
                 class="border p-2 rounded focus:outline-none focus:ring focus:ring-blue-300">
          <input [(ngModel)]="parola" name="parola" type="password" placeholder="Parolă" required
                 class="border p-2 rounded focus:outline-none focus:ring focus:ring-blue-300">
          <button type="submit"
                  class="bg-blue-600 text-white p-2 rounded hover:bg-blue-700 transition">
            Login
          </button>
          <p *ngIf="eroare" class="text-red-500 text-sm mt-1">{{ eroare }}</p>
        </form>
        <button (click)="close()" 
                class="absolute top-2 right-2 text-gray-500 hover:text-gray-700">&times;</button>
      </div>
    </div>
  `
})
export class LoginModalComponent {
  authService = inject(AuthService);

  email = '';
  parola = '';
  eroare = '';

  // ✅ Decorăm ca Input pentru a primi callback
  @Input() onClose: (() => void) | null = null;

  login() {
    this.authService.login({ email: this.email, parola: this.parola }).subscribe({
      next: () => {
        this.eroare = '';
        this.close();
      },
      error: err => {
        this.eroare = err.error?.message ?? 'Eroare la autentificare';
      }
    });
  }

  close() {
    if (this.onClose) this.onClose();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const modalEl = document.getElementById('modal');
    if (modalEl && !modalEl.contains(event.target as Node)) {
      this.close();
    }
  }
}
