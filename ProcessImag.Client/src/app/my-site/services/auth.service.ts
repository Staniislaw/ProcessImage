import { Injectable, signal, computed } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';

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
  private apiUrl = 'https://localhost:7048/api/Utilizator'; 

  private userSignal = signal<User | null>(null);
  user = this.userSignal.asReadonly();

  tokenKey = 'jwt_token';

  constructor(private http: HttpClient) {
    const token = localStorage.getItem(this.tokenKey);
    if (token) {
      this.loadProfile().subscribe({
        next: () => {},
        error: () => this.signOut()
      });
    }
  }

  login(request: { email: string; parola: string }) {
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
    const headers = this.getAuthHeaders();
    return this.http.get(`${this.apiUrl}/profil`, { headers }).pipe(
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
    const headers = this.getAuthHeaders();
    return this.http.put(`${this.apiUrl}/update-profil`, request, { headers }).pipe(
      tap(() => this.userSignal.update(u => ({ ...u!, nume: request.nume ?? u!.name })))
    );
  }

  changePassword(request: ChangePasswordRequest): Observable<any> {
    const headers = this.getAuthHeaders();
    return this.http.post(`${this.apiUrl}/change-password`, request, { headers });
  }

  signOut() {
    localStorage.removeItem(this.tokenKey);
    this.userSignal.set(null);
  }

  isAuthenticated(): boolean {
    return !!this.userSignal() && this.userSignal()?.isAuthenticated === true;
  }

  getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem(this.tokenKey);
    return new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
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
