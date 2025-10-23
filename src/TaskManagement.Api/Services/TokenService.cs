using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.Api.Helpers;
using TaskManagement.Core.Entities;

namespace TaskManagement.Api.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user, IEnumerable<string> roles = null);
    }

    public class TokenService : ITokenService
    {
        private readonly JwtSettings _settings;
        private readonly byte[] _keyBytes;

        public TokenService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
            _keyBytes = Encoding.UTF8.GetBytes(_settings.Secret);
        }

        public string GenerateToken(User user, IEnumerable<string> roles = null)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, $"{user.FirstName} {user.LastName}"),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
            };

            if (roles != null)
            {
                foreach (var r in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, r));
                }
            }

            var creds = new SigningCredentials(new SymmetricSecurityKey(_keyBytes), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(_settings.ExpiresMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}