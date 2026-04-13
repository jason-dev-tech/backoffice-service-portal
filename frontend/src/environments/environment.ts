const runtimeConfig = globalThis as typeof globalThis & {
  BACKOFFICE_API_BASE_URL?: string;
};

const runtimeApiBaseUrl =
  typeof runtimeConfig.BACKOFFICE_API_BASE_URL === 'string' &&
  runtimeConfig.BACKOFFICE_API_BASE_URL.trim()
    ? runtimeConfig.BACKOFFICE_API_BASE_URL.trim()
    : 'https://localhost:7179';

export const environment = {
  production: false,
  apiBaseUrl: runtimeApiBaseUrl,
};
