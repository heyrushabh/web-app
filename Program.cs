using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AnimalFactsAuthApp.Data;
using AnimalFactsAuthApp.Models;
using AnimalFactsAuthApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["DB_CONNECTION_STRING"]
    ?? builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Set DB_CONNECTION_STRING or ConnectionStrings:Default.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddSingleton<PasswordService>();
builder.Services.AddSingleton<AnimalFactService>();
builder.Services.AddHealthChecks();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "AnimalFacts.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = 401; return Task.CompletedTask; };
        options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");

var auth = app.MapGroup("/api/auth");

auth.MapPost("/register", async (RegisterRequest request, AppDbContext db, PasswordService passwords) =>
{
    var email = request.Email.Trim().ToLowerInvariant();
    if (!new EmailAddressAttribute().IsValid(email)) return Results.BadRequest(new { error = "Enter a valid email address." });
    if (request.Password.Length < 8) return Results.BadRequest(new { error = "Password must be at least 8 characters." });
    if (await db.Users.AnyAsync(x => x.Email == email)) return Results.Conflict(new { error = "An account with this email already exists." });

    db.Users.Add(new User { Email = email, PasswordHash = passwords.Hash(request.Password) });
    await db.SaveChangesAsync();
    return Results.Created("/api/auth/register", new { message = "Account created. You can now sign in." });
});

auth.MapPost("/login", async (LoginRequest request, AppDbContext db, PasswordService passwords, AnimalFactService facts, HttpContext context) =>
{
    var email = request.Email.Trim().ToLowerInvariant();
    var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email);
    if (user is null || !passwords.Verify(request.Password, user.PasswordHash))
        return Results.Json(new { error = "Invalid email or password." }, statusCode: 401);

    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Email, user.Email) };
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
    return Results.Ok(new { message = "Signed in successfully.", fact = facts.GetRandom() });
});

auth.MapPost("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok(new { message = "Signed out." });
}).RequireAuthorization();

app.MapGet("/api/facts/random", (AnimalFactService facts) => Results.Ok(new { fact = facts.GetRandom() })).RequireAuthorization();
app.MapGet("/api/me", (ClaimsPrincipal user) => Results.Ok(new { email = user.FindFirstValue(ClaimTypes.Email) })).RequireAuthorization();

app.Run();
public partial class Program { }
