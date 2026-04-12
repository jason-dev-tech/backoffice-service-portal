import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { AuthenticatedShellComponent } from './layouts/authenticated-shell/authenticated-shell.component';
import { DashboardPageComponent } from './pages/dashboard/dashboard-page.component';
import { LoginComponent } from './pages/login/login.component';
import { ServiceRequestsPageComponent } from './pages/service-requests/service-requests-page.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: AuthenticatedShellComponent,
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', component: DashboardPageComponent },
      { path: 'service-requests', component: ServiceRequestsPageComponent },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    ],
  },
  { path: '**', redirectTo: '' },
];
