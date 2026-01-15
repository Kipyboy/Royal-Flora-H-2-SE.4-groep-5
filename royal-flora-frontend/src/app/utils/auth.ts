// Sleutelnaam onder welke de JWT token in localStorage wordt opgeslagen
export const TOKEN_KEY = "jwt_token";

// Slaat de JWT token op of verwijdert deze wanneer null doorgegeven wordt
export function setToken(token: string | null) {
  if (token) {
    localStorage.setItem(TOKEN_KEY, token);
  } else {
    localStorage.removeItem(TOKEN_KEY);
  }
}

// Haalt de JWT token uit localStorage (of null als deze niet aanwezig is)
export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

// Slaat minimaal user-object in localStorage (als JSON) of verwijdert het
export function setUser(user: any) {
  if (user) localStorage.setItem("user", JSON.stringify(user));
  else localStorage.removeItem("user");
}

/**
 * Haalt de user op uit localStorage en vult eventueel het KVK-veld aan op basis
 * van de JWT payload (indien aanwezig). Dit is handig omdat sommige endpoints
 * of pagina's KVK verwachten maar de stored user mogelijk geen KVK bevat.
 *
 * Opmerkingen:
 * - Als de opgeslagen user niet te parsen is, wordt de entry verwijderd.
 * - JWT decoding gebeurt client-side en is alleen bedoeld als handige aanvulling
 *   (vertrouw niet blind op client-side data voor security-gevoelige logica).
 */
export function getUser(): { id: number; username: string; email: string; role: string; KVK?: string } | null {
  const userRaw = localStorage.getItem("user");
  const token = getToken();

  let user: any = null;
  if (userRaw) {
    try {
      user = JSON.parse(userRaw);
    } catch (err) {
      console.error("Failed to parse user from localStorage:", err, "Raw data:", userRaw);
      localStorage.removeItem("user");
    }
  }

  // Probeer KVK te lezen uit de JWT payload en voeg het toe aan het user object
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

  // Logging voor debugdoeleinden (kan later verwijderd worden)
  return user;
}

/**
 * Retourneert een headers-object met de Authorization header gevuld wanneer er een
 * JWT token aanwezig is. Handig om mee te geven aan fetch-aanroepen naar de API.
 */
export function getAuthHeaders(): { Authorization: string } {
  const token = getToken();
  if (!token) {
    return { Authorization: "" };
  }
  return {
    Authorization: `Bearer ${token}`
  };
}

// Log de gebruiker uit door token en user uit localStorage te verwijderen
export function logout() {
  setToken(null);
  localStorage.removeItem("user");
}
