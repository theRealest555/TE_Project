using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TE_Project.Entities;

namespace TE_Project.Helpers
{
    public static class JwtHelper
    {
        /// <summary>
        /// Validates a JWT token
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <param name="key">Secret key used to sign the JWT token</param>
        /// <param name="issuer">Token issuer</param>
        /// <param name="audience">Token audience</param>
        /// <returns>True if token is valid, otherwise false</returns>
        public static bool ValidateToken(string token, string key, string issuer, string audience)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters(key, issuer, audience);

            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the user ID from a JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>User ID or null if token is invalid</returns>
        public static string? GetUserIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                return jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Creates claims for a JWT token
        /// </summary>
        /// <param name="user">User to create claims for</param>
        /// <param name="roles">Roles assigned to the user</param>
        /// <returns>List of claims</returns>
        public static List<Claim> CreateClaims(User user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("FullName", user.FullName),
                new Claim("PlantId", user.PlantId.ToString()),
                new Claim("IsSuperAdmin", user.IsSuperAdmin.ToString()),
                new Claim("RequirePasswordChange", user.RequirePasswordChange.ToString())
            };
            
            // Add roles as claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            return claims;
        }

        /// <summary>
        /// Gets token validation parameters
        /// </summary>
        private static TokenValidationParameters GetValidationParameters(string key, string issuer, string audience)
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ClockSkew = TimeSpan.Zero
            };
        }
    }
}