# API Endpoints Summary — For Testing

## Base URL
http://localhost:5140

## Database
- Type: PostgreSQL
- Host: localhost
- Port: 5433
- Database: appcsharp
- Username: admincsharp
- Password: csharp123

---

## Endpoint 1: Health Check
- Method: GET
- URL: /api/health
- Auth: None
- Description: Checks if the server is running.
- Expected Response: 200 OK → "Ok..Running..."

---

## Endpoint 2: Register (Signup)
- Method: POST
- URL: /api/auth/signup
- Auth: None
- Headers: Content-Type: application/json
- Body:
```json
{
  "nombre": "Test User",
  "email": "test@example.com",
  "password": "password123",
  "rol": "User"
}
```
- Expected Response: 200 OK
```json
{
  "message": "User registered successfully",
  "userId": 2
}
```
- Possible Errors:
  - Email already exists → 500 with error message

---

## Endpoint 3: Login
- Method: POST
- URL: /api/auth/login
- Auth: None
- Headers: Content-Type: application/json
- Body:
```json
{
  "email": "test@example.com",
  "password": "password123"
}
```
- Expected Response: 200 OK
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```
- Possible Errors:
  - Invalid email or password → 500 "Invalid credentials"

---

## Endpoint 4: Get All Users (Admin Only)
- Method: GET
- URL: /api/users
- Auth: Required — Bearer Token with Role "Admin"
- Headers:
  - Authorization: Bearer <token_from_login>
- Expected Response: 200 OK
```json
[
  {
    "id": 1,
    "nombre": "Admin",
    "email": "admin@test.com",
    "rol": "Admin"
  }
]
```
- Possible Errors:
  - No token → 401 Unauthorized
  - Token with non-Admin role → 403 Forbidden

---

## Endpoint 5: Get User by ID (Admin Only)
- Method: GET
- URL: /api/users/{id}
- Auth: Required — Bearer Token with Role "Admin"
- Headers:
  - Authorization: Bearer <token_from_login>
- Example: GET /api/users/1
- Expected Response: 200 OK
```json
{
  "id": 1,
  "nombre": "Admin",
  "email": "admin@test.com",
  "rol": "Admin"
}
```
- Possible Errors:
  - User not found → 404 { "message": "User not found" }
  - No token → 401 Unauthorized

---

## Endpoint 6: Update User (Admin Only)
- Method: PUT
- URL: /api/users/{id}
- Auth: Required — Bearer Token with Role "Admin"
- Headers:
  - Authorization: Bearer <token_from_login>
  - Content-Type: application/json
- Example: PUT /api/users/2
- Body:
```json
{
  "nombre": "Updated Name",
  "email": "updated@example.com",
  "rol": "User"
}
```
- Expected Response: 200 OK
```json
{
  "message": "User updated successfully"
}
```
- Possible Errors:
  - User not found → 404 { "message": "User not found" }
  - No token → 401 Unauthorized

---

## Endpoint 7: Delete User (Admin Only)
- Method: DELETE
- URL: /api/users/{id}
- Auth: Required — Bearer Token with Role "Admin"
- Headers:
  - Authorization: Bearer <token_from_login>
- Example: DELETE /api/users/2
- Expected Response: 200 OK
```json
{
  "message": "User deleted successfully"
}
```
- Possible Errors:
  - User not found → 404 { "message": "User not found" }
  - No token → 401 Unauthorized

---

## Testing Order (Recommended)
1. Hit GET /api/health → Confirm server is alive.
2. Hit POST /api/auth/signup → Create a test user.
3. Hit POST /api/auth/login → Get a JWT token (login as Admin).
4. Hit GET /api/users with the token → Test get all users.
5. Hit GET /api/users/1 → Test get user by ID.
6. Hit PUT /api/users/2 → Test updating a user.
7. Hit DELETE /api/users/2 → Test deleting a user.
8. Hit GET /api/users WITHOUT a token → Verify it returns 401.
9. Login as a non-Admin user → Verify /api/users returns 403.
