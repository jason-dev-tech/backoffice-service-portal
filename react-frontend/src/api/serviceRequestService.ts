import { apiClient } from './apiClient';

export type ServiceRequestDto = {
  id: number;
  title: string;
  description: string;
  requesterName: string;
  status: string;
  createdAt: string;
  updatedAt: string | null;
};

export type PagedServiceRequestsResponse = {
  items: ServiceRequestDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type CreateServiceRequestRequest = {
  title: string;
  description: string;
  requesterName: string;
};

export const serviceRequestService = {
  getServiceRequests() {
    return apiClient.get<PagedServiceRequestsResponse>('/api/ServiceRequests');
  },

  createServiceRequest(request: CreateServiceRequestRequest) {
    return apiClient.post<ServiceRequestDto>('/api/ServiceRequests', request);
  },
};
