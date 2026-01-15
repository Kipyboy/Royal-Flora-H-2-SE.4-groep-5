// utils/api.ts
import { getToken } from "./auth";

// Helper wrapper rond fetch die, wanneer beschikbaar, automatisch de JWT Authorization
// header toevoegt aan de request. Gebruik deze functie voor alle API-aanroepen die
// authenticatie met een bearer token vereisen.
//
// Opmerkingen:
// - Als er geen token aanwezig is, worden de oorspronkelijke headers doorgestuurd.
// - Bestaande headers uit `init.headers` blijven behouden en worden uitgebreid met
//   de Authorization header wanneer er een token is.
export function authFetch(input: RequestInfo, init?: RequestInit) {
  const token = getToken();
  const headers: HeadersInit = token
    ? {
        ...init?.headers,
        Authorization: `Bearer ${token}`,
      }
    : init?.headers || {};

  // Voer de fetch uit met de (mogelijk aangepaste) headers
  return fetch(input, { ...init, headers });
}
