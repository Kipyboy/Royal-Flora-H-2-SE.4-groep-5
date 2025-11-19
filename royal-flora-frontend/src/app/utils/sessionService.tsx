
// Globale session methode die overal in de frontend gebruikt kan worden.
// Exporteren we direct om een duidelijke named export te hebben voor Turbopack.
export async function getSessionData(): Promise<any | null> {
    try {
        const response = await fetch('http://localhost:5156/api/auth/session', {
            credentials: 'include',
        });
        if (!response.ok) {
            return null;
        }
        return await response.json();
    } catch (err) {
        // In geval van netwerkfout gewoon null teruggeven zodat callers kunnen fallbacken.
        return null;
    }
}
