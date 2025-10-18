import { CanActivateFn, Router } from '@angular/router';

export function authGuard() {
  if (typeof window === 'undefined' || !window.localStorage) {
    return false; // sau true, depinde ce vrei în server-side
  }

  const token = localStorage.getItem('token');
  return !!token;
}
