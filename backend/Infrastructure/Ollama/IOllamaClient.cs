namespace backend.Infrastructure.Ollama;

public interface IOllamaClient
{
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
}
