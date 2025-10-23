import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SidebarService } from '../../services/sidebar.service';
import { AuthService } from '../../services/auth.service';
import { LoginModalComponent } from '../login/login-modal.component';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, LoginModalComponent],
  templateUrl: './header.component.html'
})
export class AppHeaderComponent {
  authService = inject(AuthService);
  private sidebarService = inject(SidebarService);

  isDropdownOpen = false;
  showLoginModal = false;

  toggleSidebar() {
    this.sidebarService.toggle();
  }

  toggleDropdown() {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  openLoginModal() {
    this.showLoginModal = true;
  }

  closeLoginModal() {
    this.showLoginModal = false;
  }

  signOut() {
    this.authService.signOut();
    this.isDropdownOpen = false;
  }
}
