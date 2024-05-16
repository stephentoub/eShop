using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

internal sealed class JohnCustomChatCompletionService(HttpClient http, IConfiguration config) : IChatCompletionService
{
    private readonly string _url = $"https://rag-{config["JohnEndpointVersion"] ?? "v1"}-service.thankfulforest-d25f7acc.westus3.azurecontainerapps.io/query";

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings, Kernel? kernel, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await http.PostAsJsonAsync(_url, new { question = chatHistory[^1].Content }, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await JsonSerializer.DeserializeAsync<JsonNode>(await response.Content.ReadAsStreamAsync(), cancellationToken: cancellationToken);
        return [new ChatMessageContent(AuthorRole.Assistant, [new TextContent(json?["answer"]?.ToString())])];
    }

    public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings, Kernel? kernel, CancellationToken cancellationToken) =>
        throw new NotSupportedException("Streaming not supported");

    public IReadOnlyDictionary<string, object?> Attributes { get; } = new Dictionary<string, object?>();
}
