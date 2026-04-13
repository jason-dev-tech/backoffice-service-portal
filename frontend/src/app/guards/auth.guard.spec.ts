import { TestBed } from '@angular/core/testing';
import { provideRouter, Router, UrlTree } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { authGuard } from './auth.guard';

describe('authGuard', () => {
  async function runGuard(isAuthenticated: boolean) {
    const authServiceStub = {
      isAuthenticated: () => isAuthenticated,
    };

    await TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authServiceStub },
      ],
    });

    return TestBed.runInInjectionContext(() => authGuard({} as never, {} as never));
  }

  it('redirects unauthenticated access to /login', async () => {
    const result = await runGuard(false);
    const router = TestBed.inject(Router);

    expect(result instanceof UrlTree).toBe(true);
    expect(router.serializeUrl(result as UrlTree)).toBe('/login');
  });

  it('allows authenticated access', async () => {
    const result = await runGuard(true);

    expect(result).toBe(true);
  });
});
