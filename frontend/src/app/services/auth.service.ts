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
}
