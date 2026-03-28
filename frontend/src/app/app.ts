import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { catchError, map, of, startWith } from 'rxjs';
import { ServiceRequest } from './models/service-request.model';
import { ServiceRequestService } from './services/service-request.service';

type ServiceRequestViewState = {
  isLoading: boolean;
  errorMessage: string;
  serviceRequests: ServiceRequest[];
};

@Component({
  selector: 'app-root',
  imports: [CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  private serviceRequestService = inject(ServiceRequestService);

  viewState$ = this.serviceRequestService.getServiceRequests().pipe(
    map(
      (data): ServiceRequestViewState => ({
        isLoading: false,
        errorMessage: '',
        serviceRequests: data,
      }),
    ),
    startWith({
      isLoading: true,
      errorMessage: '',
      serviceRequests: [],
    }),
    catchError((error) => {
      console.error('Failed to load service requests', error);

      return of({
        isLoading: false,
        errorMessage: 'Failed to load service requests.',
        serviceRequests: [],
      });
    }),
  );
}
