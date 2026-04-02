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
  private http = inject(HttpClient);
  private loginUrl = `${environment.apiBaseUrl}/api/Auth/login`;
  private accessTokenStorageKey = 'auth.accessToken';
  private expiresAtStorageKey = 'auth.expiresAtUtc';

  login(username: string, password: string): Observable<LoginResponse> {
    const payload: LoginRequest = {
      username,
      password,
    };

    return this.http.post<LoginResponse>(this.loginUrl, payload).pipe(
      tap((response) => {
        localStorage.setItem(this.accessTokenStorageKey, response.accessToken);
        localStorage.setItem(this.expiresAtStorageKey, response.expiresAtUtc);
      }),
    );
  }

  logout(): void {
    localStorage.removeItem(this.accessTokenStorageKey);
    localStorage.removeItem(this.expiresAtStorageKey);
  }

  getToken(): string | null {
    if (!this.isAuthenticated()) {
      return null;
    }

    return localStorage.getItem(this.accessTokenStorageKey);
  }

  getTokenExpiry(): Date | null {
    const expiresAtUtc = localStorage.getItem(this.expiresAtStorageKey);

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
    const token = localStorage.getItem(this.accessTokenStorageKey);
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
}
