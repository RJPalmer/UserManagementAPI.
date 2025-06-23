# UserManagementAPI

A minimal ASP.NET Core Web API for managing users with in-memory storage, model validation, token-based authentication, structured error handling, and request logging.

## Features

- **User CRUD:** Create, read, update, and delete users.
- **In-Memory Storage:** All user data is stored in memory (not persisted).
- **Model Validation:** Uses DataAnnotations to validate user input (username and email).
- **Token Authentication:** Requires a valid Bearer token for all endpoints.
- **Error Handling:** Returns consistent JSON error responses for unhandled exceptions.
- **Logging:** Logs HTTP method, request path, and response status code for each request.
- **Duplicate Checks:** Prevents duplicate usernames and emails.

## Requirements

- [.NET 6.0 SDK or later](https://dotnet.microsoft.com/download)
- Visual Studio Code or another C#-compatible IDE

## Getting Started

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd UserManagementAPI
   ```

2. **Run the API:**
   ```bash
   dotnet run
   ```

3. **Default Token:**
   The API expects the following Bearer token in the `Authorization` header:
   ```
   Authorization: Bearer your-secret-token
   ```
   *(You can change the token value in `Program.cs`.)*

## API Endpoints

All endpoints require the `Authorization` header.

### Create User

- **POST** `/users`
- **Body:**
  ```json
  {
    "username": "johndoe",
    "email": "john@example.com"
  }
  ```
- **Validation:** Username and email are required and must be unique.

### Get All Users

- **GET** `/users`

### Get User by ID

- **GET** `/users/{id}`

### Update User

- **PUT** `/users/{id}`
- **Body:**
  ```json
  {
    "username": "newname",
    "email": "newemail@example.com"
  }
  ```

### Delete User

- **DELETE** `/users/{id}`

## Error Responses

- Returns JSON error messages for validation errors, authentication failures, not found, and server errors.
- Example:
  ```json
  { "error": "Unauthorized: Invalid token." }
  ```

## Logging

Each request is logged to the console with the HTTP method, path, and response status code.

## Notes

- Data is not persisted; restarting the API will clear all users.
- For production, replace in-memory storage and token logic with a database and secure authentication.
