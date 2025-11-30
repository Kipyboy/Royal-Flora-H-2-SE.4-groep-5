export const TOKEN_KEY = "jwt_token";

export function setToken(token: string | null) {
  if (token) {
    localStorage.setItem(TOKEN_KEY, token);
  } else {
    localStorage.removeItem(TOKEN_KEY);
  }
}

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function setUser(user: any) {
  if (user) localStorage.setItem("user", JSON.stringify(user));
  else localStorage.removeItem("user");
}

/**
 * Returns the user from localStorage, ensuring KVK is populated from JWT
 */
export function getUser(): { id: number; username: string; email: string; role: string; KVK?: string } | null {
  const userRaw = localStorage.getItem("user");
  const token = getToken();

  let user: any = null;
  if (userRaw) {
    try {
      user = JSON.parse(userRaw);
    } catch {}
  }

  if (token) {
    try {
      const payloadBase64 = token.split('.')[1];
      const decoded = JSON.parse(atob(payloadBase64));
      if (decoded.KVK) {
        user = { ...user, KVK: decoded.KVK };
      }
    } catch (err) {
      console.error("Failed to decode JWT:", err);
    }
  }

  console.log("Loaded user from localStorage:", user);
  console.log("JWT token:", token);
  return user;
}

export function logout() {
  setToken(null);
  localStorage.removeItem("user");
}
