import { ApplicationConfig } from '@angular/core';
import { provideRouter, withDebugTracing } from '@angular/router';
import { provideClientHydration } from '@angular/platform-browser';
import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './core/authInterceptor';
export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes, withDebugTracing()), // router cu debug tracing
    provideClientHydration(),
    provideHttpClient(
      withInterceptors([authInterceptor])
    ),

  ]
};
