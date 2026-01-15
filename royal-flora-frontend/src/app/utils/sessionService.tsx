
// Globale sessiehelper die de huidige gebruiker ophaalt via het beveiligde `/auth/user` endpoint.
// We exporteren een named export zodat het eenvoudig in andere modules (en door Turbopack) gebruikt kan worden.
//
// Gedrag:
// - Gebruikt de in localStorage aanwezige JWT (via `getAuthHeaders`) voor autorisatie.
// - Retourneert `null` wanneer er geen token is, wanneer de server een fout teruggeeft
//   of bij netwerk/parsing fouten.
import { getAuthHeaders, getToken } from './auth';
import type { UserResponseDTO } from './dtos';
import { API_BASE_URL } from '../config/api';

// Haal huidige gebruikersgegevens op van de backend (protected endpoint).
// Retourneert `UserResponseDTO` bij succes, anders `null`.
export async function getSessionData(): Promise<UserResponseDTO | null> {
    try {
        // Controleer eerst of er een token aanwezig is; zonder token is er geen sessie
        const token = getToken();
        if (!token) return null;

        // Voer de fetch uit met de Authorization header (via getAuthHeaders)
        const response = await fetch(`${API_BASE_URL}/api/auth/user`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                ...getAuthHeaders(),
            },
        });

        // Bij een niet-ok response (bijv. 401) retourneren we null
        if (!response.ok) return null;

        // Parse het JSON-resultaat en cast naar het verwachte DTO type
        return await response.json() as UserResponseDTO;
    } catch (err) {
        // Fout tijdens netwerkverzoek of JSON parsing: behandel als geen sessie
        return null;
    }
}
