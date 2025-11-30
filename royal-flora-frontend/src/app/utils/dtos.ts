// DTO interfaces mirroring backend Auth DTOs

export interface UserInfoDTO {
  id: number;
  username: string;
  email: string;
  role: string;
  KVK?: string;
}

export interface LoginRequestDTO {
  email: string;
  password: string;
}

export interface LoginResponseDTO {
  success: boolean;
  message: string;
  token?: string;
  user?: UserInfoDTO;
}

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

export interface RegisterResponseDTO extends LoginResponseDTO {}

export interface UserResponseDTO extends UserInfoDTO {}

export default {};
