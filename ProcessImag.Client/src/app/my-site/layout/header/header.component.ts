import { Component, inject, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SidebarService } from '../../services/sidebar.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class AppHeaderComponent {
  // Inject services
  authService = inject(AuthService);
  private sidebarService = inject(SidebarService);

  isDropdownOpen = false;

  toggleSidebar() {
    this.sidebarService.toggle();
  }

  toggleDropdown() {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  signIn() {
    this.authService.signIn('user@example.com', 'password');
  }

  signOut() {
    this.authService.signOut();
    this.isDropdownOpen = false;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    const dropdown = target.closest('.relative');
    if (!dropdown && this.isDropdownOpen) {
      this.isDropdownOpen = false;
    }
  }
}
