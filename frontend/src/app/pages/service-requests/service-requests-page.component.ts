import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { BehaviorSubject, catchError, combineLatest, map, of, startWith, switchMap } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import {
  CreateServiceRequestRequest,
  PagedServiceRequestsResponse,
  ServiceRequestService,
  UpdateServiceRequestRequest,
} from '../../services/service-request.service';
import { ServiceRequest } from '../../models/service-request.model';

type ServiceRequestSortOption =
  | 'createdAt_desc'
  | 'createdAt_asc'
  | 'title_asc'
  | 'title_desc';

type ServiceRequestViewState = {
  isLoading: boolean;
  errorMessage: string;
  serviceRequests: ServiceRequest[];
  currentPage: number;
  pageSize: number;
  totalPages: number;
  totalFilteredCount: number;
  visibleCount: number;
};

@Component({
  selector: 'app-service-requests-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './service-requests-page.component.html',
  styleUrls: ['./service-requests-page.component.css'],
})
export class ServiceRequestsPageComponent {
  private authService = inject(AuthService);
  private route = inject(ActivatedRoute);
  private serviceRequestService = inject(ServiceRequestService);
  private refreshTrigger$ = new BehaviorSubject<void>(undefined);
  private currentPage$ = new BehaviorSubject<number>(1);
  private searchTerm$ = new BehaviorSubject<string>('');
  private selectedStatus$ = new BehaviorSubject<string>(this.getInitialStatusFilter());
  private sortOption$ = new BehaviorSubject<ServiceRequestSortOption>('createdAt_desc');

  createForm = {
    title: '',
    description: '',
    requesterName: '',
    status: 'Open',
  };

  successMessage = '';
  createErrorMessage = '';
  deleteErrorMessage = '';
  pageSize = 10;
  isSubmitting = false;
  isDrawerOpen = false;
  deletingRequestId: number | null = null;
  editRequestId: number | null = null;

  get searchTerm(): string {
    return this.searchTerm$.value;
  }

  get canCreateServiceRequest(): boolean {
    return this.authService.canCreateServiceRequest();
  }

  get canEditServiceRequest(): boolean {
    return this.authService.canEditServiceRequest();
  }

  get canDeleteServiceRequest(): boolean {
    return this.authService.canDeleteServiceRequest();
  }

  set searchTerm(value: string) {
    this.currentPage$.next(1);
    this.searchTerm$.next(value);
  }

  get selectedStatus(): string {
    return this.selectedStatus$.value;
  }

  set selectedStatus(value: string) {
    this.currentPage$.next(1);
    this.selectedStatus$.next(value);
  }

  get sortOption(): ServiceRequestSortOption {
    return this.sortOption$.value;
  }

  set sortOption(value: ServiceRequestSortOption) {
    this.currentPage$.next(1);
    this.sortOption$.next(value);
  }

  viewState$ = combineLatest([
    this.refreshTrigger$,
    this.currentPage$,
    this.selectedStatus$,
    this.searchTerm$,
    this.sortOption$,
  ]).pipe(
    switchMap(([, currentPage, selectedStatus, searchTerm, sortOption]) =>
      this.serviceRequestService
        .getServiceRequests({
          status: selectedStatus,
          search: searchTerm,
          page: currentPage,
          pageSize: this.pageSize,
          sort: sortOption,
        })
        .pipe(
          map(
            (response: PagedServiceRequestsResponse): ServiceRequestViewState => ({
              isLoading: false,
              errorMessage: '',
              serviceRequests: response.items,
              currentPage: response.page,
              pageSize: response.pageSize,
              totalPages: response.totalPages,
              totalFilteredCount: response.totalCount,
              visibleCount: response.items.length,
            }),
          ),
          startWith({
            isLoading: true,
            errorMessage: '',
            serviceRequests: [],
            currentPage,
            pageSize: this.pageSize,
            totalPages: 0,
            totalFilteredCount: 0,
            visibleCount: 0,
          }),
          catchError((error) => {
            console.error('Failed to load service requests', error);

            return of({
              isLoading: false,
              errorMessage: 'Failed to load service requests.',
              serviceRequests: [],
              currentPage,
              pageSize: this.pageSize,
              totalPages: 0,
              totalFilteredCount: 0,
              visibleCount: 0,
            });
          }),
        ),
    ),
  );

  get isEditMode(): boolean {
    return this.editRequestId !== null;
  }

  openCreateDrawer(): void {
    this.successMessage = '';
    this.createErrorMessage = '';
    this.deleteErrorMessage = '';
    this.resetFormState();
    this.isDrawerOpen = true;
  }

  closeDrawer(): void {
    this.resetForm();
  }

  onDrawerBackdropClick(): void {
    if (this.isSubmitting) {
      return;
    }

    this.closeDrawer();
  }

  submitForm(): void {
    this.successMessage = '';
    this.createErrorMessage = '';
    this.deleteErrorMessage = '';

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

    this.successMessage = '';
    this.isSubmitting = true;

    this.serviceRequestService.createServiceRequest(payload).subscribe({
      next: () => {
        this.successMessage = 'Service request created successfully.';
        this.isSubmitting = false;
        this.refreshTrigger$.next();
        this.closeDrawer();
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

    this.successMessage = '';
    this.isSubmitting = true;

    this.serviceRequestService.updateServiceRequest(this.editRequestId, payload).subscribe({
      next: () => {
        this.successMessage = 'Service request updated successfully.';
        this.isSubmitting = false;
        this.refreshTrigger$.next();
        this.closeDrawer();
      },
      error: (error) => {
        console.error('Failed to update service request', error);
        this.createErrorMessage = 'Failed to update service request.';
        this.isSubmitting = false;
      },
    });
  }

  startEdit(request: ServiceRequest): void {
    this.successMessage = '';
    this.createErrorMessage = '';
    this.deleteErrorMessage = '';
    this.editRequestId = request.id;
    this.isDrawerOpen = true;

    this.createForm = {
      title: request.title,
      description: request.description,
      requesterName: request.requesterName,
      status: request.status,
    };
  }

  cancelEdit(): void {
    this.closeDrawer();
  }

  deleteRequest(request: ServiceRequest): void {
    this.successMessage = '';
    this.createErrorMessage = '';
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
          this.closeDrawer();
        }

        this.successMessage = 'Service request deleted successfully.';
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
    this.resetFormState();
    this.createErrorMessage = '';
    this.isDrawerOpen = false;
  }

  resetFilters(): void {
    this.currentPage$.next(1);
    this.searchTerm = '';
    this.selectedStatus = '';
    this.sortOption = 'createdAt_desc';
  }

  goToPreviousPage(): void {
    if (this.currentPage$.value <= 1) {
      return;
    }

    this.currentPage$.next(this.currentPage$.value - 1);
  }

  goToNextPage(totalPages: number): void {
    if (this.currentPage$.value >= totalPages) {
      return;
    }

    this.currentPage$.next(this.currentPage$.value + 1);
  }

  private resetFormState(): void {
    this.editRequestId = null;
    this.createForm = {
      title: '',
      description: '',
      requesterName: '',
      status: 'Open',
    };
  }

  private getInitialStatusFilter(): string {
    const status = this.route.snapshot.queryParamMap.get('status')?.trim();

    if (!status) {
      return '';
    }

    const allowedStatuses = new Set(['Open', 'In Progress', 'Closed']);

    return allowedStatuses.has(status) ? status : '';
  }
}
