using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations; // Add for validation attributes

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = false;
});

var app = builder.Build();

// In-memory user store and fast lookup dictionaries
var users = new List<User>();
var usersById = new Dictionary<int, User>();
var usernames = new HashSet<string>();
var emails = new HashSet<string>();
var nextId = 1;

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // Global exception handler for production
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
        });
    });
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

// Minimal API endpoints for User CRUD

// Create user with model validation and optimized duplicate checks
app.MapPost("/users", (User user) =>
{
    try
    {
        // Model validation using DataAnnotations
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(user, null, null);
        if (!Validator.TryValidateObject(user, context, validationResults, true))
        {
            var errors = validationResults.Select(vr => vr.ErrorMessage).ToArray();
            return Results.BadRequest(new { errors });
        }

        if (usernames.Contains(user.Username))
        {
            return Results.BadRequest(new { error = "Username already exists." });
        }
        if (emails.Contains(user.Email))
        {
            return Results.BadRequest(new { error = "Email already exists." });
        }

        user.Id = nextId++;
        users.Add(user);
        usersById[user.Id] = user;
        usernames.Add(user.Username);
        emails.Add(user.Email);
        return Results.Created($"/users/{user.Id}", user);
    }
    catch
    {
        return Results.Problem("An unexpected error occurred while creating the user.");
    }
});

// Read all users with exception handling
app.MapGet("/users", () =>
{
    try
    {
        return Results.Ok(users);
    }
    catch
    {
        return Results.Problem("An unexpected error occurred while retrieving users.");
    }
});

// Read user by id with optimized lookup and exception handling
app.MapGet("/users/{id:int}", (int id) =>
{
    try
    {
        if (usersById.TryGetValue(id, out var user))
        {
            return Results.Ok(user);
        }
        else
        {
            return Results.NotFound(new { error = $"User with ID {id} not found." });
        }
    }
    catch
    {
        return Results.Problem("An unexpected error occurred while retrieving the user.");
    }
});

// Update user with model validation, optimized lookup and exception handling
app.MapPut("/users/{id:int}", (int id, User updatedUser) =>
{
    try
    {
        // Model validation using DataAnnotations
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(updatedUser, null, null);
        if (!Validator.TryValidateObject(updatedUser, context, validationResults, true))
        {
            var errors = validationResults.Select(vr => vr.ErrorMessage).ToArray();
            return Results.BadRequest(new { errors });
        }

        if (!usersById.TryGetValue(id, out var user))
            return Results.NotFound();

        // Check for username/email conflicts only if changed
        if (!string.Equals(user.Username, updatedUser.Username) && usernames.Contains(updatedUser.Username))
        {
            return Results.BadRequest(new { error = "Username already exists." });
        }
        if (!string.Equals(user.Email, updatedUser.Email) && emails.Contains(updatedUser.Email))
        {
            return Results.BadRequest(new { error = "Email already exists." });
        }

        // Update lookup sets if username/email changed
        if (!string.Equals(user.Username, updatedUser.Username))
        {
            usernames.Remove(user.Username);
            usernames.Add(updatedUser.Username);
        }
        if (!string.Equals(user.Email, updatedUser.Email))
        {
            emails.Remove(user.Email);
            emails.Add(updatedUser.Email);
        }

        user.Username = updatedUser.Username;
        user.Email = updatedUser.Email;
        return Results.Ok(user);
    }
    catch
    {
        return Results.Problem("An unexpected error occurred while updating the user.");
    }
});

// Delete user with optimized lookup and exception handling
app.MapDelete("/users/{id:int}", (int id) =>
{
    try
    {
        if (!usersById.TryGetValue(id, out var user))
            return Results.NotFound();

        users.Remove(user);
        usersById.Remove(id);
        usernames.Remove(user.Username);
        emails.Remove(user.Email);
        return Results.NoContent();
    }
    catch
    {
        return Results.Problem("An unexpected error occurred while deleting the user.");
    }
});

app.Run();

// User model with validation attributes
public class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Username is required.")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    public string Email { get; set; }
}