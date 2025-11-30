// utils/api.ts
import { getToken, setToken } from "./auth";

export const TOKEN_KEY = "jwt_token";

export function authFetch(input: RequestInfo, init?: RequestInit) {
  const token = getToken();
  const headers = token
    ? {
        ...init?.headers,
        Authorization: `Bearer ${token}`,
      }
    : init?.headers;

  return fetch(input, { ...init, headers });
}

// Named export for logout
export function logout() {
  setToken(null);
  localStorage.removeItem("user");
}
