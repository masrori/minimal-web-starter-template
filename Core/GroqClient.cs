using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Orchestrate.Core
{
    public class GroqResponse
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("object")] public string? Object { get; set; }
        [JsonPropertyName("created")] public long Created { get; set; }
        [JsonPropertyName("model")] public string? Model { get; set; }
        [JsonPropertyName("choices")] public Choice[]? Choices { get; set; }
        [JsonPropertyName("usage")] public Usage? Usage { get; set; }
        [JsonPropertyName("usage_breakdown")] public object? UsageBreakdown { get; set; }
        [JsonPropertyName("system_fingerprint")] public string? SystemFingerprint { get; set; }
        [JsonPropertyName("x_groq")] public XGroq? XGroq { get; set; }
        [JsonPropertyName("service_tier")] public string? ServiceTier { get; set; }
    }

    public class ToolFunction
    {
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("arguments")] public string? Arguments { get; set; }
    }

    public class ToolCall
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("type")] public string? Type { get; set; }
        [JsonPropertyName("function")] public ToolFunction? Function { get; set; }
    }
    public class Choice
    {
        [JsonPropertyName("index")] public int Index { get; set; }
        [JsonPropertyName("message")] public Message? Message { get; set; }
        [JsonPropertyName("logprobs")] public object? Logprobs { get; set; }
        [JsonPropertyName("finish_reason")] public string? FinishReason { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("role")] public string? Role { get; set; }
        [JsonPropertyName("content")] public string? Content { get; set; }
        [JsonPropertyName("tool_calls")] public ToolCall[]? ToolCalls { get; set; }
    }

    public class Usage
    {
        [JsonPropertyName("queue_time")] public double QueueTime { get; set; }
        [JsonPropertyName("prompt_tokens")] public int PromptTokens { get; set; }
        [JsonPropertyName("prompt_time")] public double PromptTime { get; set; }
        [JsonPropertyName("completion_tokens")] public int CompletionTokens { get; set; }
        [JsonPropertyName("completion_time")] public double CompletionTime { get; set; }
        [JsonPropertyName("total_tokens")] public int TotalTokens { get; set; }
        [JsonPropertyName("total_time")] public double TotalTime { get; set; }
    }

    public class XGroq
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("seed")] public int Seed { get; set; }
    }
    public class GroqClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly GithubClient _git;
        public GroqClient(HttpClient httpClient, IConfiguration config, GithubClient git)
        {
            _httpClient = httpClient;
            _apiKey = config["Keys:Groq"] ?? "";
            _git = git;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<string> AskGroqAsync(string message)
        {
            var messages = new List<object>
            {
                new { role = "user", content = message }
            };

            // 🔹 STEP 1: initial request
            var response1 = await SendToGroq(messages);
            var msg1 = response1?.Choices?[0]?.Message;

            if (msg1?.ToolCalls?.Length > 0)
            {
                foreach (var toolCall in msg1.ToolCalls)
                {
                    if (toolCall.Function?.Name == "GetRepoList")
                    {
                        // 🔹 STEP 2: execute function
                        var repos = await _git.GetUserReposAsync();
                        var repoNames = string.Join("\n", repos.Select(r => r.Name));

                        // 🔹 STEP 3: append tool result
                        messages.Add(msg1); // assistant message (tool call)
                        messages.Add(new
                        {
                            role = "tool",
                            tool_call_id = toolCall.Id,
                            content = repoNames
                        });

                        // 🔹 STEP 4: send again to LLM
                        var response2 = await SendToGroq(messages);

                        return response2?.Choices?[0]?.Message?.Content ?? "";
                    }
                    else if (toolCall.Function?.Name == "GetUserInfo")
                    {
                        var repoNames = """
                            name: Muhammad Asrori, NIK: 35051030078600002, address: BLITAR
                            """;
                        messages.Add(msg1);
                        messages.Add(new
                        {
                            role = "tool",
                            tool_call_id = toolCall.Id,
                            content = repoNames
                        });

                        // 🔹 STEP 4: send again to LLM
                        var response2 = await SendToGroq(messages);

                        return response2?.Choices?[0]?.Message?.Content ?? "";
                    }
                }
            }

            return ToHtml(msg1?.Content ?? "");
        }
        public string ToHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            // 1. encode dulu (biar aman dari HTML injection)
            var encoded = WebUtility.HtmlEncode(input);

            // 2. bold: **text**
            encoded = Regex.Replace(encoded, @"\*\*(.+?)\*\*", "<strong>$1</strong>");

            // 3. italic: *text*
            encoded = Regex.Replace(encoded, @"\*(.+?)\*", "<em>$1</em>");

            // 4. newline → <br>
            encoded = encoded.Replace("\n", "<br/>");

            return $"<div style='font-family:sans-serif'>{encoded}</div>";
        }
        private async Task<GroqResponse?> SendToGroq(List<object> messages)
        {
            var requestBody = new
            {
                model = "qwen/qwen3-32b",
                messages = messages,
                tools = new object[]
                {
                    new
                    {
                        type = "function",
                        function = new
                        {
                            name = "GetRepoList",
                            description = "Get all repositories from GitHub",
                            parameters = new
                            {
                                type = "object",
                                properties = new { },
                                required = new string[] { }
                            }
                        }
                    },
                    new 
                    {
                        type = "function",
                        function = new
                        {
                            name = "GetUserInfo",
                            description = "Get information of current logged in user",
                            parameters = new
                            {
                                type = "object",
                                properties = new { },
                                required = new string[] { }
                            }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("openai/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode) throw new Exception(responseString);
            return JsonSerializer.Deserialize<GroqResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
