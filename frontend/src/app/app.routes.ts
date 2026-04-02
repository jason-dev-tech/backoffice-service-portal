import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { ServiceRequestsPageComponent } from './pages/service-requests/service-requests-page.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: '', component: ServiceRequestsPageComponent },
];
