using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authorization; // Added for authorization policies
using System.Security.Claims; // For claims access

var builder = WebApplication.CreateBuilder(args);

// Define a variable for your CORS policy name (good practice)
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Add CORS services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.AllowAnyOrigin() // Your production frontend URL
                                .AllowAnyHeader()
                                .AllowAnyMethod(); 
                      });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Azure AD Authentication
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "AzureAd");

// Configure Authorization Policies (Optional, but good for complex roles/permissions)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireSuperAdminRole", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("RequireAdminOrSuperAdminRole", policy => policy.RequireRole("Admin", "SuperAdmin"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User", "Admin", "SuperAdmin")); // Everyone is a user
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// 1. Routing must come first to identify the endpoint.
app.UseRouting();

// 2. CORS must come after Routing, but before Authentication/Authorization
//    so that the preflight requests are handled correctly.
app.UseCors(MyAllowSpecificOrigins);

// 3. Authentication must happen BEFORE Authorization.
//    This middleware processes the JWT token and populates the User principal.
app.UseAuthentication(); 

// 4. Authorization checks the permissions based on the authenticated user.
app.UseAuthorization();

// 5. Map controllers after all middleware is configured.
app.MapControllers();

app.Run();