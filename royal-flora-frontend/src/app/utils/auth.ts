// Helper utilities for storing and using JWT bearer tokens
const TOKEN_KEY = 'jwt_token';

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

export function getAuthHeaders(): Record<string, string> {
    const token = getToken();
    return token ? { Authorization: `Bearer ${token}` } : {};
}

export function logout() {
    setToken(null);
    localStorage.removeItem('user');
}

export default { setToken, getToken, getAuthHeaders, logout };
