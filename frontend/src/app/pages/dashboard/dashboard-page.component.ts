import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, map, of, startWith } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { ServiceRequestService } from '../../services/service-request.service';

type DashboardViewState = {
  isLoading: boolean;
  errorMessage: string;
  summary: {
    totalRequests: number;
    openRequests: number;
    inProgressRequests: number;
    closedRequests: number;
  };
};

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard-page.component.html',
  styleUrls: ['./dashboard-page.component.css'],
})
export class DashboardPageComponent {
  private authService = inject(AuthService);
  private router = inject(Router);
  private serviceRequestService = inject(ServiceRequestService);

  viewState$ = this.serviceRequestService.getDashboard().pipe(
    map(
      (summary): DashboardViewState => ({
        isLoading: false,
        errorMessage: '',
        summary,
      }),
    ),
    startWith({
      isLoading: true,
      errorMessage: '',
      summary: {
        totalRequests: 0,
        openRequests: 0,
        inProgressRequests: 0,
        closedRequests: 0,
      },
    }),
    catchError((error) => {
      console.error('Failed to load dashboard summary', error);

      return of({
        isLoading: false,
        errorMessage: 'Failed to load dashboard summary.',
        summary: {
          totalRequests: 0,
          openRequests: 0,
          inProgressRequests: 0,
          closedRequests: 0,
        },
      });
    }),
  );

  goToServiceRequests(): void {
    void this.router.navigate(['/service-requests']);
  }

  onLogout(): void {
    this.authService.logout();
    void this.router.navigate(['/login']);
  }
}
