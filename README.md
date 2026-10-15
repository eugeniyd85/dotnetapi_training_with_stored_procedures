# DotnetAPI: ASP.NET Core Web API with SQL Server Stored Procedures

Built while following the [C# .NET with MS SQL Complete Beginner to Master 2026](https://www.udemy.com/course/net-core-with-ms-sql-beginner-to-expert/) course.

A learning project: a REST API built with ASP.NET Core (.NET 10) that manages users and their posts. Data access goes through SQL Server **stored procedures** called with Dapper, and endpoints are protected with **JWT authentication**.

## Features

- User registration, login and token refresh with JWT (Bearer) authentication
- Password hashing with PBKDF2 (HMAC-SHA256, 100,000 iterations) and a per-user random salt
- CRUD for users (including job title, department and salary) via stored procedures
- CRUD for posts, scoped to the logged-in user (user ID is taken from the JWT claims)
- Optional filtering through query parameters (by user, post ID, search text, active flag)
- Swagger UI for exploring and testing the API
- CORS policies for Development and Production
- Entity Framework Core with a repository pattern (`IUserRepository`) as an alternative data-access approach
- AutoMapper for mapping DTOs to models

## Tech Stack

| Area | Technology |
|---|---|
| Framework | .NET 10, ASP.NET Core Web API |
| Database | SQL Server (`Microsoft.Data.SqlClient`) |
| Data access | Dapper (stored procedures), Entity Framework Core 10 |
| Auth | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| Mapping | AutoMapper |
| API docs | Swashbuckle (Swagger UI) |

## Project Structure

```
Controllers/   API endpoints (Auth, UserComplete, Post, plus legacy User and UserEF)
Data/          DataContextDapper, DataContextEF, IUserRepository / UserRepository
Dtos/          Request and response objects (registration, login, post add/edit, etc.)
Helpers/       AuthHelper (hashing, JWT creation), ReusableSql (shared user upsert)
Models/        User, UserComplete, UserSalary, UserJobInfo, Post
Program.cs     Service registration, authentication, CORS, Swagger
```

> `UserController` (raw SQL with Dapper) and `UserEFController` (Entity Framework Core) are earlier training iterations, kept for comparison. `UserCompleteController` is the one that uses stored procedures.

## API Endpoints

Endpoints marked 🔒 require an `Authorization: Bearer <token>` header.

**Auth**

| Method | Route | Description |
|---|---|---|
| POST | `/Auth/Register` | Register a new user |
| POST | `/Auth/Login` | Log in and get a JWT |
| GET | `/Auth/RefreshToken` 🔒 | Get a new token for the current user |
| PUT | `/Auth/ResetPassword` 🔒 | Set a new password |

**Users** (`UserCompleteController`)

| Method | Route | Description |
|---|---|---|
| GET | `/UserComplete/TestConnection` 🔒 | Check the database connection |
| GET | `/UserComplete/GetUsers/{userId}?isActive=` 🔒 | Get users (`userId = 0` returns all; optional active filter) |
| PUT | `/UserComplete/UpsertUser` 🔒 | Create or update a user |
| DELETE | `/UserComplete/DeleteUser/{userId}` 🔒 | Delete a user |

**Posts**

| Method | Route | Description |
|---|---|---|
| GET | `/Post/Posts/{postId}/{userId}/{searchParam}` 🔒 | Search posts (`0` / `None` mean "no filter") |
| GET | `/Post/MyPosts` 🔒 | Posts of the current user |
| PUT | `/Post/UpsertPost` 🔒 | Create or update a post |
| DELETE | `/Post/DeletePost/{postId}` 🔒 | Delete a post (current user only) |

## Database

The API expects a SQL Server database (default name `DotNetCourseDatabase`) with the schema `TutorialAppSchema`.

**Tables:** `Users`, `UserSalary`, `UserJobInfo`, `Auth`, `Posts`

**Stored procedures used by the API:**

- `spLoginConfirmation_Get`, `spRegistration_Upsert`
- `spUsers_Get`, `spUser_Upsert`, `spUser_Delete`
- `spPosts_Get`, `spPosts_Upsert`, `spPosts_Delete`

## Getting Started

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) and SQL Server (LocalDB, Express or Docker).

1. Clone the repository and open the project folder.
2. Create the database, schema, tables and stored procedures listed above.
3. Configure the connection string and secrets (see below).
4. Run the API:

   ```bash
   dotnet restore
   dotnet run
   ```

5. Open Swagger UI at `http://localhost:5000/swagger` (or `https://localhost:5001/swagger` with the `https` profile).

### Configuration

Settings live in `appsettings.json`:

| Key | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `AppSettings:PasswordKey` | Extra secret mixed into password hashing |
| `AppSettings:TokenKey` | Key used to sign JWTs |

The values committed in `appsettings.json` are placeholders for local development only. For real use, keep secrets out of source control, for example with [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):

```bash
dotnet user-secrets init
dotnet user-secrets set "AppSettings:TokenKey" "<long-random-string>"
dotnet user-secrets set "AppSettings:PasswordKey" "<another-long-random-string>"
```

### Quick Test

1. `POST /Auth/Register` with email, password, password confirmation and user details.
2. `POST /Auth/Login` and copy the `token` from the response.
3. In Swagger UI click **Authorize** and paste the token.
4. Call any 🔒 endpoint, e.g. `GET /Post/MyPosts`.

## What I Practiced

- Building a layered REST API with ASP.NET Core controllers, DTOs and dependency injection
- Calling stored procedures with Dapper and parameterized queries
- Comparing three data-access styles: raw SQL, stored procedures and Entity Framework Core with a repository
- Implementing JWT authentication and secure password hashing
- Documenting an API with Swagger and configuring CORS per environment

## Known Limitations / Ideas for Improvement

- Some queries in `AuthController` and in the legacy `UserController` build SQL by string concatenation; they should use parameters or stored procedures.
- Add a `docker-compose` file for a one-command setup.
- Add unit and integration tests.
- Replace generic `Exception` throws with proper HTTP error responses and global error handling.
