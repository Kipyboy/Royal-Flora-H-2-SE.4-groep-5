// DTO interfaces mirroring backend Auth DTOs

export interface UserInfoDTO {
  id: number;
  username: string;
  email: string;
  role: string;
}

export interface LoginRequestDTO {
  Email: string;
  Password: string;
}

export interface LoginResponseDTO {
  Success: boolean;
  Message: string;
  Token?: string;
  token?: string;
  User?: UserInfoDTO;
  user?: UserInfoDTO;
}

export interface RegisterRequestDTO {
  VoorNaam: string;
  AchterNaam: string;
  Telefoonnummer?: string;
  E_mail: string;
  Wachtwoord: string;
  KvkNummer: string;
  AccountType: string;
  Postcode?: string;
  Adres?: string;
}

export interface RegisterResponseDTO extends LoginResponseDTO {}

export interface UserResponseDTO extends UserInfoDTO {}

export default {};
