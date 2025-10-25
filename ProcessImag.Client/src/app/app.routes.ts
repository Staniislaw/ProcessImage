import { Routes } from '@angular/router';
import { DashboardComponent } from './my-site/dashboard/dashboard.component';
import { HomeComponent } from './my-site/layout/home/home.component';
import { AppLayoutComponent } from './my-site/layout/layout/layout.component';
import { ProcessImageComponent } from './my-site/process-image/process-image.component';
import { LoginPageComponent } from './my-site/layout/login/login-modal.component';
import { RegisterPageComponent } from './my-site/layout/register-page/register-page.component';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginPageComponent },
  { path: 'register', component: RegisterPageComponent },
  {
    path: '',
    component: AppLayoutComponent,
    canActivateChild: [authGuard],
    children: [
      { path: '', component: HomeComponent },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'processImage', component: ProcessImageComponent },
      { path: '**', redirectTo: '' },
    ]
  },
];
