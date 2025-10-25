import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { SidebarService } from '../../services/sidebar.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './header.component.html'
})
export class AppHeaderComponent {
  authService = inject(AuthService);
  private sidebarService = inject(SidebarService);
  private router = inject(Router);

  isDropdownOpen = false;

  toggleSidebar() {
    this.sidebarService.toggle();
  }

  toggleDropdown() {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  goToLoginPage() {
    this.router.navigate(['/login']);
  }

  signOut() {
    this.authService.signOut();
    this.isDropdownOpen = false;
  }
}
