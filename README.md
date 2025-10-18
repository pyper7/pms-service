# PMS (Performance Management System) API

A comprehensive Performance Management System API for TETFund (Tertiary Education Trust Fund).

## Project Structure

This project follows Clean Architecture principles with the following layers:

- **PMS.Domain**: Core business entities and domain logic
- **PMS.Application**: Business logic, services, and interfaces
- **PMS.Infrastructure**: Data access, repositories, and external services
- **PMS.API**: Web API controllers and configuration
- **PMS.Tests**: Unit and integration tests

## Features Implemented

### Authentication System ✅
- JWT-based authentication
- User login with email/password
- Token refresh mechanism
- User profile management
- Role-based authorization
- Password hashing with BCrypt

### Database
- Entity Framework Core with SQL Server
- Code-first migrations
- Soft delete implementation
- Audit fields (CreatedAt, UpdatedAt, etc.)

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd pms-service
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Update connection string** (if needed)
   Edit `PMS.API/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Your connection string here"
     }
   }
   ```

4. **Run the application**
   ```bash
   dotnet run --project PMS.API
   ```

5. **Access Swagger UI**
   Navigate to `https://localhost:7000` (or the port shown in console)

### Default Test User

The application creates a default HR user on startup:

- **Email**: `hr@tetfund.gov.ng`
- **StaffId**: `TET001`
- **Password**: `password123`
- **Role**: `HR`

### Available Roles

The system includes the following organizational hierarchy roles:

- **HR**: Human Resources Officer
- **Director**: Director
- **Deputy Director**: Deputy Director
- **Assistant Director**: Assistant Director
- **Officer**: Officer

### Entity Relationships

The system establishes the following key relationships:

1. **Post ↔ Role**: Each post is associated with a specific role
2. **User ↔ Post**: Users are assigned to posts through PostOccupancy
3. **User ↔ Role**: Users can have multiple roles through UserRole
4. **Post ↔ OrgUnit**: Posts belong to organizational units
5. **Post ↔ Cadre**: Posts are associated with specific cadres

### Seeded Data Structure

The system creates the following seeded data:

- **OrgUnit**: Human Resources Department (HRD)
- **Role**: HR (Human Resources Officer)
- **Post**: HR Admin (Grade 13, linked to HR role)
- **User**: John Doe (hr@tetfund.gov.ng, StaffId: TET001)
- **PostOccupancy**: Maps John Doe to HR Admin post
- **UserRole**: Assigns HR role to John Doe

## API Endpoints

### Authentication Endpoints

#### POST /api/v1/auth/login
Authenticate user and get JWT token.

**Request (Email Login):**
```json
{
  "emailOrStaffId": "hr@tetfund.gov.ng",
  "password": "password123"
}
```

**Request (StaffId Login):**
```json
{
  "emailOrStaffId": "TET001",
  "password": "password123"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "user": {
      "id": 1,
      "email": "hr@tetfund.gov.ng",
      "name": "John Doe",
      "role": "HR",
      "position": "Administrative Officer",
      "staffId": "TET001",
      "phoneNumber": "08012345678",
      "designation": "Administrative Officer"
    },
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "guid-here",
    "expiresIn": 86400
  }
}
```

#### GET /api/v1/auth/profile
Get current user profile (requires authentication).

**Headers:**
```
Authorization: Bearer <jwt_token>
```

#### POST /api/v1/auth/refresh
Refresh JWT token using refresh token.

**Request:**
```json
{
  "refreshToken": "your-refresh-token"
}
```

#### POST /api/v1/auth/logout
Logout user and invalidate refresh token (requires authentication).

**Headers:**
```
Authorization: Bearer <jwt_token>
```

## Testing the API

### Using Swagger UI
1. Open `https://localhost:7000` in your browser
2. Click "Authorize" button
3. Enter the JWT token from login response
4. Test the protected endpoints

### Using HTTP Files
Use the provided `PMS.API.http` file in Visual Studio or VS Code with REST Client extension.

### Using Postman/Insomnia
1. Import the API endpoints
2. Use the login endpoint to get a token
3. Add the token to Authorization header for protected endpoints

## Configuration

### JWT Settings
Configure JWT settings in `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "PMS.API",
    "Audience": "PMS.Users",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Database Connection
Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PMS_DB;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

## Next Steps

The authentication system is now complete. The next implementation phases include:

1. **Post Management** - CRUD operations for organizational posts
2. **Role Management** - User roles and permissions
3. **Officer Management** - Employee management
4. **Appraisal System** - Performance evaluation
5. **Dashboard APIs** - Analytics and reporting

## Development Notes

- The application uses Entity Framework Core with Code First approach
- JWT tokens are stateless and don't require server-side storage
- Refresh tokens are currently stored in memory (use Redis for production)
- All passwords are hashed using BCrypt
- Soft delete is implemented for all entities
- CORS is configured to allow all origins (restrict for production)

## Troubleshooting

### Common Issues

1. **Database connection errors**: Ensure SQL Server is running and connection string is correct
2. **JWT token errors**: Check that the secret key is at least 32 characters long
3. **CORS errors**: Ensure the frontend URL is allowed in CORS policy
4. **Authentication failures**: Verify user credentials and token validity

### Logs
Check the console output for detailed error messages and logs.
