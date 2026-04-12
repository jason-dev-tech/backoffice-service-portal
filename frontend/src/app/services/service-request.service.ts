import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ServiceRequestDashboard } from '../models/service-request-dashboard.model';
import { ServiceRequest } from '../models/service-request.model';

export type CreateServiceRequestRequest = {
  title: string;
  description: string;
  requesterName: string;
};

export type UpdateServiceRequestRequest = {
  title: string;
  description: string;
  requesterName: string;
  status: string;
};

export type PagedServiceRequestsResponse = {
  items: ServiceRequest[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

@Injectable({
  providedIn: 'root',
})
export class ServiceRequestService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/api/ServiceRequests`;

  getDashboard(): Observable<ServiceRequestDashboard> {
    return this.http.get<ServiceRequestDashboard>(`${this.apiUrl}/dashboard`);
  }

  getServiceRequests(params?: {
    status?: string;
    search?: string;
    page?: number;
    pageSize?: number;
    sort?: string;
  }): Observable<PagedServiceRequestsResponse> {
    const queryParams = new URLSearchParams();

    if (params?.status?.trim()) {
      queryParams.set('status', params.status.trim());
    }

    if (params?.search?.trim()) {
      queryParams.set('search', params.search.trim());
    }

    if (params?.sort?.trim()) {
      queryParams.set('sort', params.sort.trim());
    }

    queryParams.set('page', String(params?.page ?? 1));
    queryParams.set('pageSize', String(params?.pageSize ?? 10));

    return this.http.get<PagedServiceRequestsResponse>(`${this.apiUrl}?${queryParams.toString()}`);
  }

  createServiceRequest(payload: CreateServiceRequestRequest): Observable<ServiceRequest> {
    return this.http.post<ServiceRequest>(this.apiUrl, payload);
  }

  updateServiceRequest(
    id: number,
    payload: UpdateServiceRequestRequest,
  ): Observable<ServiceRequest> {
    return this.http.put<ServiceRequest>(`${this.apiUrl}/${id}`, payload);
  }

  deleteServiceRequest(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
