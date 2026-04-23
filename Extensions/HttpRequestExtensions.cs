using Orchestrate.Middlewares;
namespace Orchestrate.Extensions
{
    internal static class HttpRequestExtensions
    {
        internal static async Task<MemoryStream> GetMemoryStreamAsync(this HttpRequest request)
        {
            request.EnableBuffering();

            var memoryStream = new MemoryStream();
            await request.Body.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            request.Body.Position = 0;

            return memoryStream;
        }
        internal static string GetToken(this HttpRequest request)
        {
            string? authHeader = request.Headers["Authorization"].FirstOrDefault();
            string? token = null;

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeader.Substring("Bearer ".Length).Trim();
            }
            return token is null ? "" : token;
        }
        internal static short GetLocationID(this HttpRequest request)
        {
            return TokenStore.GetLocationId(request.GetToken());
        }
        internal static string GetIpAddress(this HttpRequest request)
        {
            // Cek header X-Forwarded-For
            if (request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
            {
                // Bisa ada beberapa IP, ambil yang pertama
                var ip = forwarded.ToString().Split(',')[0];
                return ip;
            }

            // Kalau tidak ada header, ambil langsung dari koneksi
            var ipaddress = request.HttpContext.Connection.RemoteIpAddress?.ToString();
            return ipaddress is null ? "127.0.0.1" : ipaddress;
        }
    }
}