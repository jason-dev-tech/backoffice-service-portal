import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BehaviorSubject, catchError, map, of, startWith, switchMap } from 'rxjs';
import {
  CreateServiceRequestRequest,
  ServiceRequestService,
} from './services/service-request.service';
import { ServiceRequest } from './models/service-request.model';

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
  private refreshTrigger$ = new BehaviorSubject<void>(undefined);

  createForm = {
    title: '',
    description: '',
    requesterName: '',
  };

  createErrorMessage = '';
  isSubmitting = false;

  viewState$ = this.refreshTrigger$.pipe(
    switchMap(() =>
      this.serviceRequestService.getServiceRequests().pipe(
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
      ),
    ),
  );

  submitCreateForm(): void {
    this.createErrorMessage = '';

    const payload: CreateServiceRequestRequest = {
      title: this.createForm.title.trim(),
      description: this.createForm.description.trim(),
      requesterName: this.createForm.requesterName.trim(),
    };

    if (!payload.title || !payload.description || !payload.requesterName) {
      this.createErrorMessage = 'All fields are required.';
      return;
    }

    this.isSubmitting = true;

    this.serviceRequestService.createServiceRequest(payload).subscribe({
      next: () => {
        this.createForm = {
          title: '',
          description: '',
          requesterName: '',
        };
        this.isSubmitting = false;
        this.refreshTrigger$.next();
      },
      error: (error) => {
        console.error('Failed to create service request', error);
        this.createErrorMessage = 'Failed to create service request.';
        this.isSubmitting = false;
      },
    });
  }
}
