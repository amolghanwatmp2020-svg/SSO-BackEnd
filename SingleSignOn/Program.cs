using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Define a variable for your CORS policy name (good practice)
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Add CORS services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          // For production, replace AllowAnyOrigin() with your specific front-end URL:
                          // policy.WithOrigins("https://your-angular-app.azurewebsites.net")
                          policy.AllowAnyOrigin()
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
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User", "Admin", "SuperAdmin"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.

// Enable Swagger in all environments for testing.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// 1. Static Files: This middleware must come first to serve the Angular files.
//    The server will try to find a matching static file (e.g., an image, a JS file)
//    before it routes the request to an API controller.
app.UseStaticFiles();

// 2. Routing: Must come before CORS, Authentication, and Authorization to match
//    the incoming URL to an endpoint.
app.UseRouting();

// 3. CORS: Must come after UseRouting() but before UseAuthentication() and UseAuthorization().
//    This allows the CORS middleware to handle pre-flight OPTIONS requests correctly.
app.UseCors(MyAllowSpecificOrigins);

// 4. Authentication: This processes the JWT token and populates the User principal.
//    It must run before Authorization.
app.UseAuthentication();

// 5. Authorization: This checks the permissions based on the authenticated user.
//    It must run after Authentication.
app.UseAuthorization();

// 6. API Controllers: Map controllers after all middleware is configured.
app.MapControllers();

// 7. Fallback: This is the crucial part for single-page applications like Angular.
//    It redirects all requests that don't match a static file or an API endpoint
//    to the index.html file. This allows Angular to handle client-side routing.
app.MapFallbackToFile("index.html");

app.Run();
