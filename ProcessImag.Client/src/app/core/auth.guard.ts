// guards/auth.guard.ts
import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { map, filter, take } from 'rxjs/operators';
import { AuthService } from '../my-site/services/auth.service';
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // ✅ Dacă suntem deja pe login sau register, nu redirecționăm
  const publicPaths = ['/login', '/register'];
  if (publicPaths.includes(state.url)) {
    return true;
  }

  if (!authService.loading()) {
    if (authService.isAuthenticated()) {
      return true;
    } else {
      router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
      return false;
    }
  }

  return new Promise<boolean>((resolve) => {
    const checkInterval = setInterval(() => {
      if (!authService.loading()) {
        clearInterval(checkInterval);
        if (authService.isAuthenticated()) {
          resolve(true);
        } else {
          // ❌ Nu redirecționa dacă suntem deja pe login/register
          if (!publicPaths.includes(state.url)) {
            router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
          }
          resolve(false);
        }
      }
    }, 50);
  });
};
