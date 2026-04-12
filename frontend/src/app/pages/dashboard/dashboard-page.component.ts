import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, map, of, startWith } from 'rxjs';
import { ServiceRequestDashboard } from '../../models/service-request-dashboard.model';
import { ServiceRequestService } from '../../services/service-request.service';

type DashboardViewState = {
  isLoading: boolean;
  errorMessage: string;
  dashboard: ServiceRequestDashboard;
};

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard-page.component.html',
  styleUrls: ['./dashboard-page.component.css'],
})
export class DashboardPageComponent {
  private router = inject(Router);
  private serviceRequestService = inject(ServiceRequestService);

  viewState$ = this.serviceRequestService.getDashboard().pipe(
    map(
      (dashboard): DashboardViewState => ({
        isLoading: false,
        errorMessage: '',
        dashboard,
      }),
    ),
    startWith({
      isLoading: true,
      errorMessage: '',
      dashboard: {
        totalRequests: 0,
        openRequests: 0,
        inProgressRequests: 0,
        closedRequests: 0,
        oldestOpenRequestCreatedAt: null,
        mostRecentRequestCreatedAt: null,
        openSharePercentage: 0,
        closedSharePercentage: 0,
        statusDistribution: [],
      },
    }),
    catchError((error) => {
      console.error('Failed to load dashboard summary', error);

      return of({
        isLoading: false,
        errorMessage: 'Failed to load dashboard summary.',
        dashboard: {
          totalRequests: 0,
          openRequests: 0,
          inProgressRequests: 0,
          closedRequests: 0,
          oldestOpenRequestCreatedAt: null,
          mostRecentRequestCreatedAt: null,
          openSharePercentage: 0,
          closedSharePercentage: 0,
          statusDistribution: [],
        },
      });
    }),
  );

  drillDownToStatus(status: string): void {
    void this.router.navigate(['/service-requests'], {
      queryParams: { status },
    });
  }
}
