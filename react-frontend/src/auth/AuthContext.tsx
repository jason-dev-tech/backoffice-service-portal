import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import {
  clearAccessToken,
  setAccessToken,
} from '../api/apiClient';
import {
  clearAuthState,
  readAuthState,
  writeAuthState,
} from './authStorage';
import type { AuthState, CurrentUser } from './authTypes';

type AuthContextValue = {
  authState: AuthState | null;
  isAuthenticated: boolean;
  currentUser: CurrentUser;
  setAuthState: (authState: AuthState) => void;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

type AuthProviderProps = {
  children: ReactNode;
};

export function AuthProvider({ children }: AuthProviderProps) {
  const [authState, setStoredAuthState] = useState<AuthState | null>(() => {
    const storedAuthState = readAuthState();

    if (storedAuthState) {
      setAccessToken(storedAuthState.accessToken);
    }

    return storedAuthState;
  });

  const setAuthState = useCallback((nextAuthState: AuthState) => {
    writeAuthState(nextAuthState);
    setAccessToken(nextAuthState.accessToken);
    setStoredAuthState(nextAuthState);
  }, []);

  const logout = useCallback(() => {
    clearAuthState();
    clearAccessToken();
    setStoredAuthState(null);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      authState,
      isAuthenticated: authState !== null,
      currentUser: null,
      setAuthState,
      logout,
    }),
    [authState, logout, setAuthState],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }

  return context;
}
