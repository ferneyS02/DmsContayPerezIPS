namespace DmsContayPerezIPS.API.Services
{
    public interface ITextExtractor
    {
        Task<string?> ExtractAsync(IFormFile file, CancellationToken ct = default);
    }
}
