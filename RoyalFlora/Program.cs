using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text;

namespace RoyalFlora
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Forwarded Headers voor Caddy reverse proxy
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // Add services to the container.
            builder.Services.AddDbContext<MyDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            // Configure JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddAuthorization();

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(
                              "http://localhost:3000",
                              "http://80.56.53.41:3000",
                              "https://chicken.servegame.com/"  // Vervang met je No-IP domein
                          )
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            var app = builder.Build();

            // zorgen dat er automatisch gemigreerd wordt moest ik neerzetten voor de docker anders werkte het niet
            // Retry logika voor Docker: wacht tot de database klaar is
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                
                var maxRetries = 30;
                var delay = TimeSpan.FromSeconds(5);
                
                for (int i = 0; i < maxRetries; i++)
                {
                    try
                    {
                        logger.LogInformation("Attempting to connect to database (attempt {Attempt}/{MaxRetries})...", i + 1, maxRetries);
                        
                        if (dbContext.Database.CanConnect())
                        {
                            logger.LogInformation("Database connection successful. Running migrations...");
                            dbContext.Database.Migrate();
                            logger.LogInformation("Migrations completed successfully.");
                            break;
                        }
                        else
                        {
                            logger.LogWarning("Cannot connect to database yet, waiting...");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Database connection/migration attempt {Attempt} failed: {Message}", i + 1, ex.Message);
                        
                        if (i == maxRetries - 1)
                        {
                            logger.LogError("Max retries reached. Could not connect to database.");
                            throw;
                        }
                    }
                    
                    Thread.Sleep(delay);
                }
            }

            // Configure the HTTP request pipeline.
            
            // Forwarded Headers moet als eerste middleware staan voor correcte HTTPS detectie
            app.UseForwardedHeaders();
            
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            // Disabled for development to allow SameSite=Lax cookies over HTTP
            // app.UseHttpsRedirection();

            // Use CORS
            app.UseCors("AllowFrontend");

            // Use Authentication and Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles();

            app.MapControllers();

            app.Run();
        }
        
    }
}