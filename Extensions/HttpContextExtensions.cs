using System.Security.Claims;

namespace Orchestrate.Extensions
{
    internal static class HttpContextExtensions
    {
        internal static Guid GetUserID(this HttpContext context)
        {
            var claimsPrincipal = context.User;
            string? userID = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Actor)?.Value;
            if (Guid.TryParse(userID, out var id)) { return id; }
            return Guid.Empty;
        }
    }
}
