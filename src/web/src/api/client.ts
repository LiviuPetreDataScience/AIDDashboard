import axios from 'axios';

/**
 * Shared Axios instance. All API calls go through here so the base URL, the bearer-token
 * header and global error handling are configured in one place.
 *
 * The base URL is the same origin "/api": in development Vite proxies it to the backend,
 * and in production the backend serves both the SPA and the API.
 */
export const apiClient = axios.create({
  baseURL: '/api',
});

const TOKEN_STORAGE_KEY = 'aid.authToken';

/** Stores (or clears) the bearer token used for subsequent requests. */
export function setAuthToken(token: string | null): void {
  if (token) {
    localStorage.setItem(TOKEN_STORAGE_KEY, token);
  } else {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
  }
}

export function getAuthToken(): string | null {
  return localStorage.getItem(TOKEN_STORAGE_KEY);
}

// Attach the bearer token to every outgoing request when present.
apiClient.interceptors.request.use((config) => {
  const token = getAuthToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

/**
 * Callback invoked when the API returns 401 (expired/invalid token), letting the auth
 * layer redirect to the login screen. Set by the AuthProvider.
 */
let onUnauthorized: (() => void) | null = null;

export function setUnauthorizedHandler(handler: () => void): void {
  onUnauthorized = handler;
}

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      onUnauthorized?.();
    }
    return Promise.reject(error);
  },
);
