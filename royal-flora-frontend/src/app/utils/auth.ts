// utils/auth.ts

const TOKEN_KEY = 'jwt_token';
const USER_KEY = 'user';

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

export function setUser(user: any | null) {
    if (user) {
        localStorage.setItem(USER_KEY, JSON.stringify(user));
    } else {
        localStorage.removeItem(USER_KEY);
    }
}

export function getUser(): any | null {
    const data = localStorage.getItem(USER_KEY);
    return data ? JSON.parse(data) : null;
}

export function getAuthHeaders(): Record<string, string> {
    const token = getToken();
    return token ? { Authorization: `Bearer ${token}` } : {};
}

export function logout() {
    setToken(null);
    setUser(null);
    window.location.href = '/login';
}

export default { setToken, getToken, getUser, setUser, getAuthHeaders, logout };
