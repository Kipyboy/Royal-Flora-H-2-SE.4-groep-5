
// Globale session methode die overal in de frontend gebruikt kan worden.
// Exporteren we direct om een duidelijke named export te hebben voor Turbopack.
import { getAuthHeaders, getToken } from './auth';
import type { UserResponseDTO } from './dtos';
import { API_BASE_URL } from '../config/api';

// Returns current user info by calling the protected `/auth/user` endpoint
// Uses the stored JWT in Authorization header. Returns `null` when no token
// or when the server returns non-ok.
export async function getSessionData(): Promise<UserResponseDTO | null> {
    try {
        const token = getToken();
        if (!token) return null;

        const response = await fetch(`${API_BASE_URL}/api/auth/user`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                ...getAuthHeaders(),
            },
        });

        if (!response.ok) return null;
        return await response.json() as UserResponseDTO;
    } catch (err) {
        // Network or parsing error
        return null;
    }
}
