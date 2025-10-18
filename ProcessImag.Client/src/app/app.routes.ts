import { Routes } from '@angular/router';
import { DashboardComponent } from './my-site/dashboard/dashboard.component';
import { HomeComponent } from './my-site/layout/home/home.component';
import { AppLayoutComponent } from './my-site/layout/layout/layout.component';
import { ProcessImageComponent } from './my-site/process-image/process-image.component';

export const routes: Routes = [
  {
    path: '',
    component: AppLayoutComponent,
    children: [
      { path: '', component: HomeComponent },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'processImage', component: ProcessImageComponent },
      { path: '**', redirectTo: '' }
    ]
  }
];