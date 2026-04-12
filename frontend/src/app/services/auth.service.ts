import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';

type LoginRequest = {
  username: string;
  password: string;
};

type LoginResponse = {
  accessToken: string;
  expiresAtUtc: string;
};

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private static readonly TOKEN_STORAGE_KEY = 'auth.accessToken';
  private static readonly EXPIRES_AT_STORAGE_KEY = 'auth.expiresAtUtc';
  private static readonly ROLE_CLAIM_KEYS = [
    'role',
    'roles',
    'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
  ] as const;

  private http = inject(HttpClient);
  private loginUrl = `${environment.apiBaseUrl}/api/Auth/login`;

  login(username: string, password: string): Observable<LoginResponse> {
    const payload: LoginRequest = {
      username,
      password,
    };

    return this.http.post<LoginResponse>(this.loginUrl, payload).pipe(
      tap((response) => {
        this.setToken(response.accessToken);
        localStorage.setItem(AuthService.EXPIRES_AT_STORAGE_KEY, response.expiresAtUtc);
      }),
    );
  }

  setToken(token: string): void {
    localStorage.setItem(AuthService.TOKEN_STORAGE_KEY, token);
  }

  getToken(): string | null {
    if (!this.isAuthenticated()) {
      return null;
    }

    return localStorage.getItem(AuthService.TOKEN_STORAGE_KEY);
  }

  removeToken(): void {
    localStorage.removeItem(AuthService.TOKEN_STORAGE_KEY);
  }

  getTokenExpiry(): Date | null {
    const expiresAtUtc = localStorage.getItem(AuthService.EXPIRES_AT_STORAGE_KEY);

    if (!expiresAtUtc) {
      return null;
    }

    const expiry = new Date(expiresAtUtc);

    if (Number.isNaN(expiry.getTime())) {
      return null;
    }

    return expiry;
  }

  isAuthenticated(): boolean {
    const token = localStorage.getItem(AuthService.TOKEN_STORAGE_KEY);
    const expiry = this.getTokenExpiry();

    if (!token || !expiry) {
      return false;
    }

    if (expiry <= new Date()) {
      this.logout();
      return false;
    }

    return true;
  }

  logout(): void {
    this.removeToken();
    localStorage.removeItem(AuthService.EXPIRES_AT_STORAGE_KEY);
  }

  getCurrentUserRole(): string | null {
    const token = this.getToken();

    if (!token) {
      return null;
    }

    const payload = this.getTokenPayload(token);

    if (!payload || typeof payload !== 'object') {
      return null;
    }

    for (const claimKey of AuthService.ROLE_CLAIM_KEYS) {
      const claimValue = payload[claimKey];

      if (typeof claimValue === 'string' && claimValue.trim()) {
        return claimValue.trim();
      }

      if (Array.isArray(claimValue)) {
        const firstRole = claimValue.find(
          (value): value is string => typeof value === 'string' && value.trim().length > 0,
        );

        if (firstRole) {
          return firstRole.trim();
        }
      }
    }

    return null;
  }

  isAdmin(): boolean {
    return this.getCurrentUserRole() === 'Admin';
  }

  canCreateServiceRequest(): boolean {
    const role = this.getCurrentUserRole();
    return role === 'Admin' || role === 'Operator';
  }

  canEditServiceRequest(): boolean {
    const role = this.getCurrentUserRole();
    return role === 'Admin' || role === 'Operator';
  }

  canDeleteServiceRequest(): boolean {
    return this.getCurrentUserRole() === 'Admin';
  }

  private getTokenPayload(token: string): Record<string, unknown> | null {
    const parts = token.split('.');

    if (parts.length < 2) {
      return null;
    }

    try {
      const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
      const paddedBase64 = base64.padEnd(Math.ceil(base64.length / 4) * 4, '=');
      const jsonPayload = atob(paddedBase64);

      return JSON.parse(jsonPayload) as Record<string, unknown>;
    } catch {
      return null;
    }
  }
}
