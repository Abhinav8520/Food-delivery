using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace RecommendationService.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtMiddleware> _logger;

    public JwtMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<JwtMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip authentication for health check or Swagger endpoints
        if (context.Request.Path.StartsWithSegments("/swagger") || 
            context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last() 
                   ?? context.Request.Headers["token"].FirstOrDefault();

        if (token != null)
        {
            AttachUserToContext(context, token);
        }
        else
        {
            // For internal service-to-service communication, we might allow requests without token
            // if they're from the same network/localhost. This is a simplified approach.
            // In production, you'd want stricter validation.
            var isLocalhost = context.Connection.RemoteIpAddress?.ToString() == "127.0.0.1" 
                            || context.Connection.RemoteIpAddress?.ToString() == "::1"
                            || context.Request.Headers["User-Agent"].ToString().Contains("Node.js");
            
            if (!isLocalhost)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
        }

        await _next(context);
    }

    private void AttachUserToContext(HttpContext context, string token)
    {
        try
        {
            // Load from environment variable (which should be loaded from backend/.env)
            var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
                           ?? _configuration["JWT:Secret"] 
                           ?? _configuration["JWT_SECRET"];
            if (string.IsNullOrEmpty(jwtSecret))
            {
                _logger.LogWarning("JWT secret not configured");
                return;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSecret);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            context.Items["UserId"] = jwtToken.Claims.First(x => x.Type == "id").Value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate JWT token");
        }
    }
}

public static class JwtMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtMiddleware>();
    }
}

