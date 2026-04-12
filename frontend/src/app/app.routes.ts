import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { DashboardPageComponent } from './pages/dashboard/dashboard-page.component';
import { LoginComponent } from './pages/login/login.component';
import { ServiceRequestsPageComponent } from './pages/service-requests/service-requests-page.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardPageComponent, canActivate: [authGuard] },
  { path: 'service-requests', component: ServiceRequestsPageComponent, canActivate: [authGuard] },
  { path: '', redirectTo: 'service-requests', pathMatch: 'full' },
];
