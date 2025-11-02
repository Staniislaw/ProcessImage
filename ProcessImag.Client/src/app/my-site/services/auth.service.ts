import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface User {
  id: number;
  name: string;
  email: string;
  isAuthenticated: boolean;
}

export interface LoginRequest {
  email: string;
  parola: string;
}

export interface RegisterRequest {
  nume: string;
  email: string;
  parola: string;
  subscriptieId: number;
}

export interface UpdateProfilRequest {
  nume: string;
}

export interface ChangePasswordRequest {
  parolaVeche: string;
  parolaNoua: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiBaseUrl + '/Utilizator';
  tokenKey = 'jwt_token';
  private userSignal = signal<User | null>(null);
  private loadingSignal = signal<boolean>(true); 
  user = this.userSignal.asReadonly();
  loading = this.loadingSignal.asReadonly(); 
  constructor(private http: HttpClient) {
    this.initializeAuth();
  }
  private initializeAuth(): void {
  const token = localStorage.getItem(this.tokenKey);
  if (token) {
    this.loadProfile().subscribe({
      next: (user) => {
        this.loadingSignal.set(false);
      },
      error: (err) => {
        this.signOut();
        this.loadingSignal.set(false);
      }
    });
  } else {
    this.loadingSignal.set(false);
  }
}


  login(request: { email: string; parola: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, request).pipe(
      tap((response: any) => {
        localStorage.setItem(this.tokenKey, response.token);
        this.userSignal.set({
          id: response.utilizator.id,
          name: response.utilizator.nume,
          email: response.utilizator.email,
          isAuthenticated: true
        });
      })
    );
  }

  register(request: RegisterRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, request);
  }

  loadProfile(): Observable<any> {
    return this.http.get(`${this.apiUrl}/profil`).pipe(
      tap((user: any) => {
        this.userSignal.set({
          id: user.id,
          name: user.nume,
          email: user.email,
          isAuthenticated: true
        });
      })
    );
  }

  updateProfile(request: UpdateProfilRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/update-profil`, request).pipe(
      tap(() => {
        this.userSignal.update(u => u ? { ...u, name: request.nume } : null);
      })
    );
  }

  changePassword(request: ChangePasswordRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/change-password`, request);
  }

  signOut(): void {
    localStorage.removeItem(this.tokenKey);
    this.userSignal.set(null);
  }

  isAuthenticated(): boolean {
    return !!this.userSignal() && this.userSignal()?.isAuthenticated === true;
  }

  initials = computed(() => {
    const user = this.userSignal();
    if (!user?.name) return 'U';
    return user.name
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  });
}