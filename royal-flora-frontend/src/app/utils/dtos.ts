// DTO (Data Transfer Object) interfaces die overeenkomen met de backend Auth DTOs.
// Deze types worden gebruikt voor typeveiligheid bij requests en responses tussen
// frontend en backend — houd ze in sync met de backend contracten.

// Basisinformatie over een gebruiker zoals teruggegeven door de server
export interface UserInfoDTO {
  id: number;
  username: string;
  email: string;
  role: string;
  // Optioneel KVK-nummer (wordt vaak uit de JWT payload gehaald)
  KVK?: string;
}

// Payload voor het /auth/login endpoint
export interface LoginRequestDTO {
  email: string;
  password: string;
}

// Response van login/register endpoints: successtatus, optionele token en user object
export interface LoginResponseDTO {
  success: boolean;
  message: string;
  token?: string;
  user?: UserInfoDTO;
}

// Payload voor het register endpoint (persoonlijke gegevens + KvK en accountType)
export interface RegisterRequestDTO {
  voorNaam: string;
  achterNaam: string;
  telefoonnummer?: string;
  email: string;
  wachtwoord: string;
  kvkNummer: string;
  accountType: string;
  postcode?: string;
  adres?: string;
}

// Register response gebruikt hetzelfde schema als LoginResponseDTO
export interface RegisterResponseDTO extends LoginResponseDTO {}

// Alias voor user response wanneer een endpoint direct een User-info terugstuurt
export interface UserResponseDTO extends UserInfoDTO {}

// Lege default export zodat dit bestand als module gebruikt kan worden indien gewenst
export default {};
