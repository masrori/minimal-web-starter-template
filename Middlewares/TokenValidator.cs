using Orchestrate.Extensions;
using Orchestrate.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Security.Claims;
namespace Orchestrate.Middlewares
{
    public class TokenValidator
    {
        public Task ValidateAsync(MessageReceivedContext context)
        {
            string requestToken = context.Request.GetToken();
            string requestUserAgent = context.Request.Headers.UserAgent.ToString();
            var requestIPV4 = context.Request.GetIpAddress().ToInet();

            var data = TokenStore.Get(requestToken);
            if (data is null)
            {
                TokenStore.Delete(requestToken);

                context.Fail("Unauthorized request");
                return Task.CompletedTask;
            }


            using var stream = new MemoryStream(data);
            using var reader = new Orchestrate.Binaries.BinaryDataReader(stream);

            var expiredDate = new DateTime(BitConverter.ToInt64(data, 0));
            if (expiredDate < DateTime.Now)
            {
                context.Fail("Expired token");
                return Task.CompletedTask;
            }
            var IPV4 = data.SubBytes(8, 4);
            if (!IPV4.SequenceEqual(requestIPV4))
            {
                context.Fail("IP Address or User Agent do not match");
                return Task.CompletedTask;
            }

            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Actor, BitConverter.ToInt16(data, 14).ToString()),
                new Claim(ClaimTypes.Role, BitConverter.ToInt16(data, 16).ToString())
            }, "Bearer");

            context.Principal = new ClaimsPrincipal(identity);
            context.Success();

            return Task.CompletedTask;
        }
    }

    internal static class TokenStore
    {
        private static ConcurrentDictionary<string, byte[]> dict = new();
        internal static void Set(string token, byte[] value)
        {
            dict[token] = value;
        }
        internal static byte[]? Get(string token)
        {
            return dict.TryGetValue(token, out var value) ? value : null;
        }
        internal static void Delete(string token)
        {
            dict.TryRemove(token, out _);
        }
        internal static void Update(string token, byte[] value)
        {
            dict[token] = value;
        }
        internal static void Delete(short userId)
        {
            foreach (var kv in dict)
            {
                var data = kv.Value;
                if (data.Length < 16) continue;
                var employeeId = BitConverter.ToInt16(data, 14);
                if (employeeId == userId)
                {
                    TokenStore.Delete(kv.Key);
                }
            }
        }
        internal static short GetLocationId(string key)
        {
            var data = TokenStore.Get(key);
            if (data is null) return -1;

            return BitConverter.ToInt16(data, 12);
        }
        internal static void SetLocationId(string key, short locationId)
        {
            var data = TokenStore.Get(key);
            if (data is null) return;

            var bytes = BitConverter.GetBytes(locationId);
            data[12] = bytes[0];
            data[13] = bytes[1];
            TokenStore.Update(key, data);
        }
    }
}