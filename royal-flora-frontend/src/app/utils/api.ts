// utils/api.ts
import { getToken } from "./auth";

/**
 * Wrapper around fetch that includes JWT Authorization header if available
 */
export function authFetch(input: RequestInfo, init?: RequestInit) {
  const token = getToken();
  const headers: HeadersInit = token
    ? {
        ...init?.headers,
        Authorization: `Bearer ${token}`,
      }
    : init?.headers || {};

  return fetch(input, { ...init, headers });
}
