namespace Gateway.Domain.Interfaces;

public interface ILmStudioClient
{
    Task<IEnumerable<LmModel>> GetModelsAsync(CancellationToken cancellationToken = default);
    Task<Stream> ChatCompletionStreamAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default);
}

public record LmModel(string Id, string Object, long Created, string OwnedBy);

public record ChatCompletionRequest(
    string Model,
    IEnumerable<ChatMessage> Messages,
    double? Temperature = null,
    int? MaxTokens = null,
    double? TopP = null,
    bool Stream = true);

public record ChatMessage(string Role, string Content);
