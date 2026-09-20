using System.Net.Http.Json;
using System.Text.Json;
using backend.Infrastructure.Exceptions;
using backend.Options;

namespace backend.Infrastructure.Ollama;

public sealed class OllamaClient : IOllamaClient
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaClient(HttpClient httpClient, OllamaOptions options)
    {
        _httpClient = httpClient;
        _options = options;

        if (!Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out var baseUri))
        {
            throw new ArgumentException("The Ollama base URL is invalid.", nameof(options));
        }

        _httpClient.BaseAddress = baseUri;
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("The Ollama prompt is required.", nameof(prompt));
        }

        try
        {
            using var response = await _httpClient.PostAsJsonAsync("/api/generate", new
            {
                model = _options.Model,
                prompt,
                stream = false,
                options = new { temperature = 0 }
            }, cancellationToken);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new DependencyUnavailableException(
                    $"Ollama rejected the prompt. Status: {(int)response.StatusCode} {response.ReasonPhrase}. Details: {body}");
            }

            using var document = JsonDocument.Parse(body);
            var generated = document.RootElement.GetProperty("response").GetString();
            if (string.IsNullOrWhiteSpace(generated))
            {
                throw new DependencyUnavailableException("Ollama returned an empty response.");
            }

            return generated.Trim();
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("The Ollama request timed out.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new DependencyUnavailableException("Unable to reach the Ollama service.", ex);
        }
        catch (JsonException ex)
        {
            throw new DependencyUnavailableException("Ollama returned an invalid response.", ex);
        }
    }
}
