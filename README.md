# Animal Facts Authentication App

A complete ASP.NET Core 10 minimal API app with PostgreSQL, cookie authentication, password hashing, a responsive HTML/CSS/JavaScript frontend, and protected random animal facts.

## Features
- Sign up with email and password
- Duplicate-email validation
- Passwords stored as salted PBKDF2 hashes, never plaintext
- Sign in with a generic error for incorrect credentials
- Secure HTTP-only authentication cookie
- Protected random-fact endpoint
- Sign out and session restoration
- Docker Compose setup with PostgreSQL
- Health endpoint at `/health`

## Run with Docker
1. Install Docker Desktop.
2. Open a terminal in this folder.
3. Run:
   ```bash
   docker compose up --build
   ```
4. Open `http://localhost:8080` in a browser.
5. Stop with `docker compose down`.
6. To also delete the database volume, use `docker compose down -v`.

## Run without Docker
Requirements: .NET 10 SDK and PostgreSQL.

```bash
export DB_CONNECTION_STRING="Host=localhost;Port=5432;Database=animalfacts;Username=postgres;Password=postgres;Gss Encryption Mode=Disable"
dotnet restore
dotnet run
```

## API
- `POST /api/auth/register` body: `{ "email": "user@example.com", "password": "Password123!" }`
- `POST /api/auth/login` body: same as register
- `POST /api/auth/logout` requires authentication
- `GET /api/me` requires authentication
- `GET /api/facts/random` requires authentication
- `GET /health`

## Notes
`EnsureCreated()` is used to keep this learning project simple. For a long-lived production app, use EF Core migrations, HTTPS, secret management, rate limiting, CSRF protection, email verification, password-reset flows, and a managed identity provider.
