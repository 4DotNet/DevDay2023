using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text;
using DevDay.Contracts;

namespace DevDay.Web.Services;

public class ApiPromptQueueClient(IServiceProvider provider, ILogger<ApiPromptQueueClient> logger)
{
    private readonly StringBuilder _responseBuffer = new();
    private Task? _processPromptTask = null;

    public void Enqueue(string prompt, Func<PromptResponse, Task> handler)
    {
        if (_processPromptTask is not null)
        {
            return;
        }

        _processPromptTask = Task.Run(async () =>
        {
            try
            {
                var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                var json = JsonSerializer.Serialize(new PromptRequest(prompt), options);

                using var body = new StringContent(json, Encoding.UTF8, "application/json");
                using var scope = provider.CreateScope();

                var factory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
                using var client = factory.CreateClient(nameof(ApiPromptQueueClient));
                var response = await client.PostAsync("api/openai/chat", body);

                if (response.IsSuccessStatusCode)
                {
                    await using var stream = await response.Content.ReadAsStreamAsync();

                    await foreach (var chunk in
                        JsonSerializer.DeserializeAsyncEnumerable<ChatChunkResponse>(stream, options))
                    {
                        if (chunk is null)
                        {
                            continue;
                        }

                        _responseBuffer.Append(chunk.Text);

                        var responseText = NormalizeResponseText(_responseBuffer, logger);
                        await handler(
                            new PromptResponse(
                                prompt, responseText));

                        await Task.Delay(1);
                    }
                }
            }
            catch (Exception ex)
            {
                await handler(
                    new PromptResponse(prompt, ex.Message, true));
            }
            finally
            {
                if (_responseBuffer.Length > 0)
                {
                    var responseText = NormalizeResponseText(_responseBuffer, logger);
                    await handler(
                        new PromptResponse(
                            prompt, responseText, true));
                    _responseBuffer.Clear();
                }

                _processPromptTask = null;
            }
        });
    }
    private static string NormalizeResponseText(StringBuilder builder, ILogger logger)
    {
        if (builder is null or { Length: 0 })
        {
            return "";
        }

        var text = builder.ToString();

        logger.LogDebug("Before normalize\n\t{Text}", text);

        text = text.StartsWith("null,") ? text[5..] : text;
        text = text.Replace("\r", "\n")
            .Replace("\\n\\r", "\n")
            .Replace("\\n", "\n");

        text = Regex.Unescape(text);

        logger.LogDebug("After normalize:\n\t{Text}", text);

        return text;
    }
}
