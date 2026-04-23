using Orchestrate.Core;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Orchestrate.pages
{
    public static class Agent
    {
        public static void MapAgentEndPoints(this WebApplication app)
        {
            app.MapPost("/ai/agent/ask", AskGroqAsync);
        }

        internal static async Task<IResult> AskGroqAsync([FromBody] AskRequest request, GroqClient groqClient)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return Results.BadRequest("Message kosong");
            }

            var result = await groqClient.AskGroqAsync(request.Message);

            return Results.Ok(new
            {
                success = true,
                data = result
            });
        }
    }

    public class AskRequest
    {
        [JsonPropertyName("message")] public string? Message { get; set; }
    }
}
