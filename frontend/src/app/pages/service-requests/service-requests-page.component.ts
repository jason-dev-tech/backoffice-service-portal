import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BehaviorSubject, catchError, map, of, startWith, switchMap } from 'rxjs';
import {
  CreateServiceRequestRequest,
  ServiceRequestService,
  UpdateServiceRequestRequest,
} from '../../services/service-request.service';
import { ServiceRequest } from '../../models/service-request.model';

type ServiceRequestViewState = {
  isLoading: boolean;
  errorMessage: string;
  serviceRequests: ServiceRequest[];
};

@Component({
  selector: 'app-service-requests-page',
  standalone: true,
  imports: [CommonModule],
  template: '',
})
export class ServiceRequestsPageComponent {
  private serviceRequestService = inject(ServiceRequestService);
  private refreshTrigger$ = new BehaviorSubject<void>(undefined);

  createForm = {
    title: '',
    description: '',
    requesterName: '',
    status: 'Open',
  };

  createErrorMessage = '';
  deleteErrorMessage = '';
  isSubmitting = false;
  deletingRequestId: number | null = null;
  editRequestId: number | null = null;

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

  get isEditMode(): boolean {
    return this.editRequestId !== null;
  }

  submitForm(): void {
    this.createErrorMessage = '';

    if (this.isEditMode) {
      this.submitEditForm();
      return;
    }

    this.submitCreateForm();
  }

  submitCreateForm(): void {
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
        this.resetForm();
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

  submitEditForm(): void {
    if (this.editRequestId === null) {
      return;
    }

    const payload: UpdateServiceRequestRequest = {
      title: this.createForm.title.trim(),
      description: this.createForm.description.trim(),
      requesterName: this.createForm.requesterName.trim(),
      status: this.createForm.status.trim(),
    };

    if (!payload.title || !payload.description || !payload.requesterName || !payload.status) {
      this.createErrorMessage = 'All fields are required.';
      return;
    }

    this.isSubmitting = true;

    this.serviceRequestService.updateServiceRequest(this.editRequestId, payload).subscribe({
      next: () => {
        this.resetForm();
        this.isSubmitting = false;
        this.refreshTrigger$.next();
      },
      error: (error) => {
        console.error('Failed to update service request', error);
        this.createErrorMessage = 'Failed to update service request.';
        this.isSubmitting = false;
      },
    });
  }

  startEdit(request: ServiceRequest): void {
    this.editRequestId = request.id;
    this.createErrorMessage = '';
    this.deleteErrorMessage = '';

    this.createForm = {
      title: request.title,
      description: request.description,
      requesterName: request.requesterName,
      status: request.status,
    };
  }

  cancelEdit(): void {
    this.resetForm();
  }

  deleteRequest(request: ServiceRequest): void {
    this.deleteErrorMessage = '';

    const confirmed = window.confirm(
      `Are you sure you want to delete service request "${request.title}"?`,
    );

    if (!confirmed) {
      return;
    }

    this.deletingRequestId = request.id;

    this.serviceRequestService.deleteServiceRequest(request.id).subscribe({
      next: () => {
        if (this.editRequestId === request.id) {
          this.resetForm();
        }

        this.deletingRequestId = null;
        this.refreshTrigger$.next();
      },
      error: (error) => {
        console.error('Failed to delete service request', error);
        this.deleteErrorMessage = 'Failed to delete service request.';
        this.deletingRequestId = null;
      },
    });
  }

  resetForm(): void {
    this.editRequestId = null;
    this.createErrorMessage = '';
    this.createForm = {
      title: '',
      description: '',
      requesterName: '',
      status: 'Open',
    };
  }
}
