using Microsoft.SemanticKernel.Embeddings;
using Pgvector;

namespace eShop.Catalog.API.Services;

public sealed class CatalogAI : ICatalogAI
{
    private const int EmbeddingDimensions = 384;
    private readonly ITextEmbeddingGenerationService _embeddingGenerator;

    /// <summary>The web host environment.</summary>
    private readonly IWebHostEnvironment _environment;
    /// <summary>Logger for use in AI operations.</summary>
    private readonly ILogger _logger;

    public CatalogAI(IWebHostEnvironment environment, ILogger<CatalogAI> logger, ITextEmbeddingGenerationService embeddingGenerator = null)
    {
        _embeddingGenerator = embeddingGenerator;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>Gets whether the AI system is enabled.</summary>
    public bool IsEnabled => _embeddingGenerator is not null;

    /// <summary>Gets an embedding vector for the specified text.</summary>
    public async ValueTask<Vector> GetEmbeddingAsync(string text)
    {
        if (IsEnabled)
        {
            ReadOnlyMemory<float> embedding = await _embeddingGenerator.GenerateEmbeddingAsync(text);
            embedding = embedding[0..EmbeddingDimensions];
            return new Vector(embedding);
        }

        return null;
    }

    /// <summary>Gets an embedding vector for the specified catalog item.</summary>
    public ValueTask<Vector> GetEmbeddingAsync(CatalogItem item) =>
        IsEnabled ? 
            GetEmbeddingAsync(CatalogItemToString(item)) :
            ValueTask.FromResult<Vector>(null);

    /// <summary>Gets embedding vectors for the specified catalog items.</summary>
    public async ValueTask<IReadOnlyList<Vector>> GetEmbeddingsAsync(IEnumerable<CatalogItem> items)
    {
        if (IsEnabled)
        {
            IList<ReadOnlyMemory<float>> embeddings = await _embeddingGenerator.GenerateEmbeddingsAsync(items.Select(CatalogItemToString).ToList());
            return embeddings.Select(m => new Vector(m[0..EmbeddingDimensions])).ToList();
        }

        return null;
    }

    private static string CatalogItemToString(CatalogItem item) => $"{item.Name} {item.Description}";
}
