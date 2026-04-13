import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;

  beforeEach(() => {
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [provideHttpClient()],
    });

    service = TestBed.inject(AuthService);
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('returns false when no token is present', () => {
    expect(service.isAuthenticated()).toBe(false);
  });

  it('returns false when the token is expired', () => {
    service.setToken('test-token');
    localStorage.setItem('auth.expiresAtUtc', new Date(Date.now() - 60_000).toISOString());

    expect(service.isAuthenticated()).toBe(false);
  });

  it('returns true when a token is present and expiry is in the future', () => {
    service.setToken('test-token');
    localStorage.setItem('auth.expiresAtUtc', new Date(Date.now() + 60_000).toISOString());

    expect(service.isAuthenticated()).toBe(true);
  });
});
