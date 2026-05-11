import { apiClient } from './apiClient';

export type LoginRequest = {
  username: string;
  password: string;
};

export type LoginResponse = {
  accessToken: string;
  expiresAtUtc: string;
};

export const authService = {
  login(request: LoginRequest) {
    return apiClient.post<LoginResponse>('/api/Auth/login', request);
  },
};
