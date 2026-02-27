# Project Flow: How a Request Travels Through the Application

This document traces the exact path of a request, step by step, 
from the moment the user clicks "Login" to the moment they receive a response.

---

## Flow 1: Application Startup (What happens when you run `dotnet run`)

```
Step 1: Program.cs starts
    │
    ├── Step 2: Load settings from appsettings.json
    │       → Connection String (Database URL)
    │       → JWT Secret Key, Issuer, Audience
    │
    ├── Step 3: Register all services (builder.Services)
    │       → AppDbContext      (Database bridge)
    │       → PasswordHasher    (Password encryption)
    │       → JwtProvider       (Token generator)
    │       → AuthService       (Login/Register logic)
    │
    ├── Step 4: Build the app (var app = builder.Build())
    │       → All settings are locked in
    │
    ├── Step 5: Set up the Middleware Pipeline (the order matters!)
    │       → ExceptionMiddleware  (1st: Safety Net)
    │       → HTTPS Redirect       (2nd: Force encryption)
    │       → CORS                 (3rd: Allow frontend)
    │       → Authentication       (4th: Check ID badge)
    │       → Authorization        (5th: Check permissions)
    │       → MapControllers       (6th: Connect URLs to code)
    │
    ├── Step 6: Seed the Database (DbInitializer)
    │       → Create tables if they don't exist
    │       → Insert default Admin user
    │
    └── Step 7: app.Run() → Server is LIVE and listening!
```

---

## Flow 2: Login Request (`POST /api/auth/login`)

```
USER types email + password on the website
    │
    ▼
[1] Frontend sends HTTP POST to /api/auth/login
    Body: { "email": "admin@test.com", "password": "123456" }
    │
    ▼
[2] ExceptionMiddleware catches the request
    → Wraps everything in a try-catch (safety net)
    → If anything explodes, it returns a clean JSON error
    │
    ▼
[3] HTTPS Redirect checks: "Is this HTTPS?"
    → If not, redirect to HTTPS
    │
    ▼
[4] CORS checks: "Is this website allowed?"
    → Checks if the request comes from localhost:3000
    → If yes, allow it
    │
    ▼
[5] Authentication: SKIPPED for login
    → /api/auth/login has NO [Authorize] attribute
    → So the request passes through freely
    │
    ▼
[6] Authorization: SKIPPED for login
    → Same reason, no [Authorize] on AuthController
    │
    ▼
[7] MapControllers routes the URL:
    → "/api/auth/login" matches AuthController → Login()
    │
    ▼
[8] AuthController.Login() receives the request
    → Takes the LoginDto { Email, Password } from the body
    → Calls: _authService.Login(loginDto)
    │
    ▼
[9] AuthService.Login() does the real work:
    │
    ├── Step A: Ask the Database
    │   → _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@test.com")
    │   → Database returns the User object (with hashed password)
    │
    ├── Step B: Verify the Password
    │   → _passwordHasher.Verify("123456", "$2a$11$xyz...")
    │   → BCrypt compares the plain text with the hash
    │   → Returns TRUE if they match
    │
    └── Step C: Generate the Token
        → _jwtProvider.Generate(user)
        → Creates a JWT with: { userId: 1, email: "admin@test.com", role: "Admin" }
        → Signs it with the secret key from appsettings.json
        → Sets expiration to 60 minutes
        → Returns: "eyJhbGciOiJIUzI1NiIs..."
    │
    ▼
[10] AuthController receives the token string
     → Returns: Ok(new { token = "eyJhbGci..." })
     → HTTP 200 response sent to frontend
    │
    ▼
[11] Frontend saves the token (localStorage or cookie)
     → Uses it for all future requests
```

---

## Flow 3: Protected Request (`GET /api/users` — Admin Only)

```
Frontend wants to see the list of users
    │
    ▼
[1] Frontend sends GET /api/users
    Header: "Authorization: Bearer eyJhbGci..."
    │
    ▼
[2] ExceptionMiddleware: Safety net active
    │
    ▼
[3] HTTPS + CORS: Passed
    │
    ▼
[4] Authentication (app.UseAuthentication):
    → Reads the "Bearer" token from the header
    → Decodes the JWT using the secret key
    → Validates: Is it expired? Is the signature valid?
    → Extracts: userId=1, email=admin@test.com, role=Admin
    → Sets HttpContext.User = { Id: 1, Role: "Admin" }
    │
    ▼
[5] Authorization (app.UseAuthorization):
    → Sees [Authorize(Roles = "Admin")] on UsersController
    → Checks HttpContext.User.Role == "Admin"?
    → YES → Request is ALLOWED to continue
    → (If NO → returns HTTP 403 Forbidden immediately)
    │
    ▼
[6] UsersController.GetUsers() runs:
    → _context.Users.Select(u => new { u.Id, u.Nombre, u.Email, u.Rol })
    → Database returns all users
    → Returns: Ok(users)
    │
    ▼
[7] Frontend receives the list of users
    → Displays them on screen
```

---

## Flow 4: Registration (`POST /api/auth/signup`)

```
USER fills out the registration form
    │
    ▼
[1] Frontend sends POST /api/auth/signup
    Body: { "nombre": "Juan", "email": "juan@test.com", 
            "password": "mypass", "rol": "User" }
    │
    ▼
[2-7] Same middleware chain as Login (no [Authorize] needed)
    │
    ▼
[8] AuthController.Signup() receives the request
    → Calls: _authService.Register(registerDto)
    │
    ▼
[9] AuthService.Register() does the work:
    │
    ├── Step A: Check if email already exists
    │   → _context.Users.AnyAsync(u => u.Email == "juan@test.com")
    │   → If TRUE → throw Exception("Email already exists")
    │
    ├── Step B: Hash the password
    │   → _passwordHasher.Hash("mypass")
    │   → Returns: "$2a$11$abc..." (encrypted, unreadable)
    │
    ├── Step C: Create the User object
    │   → new User { Nombre="Juan", Email="juan@test.com", 
    │                 Password="$2a$11$abc...", Rol="User" }
    │
    ├── Step D: Save to Database
    │   → _context.Users.Add(user)        ← Stage the change
    │   → _context.SaveChangesAsync()     ← Execute INSERT INTO users
    │
    └── Step E: Return the new user
    │
    ▼
[10] AuthController returns:
     → Ok(new { message = "User registered successfully", userId = user.Id })
     → HTTP 200 response sent to frontend
```

---

## Flow 5: Error Flow (When Something Goes Wrong)

```
[1] A request arrives at any endpoint
    │
    ▼
[2] ExceptionMiddleware wraps everything in try-catch
    │
    ▼
[3] Something crashes (e.g., database is offline)
    → Exception is thrown!
    │
    ▼
[4] ExceptionMiddleware CATCHES the error:
    │
    ├── Logs the error: _logger.LogError(ex, ex.Message)
    │
    ├── Sets response code: 500 (Internal Server Error)
    │
    └── Builds the response:
        │
        ├── Development? → { message: "Connection refused", 
        │                     stackTrace: "at AuthService.cs line 135..." }
        │
        └── Production?  → { message: "Internal Server Error" }
    │
    ▼
[5] JSON error response sent to frontend
    → The server does NOT crash
    → It continues running for other users
```
