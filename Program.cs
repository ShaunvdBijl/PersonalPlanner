using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using PersonalPlanner.Components;
using PersonalPlanner.Data;
using PersonalPlanner.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// PostgreSQL DbContextFactory for Blazor Interactive Server concurrency safety
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("PostgreSQL connection string 'DefaultConnection' not found.");

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Initialize Firebase Admin SDK
var firebaseKeyPath = builder.Configuration["Firebase:ServiceAccountKeyPath"];
if (!string.IsNullOrWhiteSpace(firebaseKeyPath) && File.Exists(firebaseKeyPath))
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(firebaseKeyPath),
        ProjectId = builder.Configuration["Firebase:ProjectId"]
    });
}
else if (FirebaseApp.DefaultInstance is null)
{
    FirebaseApp.Create(new AppOptions
    {
        ProjectId = builder.Configuration["Firebase:ProjectId"]
    });
}

// Register Authentication & Planner Services
builder.Services.AddScoped<PlannerAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<PlannerAuthStateProvider>());
builder.Services.AddScoped<FirebaseAuthService>();
builder.Services.AddScoped<PlannerService>();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Ensure PostgreSQL schema is created/migrated on startup
using (var scope = app.Services.CreateScope())
{
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var db = await dbFactory.CreateDbContextAsync();
    await db.Database.MigrateAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();