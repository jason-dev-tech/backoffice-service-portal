import type { AuthState } from './authTypes';

const AUTH_STORAGE_KEY = 'backoffice.auth';

function isAuthState(value: unknown): value is AuthState {
  if (!value || typeof value !== 'object') {
    return false;
  }

  const authState = value as Partial<AuthState>;
  return (
    typeof authState.accessToken === 'string' &&
    typeof authState.expiresAtUtc === 'string'
  );
}

function isExpired(expiresAtUtc: string) {
  const expiresAt = Date.parse(expiresAtUtc);
  return Number.isNaN(expiresAt) || expiresAt <= Date.now();
}

export function readAuthState(): AuthState | null {
  const storedValue = localStorage.getItem(AUTH_STORAGE_KEY);

  if (!storedValue) {
    return null;
  }

  try {
    const parsedValue: unknown = JSON.parse(storedValue);

    if (
      !isAuthState(parsedValue) ||
      !parsedValue.accessToken ||
      !parsedValue.expiresAtUtc ||
      isExpired(parsedValue.expiresAtUtc)
    ) {
      clearAuthState();
      return null;
    }

    return parsedValue;
  } catch {
    clearAuthState();
    return null;
  }
}

export function writeAuthState(authState: AuthState) {
  localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(authState));
}

export function clearAuthState() {
  localStorage.removeItem(AUTH_STORAGE_KEY);
}
