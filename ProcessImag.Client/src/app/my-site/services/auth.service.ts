import { Injectable, signal, computed } from '@angular/core';
export interface User {
  name: string;
  email: string;
  avatar?: string;
  isAuthenticated: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private userSignal = signal<User>({
    name: 'John Doe',
    email: 'john.doe@example.com',
    avatar: '',
    isAuthenticated: true
  });
  user = this.userSignal.asReadonly();
  initials = computed(() => {
    const name = this.userSignal().name;
    if (!name) return 'U';
    return name
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  });

  signIn(email: string, password: string) {
    console.log('Signing in:', email);
    this.userSignal.set({
      name: 'John Doe',
      email: email,
      avatar: '',
      isAuthenticated: true
    });
  }

  signOut() {
    this.userSignal.set({
      name: '',
      email: '',
      avatar: '',
      isAuthenticated: false
    });
  }

  updateProfile(updates: Partial<User>) {
    this.userSignal.update(current => ({
      ...current,
      ...updates
    }));
  }
  checkAuth(): boolean {
    return this.userSignal().isAuthenticated;
  }
}
