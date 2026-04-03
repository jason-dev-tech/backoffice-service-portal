import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { BehaviorSubject, catchError, combineLatest, map, of, startWith, switchMap } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import {
  CreateServiceRequestRequest,
  ServiceRequestService,
  UpdateServiceRequestRequest,
} from '../../services/service-request.service';
import { ServiceRequest } from '../../models/service-request.model';

type ServiceRequestSortOption =
  | 'created-desc'
  | 'created-asc'
  | 'title-asc'
  | 'title-desc'
  | 'status-asc'
  | 'status-desc';

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
})
export class ServiceRequestsPageComponent {
  private authService = inject(AuthService);
  private router = inject(Router);
  private serviceRequestService = inject(ServiceRequestService);
  private refreshTrigger$ = new BehaviorSubject<void>(undefined);
  private currentPage$ = new BehaviorSubject<number>(1);
  private searchTerm$ = new BehaviorSubject<string>('');
  private selectedStatus$ = new BehaviorSubject<string>('');
  private sortOption$ = new BehaviorSubject<ServiceRequestSortOption>('created-desc');

  createForm = {
    title: '',
    description: '',
    requesterName: '',
    status: 'Open',
  };

  successMessage = '';
  createErrorMessage = '';
  deleteErrorMessage = '';
  pageSize = 5;
  isSubmitting = false;
  deletingRequestId: number | null = null;
  editRequestId: number | null = null;

  get currentPage(): number {
    return this.currentPage$.value;
  }

  set currentPage(value: number) {
    this.currentPage$.next(value);
  }

  get searchTerm(): string {
    return this.searchTerm$.value;
  }

  set searchTerm(value: string) {
    this.currentPage = 1;
    this.searchTerm$.next(value);
  }

  get selectedStatus(): string {
    return this.selectedStatus$.value;
  }

  set selectedStatus(value: string) {
    this.currentPage = 1;
    this.selectedStatus$.next(value);
  }

  get sortOption(): ServiceRequestSortOption {
    return this.sortOption$.value;
  }

  set sortOption(value: ServiceRequestSortOption) {
    this.currentPage = 1;
    this.sortOption$.next(value);
  }

  viewState$ = combineLatest([
    this.refreshTrigger$.pipe(
      switchMap(() =>
        this.serviceRequestService.getServiceRequests().pipe(
          map(
            (data): ServiceRequestViewState => ({
              isLoading: false,
              errorMessage: '',
              serviceRequests: data,
              currentPage: this.currentPage,
              pageSize: this.pageSize,
              totalPages: 0,
              totalFilteredCount: 0,
              visibleCount: 0,
            }),
          ),
          startWith({
            isLoading: true,
            errorMessage: '',
            serviceRequests: [],
            currentPage: this.currentPage,
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
              currentPage: this.currentPage,
              pageSize: this.pageSize,
              totalPages: 0,
              totalFilteredCount: 0,
              visibleCount: 0,
            });
          }),
        ),
      ),
    ),
    this.currentPage$,
    this.searchTerm$,
    this.selectedStatus$,
    this.sortOption$,
  ]).pipe(
    map(([viewState, currentPage, searchTerm, selectedStatus, sortOption]) => {
      const filteredServiceRequests = this.filterServiceRequests(
        viewState.serviceRequests,
        searchTerm,
        selectedStatus,
      );
      const sortedServiceRequests = this.sortServiceRequests(filteredServiceRequests, sortOption);

      return {
        ...viewState,
        ...this.paginateServiceRequests(sortedServiceRequests, currentPage),
      };
    }),
  );

  get isEditMode(): boolean {
    return this.editRequestId !== null;
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
        this.resetForm();
        this.successMessage = 'Service request created successfully.';
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

    this.successMessage = '';
    this.isSubmitting = true;

    this.serviceRequestService.updateServiceRequest(this.editRequestId, payload).subscribe({
      next: () => {
        this.resetForm();
        this.successMessage = 'Service request updated successfully.';
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
    this.successMessage = '';
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
          this.resetForm();
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
    this.editRequestId = null;
    this.createErrorMessage = '';
    this.createForm = {
      title: '',
      description: '',
      requesterName: '',
      status: 'Open',
    };
  }

  onLogout(): void {
    this.authService.logout();
    void this.router.navigate(['/login']);
  }

  resetFilters(): void {
    this.currentPage = 1;
    this.searchTerm = '';
    this.selectedStatus = '';
    this.sortOption = 'created-desc';
  }

  private filterServiceRequests(
    serviceRequests: ServiceRequest[],
    searchTerm: string,
    selectedStatus: string,
  ): ServiceRequest[] {
    const normalizedSearchTerm = searchTerm.trim().toLowerCase();
    const normalizedSelectedStatus = selectedStatus.trim().toLowerCase();

    return serviceRequests.filter((request) =>
      (!normalizedSearchTerm ||
        [request.title, request.description, request.status].some((value) =>
          value.toLowerCase().includes(normalizedSearchTerm),
        )) &&
      (!normalizedSelectedStatus || request.status.toLowerCase() === normalizedSelectedStatus),
    );
  }

  private paginateServiceRequests(
    serviceRequests: ServiceRequest[],
    currentPage: number,
  ): Pick<
    ServiceRequestViewState,
    'serviceRequests' | 'currentPage' | 'pageSize' | 'totalPages' | 'totalFilteredCount' | 'visibleCount'
  > {
    const totalPages =
      serviceRequests.length === 0 ? 0 : Math.ceil(serviceRequests.length / this.pageSize);
    const normalizedCurrentPage = totalPages === 0 ? 1 : Math.min(currentPage, totalPages);
    const startIndex = (normalizedCurrentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    const visibleServiceRequests = serviceRequests.slice(startIndex, endIndex);

    return {
      serviceRequests: visibleServiceRequests,
      currentPage: normalizedCurrentPage,
      pageSize: this.pageSize,
      totalPages,
      totalFilteredCount: serviceRequests.length,
      visibleCount: visibleServiceRequests.length,
    };
  }

  private sortServiceRequests(
    serviceRequests: ServiceRequest[],
    sortOption: ServiceRequestSortOption,
  ): ServiceRequest[] {
    const sortedServiceRequests = [...serviceRequests];

    sortedServiceRequests.sort((left, right) => {
      switch (sortOption) {
        case 'created-asc':
          return new Date(left.createdAt).getTime() - new Date(right.createdAt).getTime();
        case 'created-desc':
          return new Date(right.createdAt).getTime() - new Date(left.createdAt).getTime();
        case 'title-asc':
          return left.title.localeCompare(right.title);
        case 'title-desc':
          return right.title.localeCompare(left.title);
        case 'status-asc':
          return left.status.localeCompare(right.status);
        case 'status-desc':
          return right.status.localeCompare(left.status);
      }
    });

    return sortedServiceRequests;
  }
}
