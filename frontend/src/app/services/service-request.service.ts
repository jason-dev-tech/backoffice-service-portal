import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
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

@Injectable({
  providedIn: 'root',
})
export class ServiceRequestService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/api/ServiceRequests`;

  getServiceRequests(): Observable<ServiceRequest[]> {
    return this.http.get<ServiceRequest[]>(this.apiUrl);
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
