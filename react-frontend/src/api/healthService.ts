import { apiClient } from './apiClient';

export type HealthStatusResponse = {
  status: string;
};

export const healthService = {
  getHealthStatus() {
    return apiClient.get<HealthStatusResponse>('/status');
  },

  createPlaceholder(payload: { name: string }) {
    return apiClient.post<HealthStatusResponse>('/placeholder', payload);
  },
};
