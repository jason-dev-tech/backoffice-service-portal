export type AuthState = {
  accessToken: string;
  expiresAtUtc: string;
};

export type CurrentUser = {
  id: string;
  displayName: string;
  roles: string[];
} | null;
