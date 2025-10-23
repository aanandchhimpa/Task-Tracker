using System;
using System.Security.Claims;

namespace TaskManagement.Api.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            if (user == null) return null;
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (int.TryParse(sub, out var id)) return id;
            return null;
        }
    }
}