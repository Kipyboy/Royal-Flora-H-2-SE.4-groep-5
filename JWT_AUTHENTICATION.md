# JWT Bearer Authentication - API Documentation

## Overview
This API now uses JWT (JSON Web Token) Bearer authentication instead of sessions. The frontend should store the JWT token received from login/register endpoints and include it in the Authorization header for authenticated requests.

## Authentication Endpoints

### POST /api/auth/login
Login with email and password to receive a JWT token.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Login succesvol",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "username": "John Doe",
    "email": "user@example.com",
    "role": "Inkooper"
  }
}
```

**Error Response (401 Unauthorized):**
```json
{
  "success": false,
  "message": "Ongeldige inloggegevens"
}
```

### POST /api/auth/register
Register a new user account and receive a JWT token.

**Request Body:**
```json
{
  "voorNaam": "John",
  "achterNaam": "Doe",
  "telefoonnummer": "0612345678",
  "e_mail": "user@example.com",
  "wachtwoord": "password123",
  "kvkNummer": "12345678",
  "accountType": "inkooper"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Registratie succesvol",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "username": "John Doe",
    "email": "user@example.com",
    "role": "Inkooper"
  }
}
```

### POST /api/auth/logout
Logout endpoint (client-side token removal).

**Response (200 OK):**
```json
{
  "message": "Succesvol uitgelogd. Verwijder het token client-side."
}
```

Note: With JWT authentication, logout is handled by the client removing the token from storage. No server-side session to clear.

### GET /api/auth/user
Get current authenticated user information from JWT token. Requires Authorization header.

**Request Headers:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Success Response (200 OK):**
```json
{
  "id": 1,
  "username": "John Doe",
  "email": "user@example.com",
  "role": "Inkooper"
}
```

**Error Response (401 Unauthorized):**
```json
{
  "message": "Niet ingelogd"
}
```

## Using JWT Tokens

### Client-Side Implementation

1. **Store the token** after successful login/register:
   ```javascript
   // Save token to localStorage
   localStorage.setItem('jwt_token', response.token);
   ```

2. **Include token in requests**:
   ```javascript
   // Add Authorization header to all authenticated requests
   fetch('/api/protected-endpoint', {
     headers: {
       'Authorization': `Bearer ${localStorage.getItem('jwt_token')}`,
       'Content-Type': 'application/json'
     }
   });
   ```

3. **Remove token on logout**:
   ```javascript
   localStorage.removeItem('jwt_token');
   ```

### Token Expiration
- Tokens expire after 24 hours (1440 minutes)
- Expired tokens will result in 401 Unauthorized responses
- Users must login again to receive a new token

## Security Notes

### Production Deployment
⚠️ **IMPORTANT**: Before deploying to production:

1. **Move JWT Secret Key**: The JWT Key in `appsettings.json` should NOT be used in production.
   - Use environment variables
   - Use Azure Key Vault or similar secret management
   - Use user secrets for development (`dotnet user-secrets set "Jwt:Key" "your-secret-key"`)

2. **Use HTTPS**: JWT tokens should only be transmitted over HTTPS in production.

3. **Enable HTTPS Redirection**: Uncomment `app.UseHttpsRedirection();` in `Program.cs` for production.

4. **Update CORS**: Configure CORS policy to only allow your production frontend domain.

### Token Security
- Tokens are signed using HMAC SHA256
- Tokens include user ID, email, username, and role claims
- Each token has a unique JTI (JWT ID) claim
- Tokens cannot be revoked server-side (this is a limitation of stateless JWT authentication)

## Migration from Session-Based Auth

### Changes for Frontend
If you're migrating from the previous session-based authentication:

1. **Store tokens**: Instead of relying on session cookies, store the JWT token returned from login/register
2. **Include Authorization header**: Add `Authorization: Bearer <token>` header to all authenticated requests
3. **Update logout**: Remove the token from client-side storage instead of calling the server
4. **Update session check**: Use the new `/api/auth/user` endpoint instead of `/api/auth/session`

### Removed Endpoints
- `/api/auth/session` (replaced by `/api/auth/user`)

### API Changes
- Login response now includes `token` field
- Register response now includes `token` field
- All responses no longer set session cookies
