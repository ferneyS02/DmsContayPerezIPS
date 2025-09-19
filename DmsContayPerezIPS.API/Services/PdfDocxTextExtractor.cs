using DocumentFormat.OpenXml.Packaging;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace DmsContayPerezIPS.API.Services
{
    public class PdfDocxTextExtractor : ITextExtractor
    {
        public async Task<string?> ExtractAsync(IFormFile file, CancellationToken ct = default)
        {
            if (file == null || file.Length == 0) return null;

            var name = file.FileName?.ToLowerInvariant() ?? string.Empty;
            var ctType = file.ContentType?.ToLowerInvariant() ?? string.Empty;

            if (name.EndsWith(".pdf") || ctType.Contains("pdf"))
                return await ExtractPdfAsync(file, ct);

            if (name.EndsWith(".docx") || ctType.Contains("officedocument.wordprocessingml.document"))
                return await ExtractDocxAsync(file, ct);

            // Otros tipos (doc/xlsx/jpg/png): por ahora no extraemos
            return null;
        }

        private static async Task<string> ExtractPdfAsync(IFormFile file, CancellationToken ct)
        {
            // PdfPig requiere stream seekable → copiamos a MemoryStream
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);
            ms.Position = 0;

            var sb = new StringBuilder();
            using var pdf = PdfDocument.Open(ms);
            foreach (var page in pdf.GetPages())
            {
                ct.ThrowIfCancellationRequested();
                if (!string.IsNullOrWhiteSpace(page.Text))
                    sb.AppendLine(page.Text);
            }
            return Normalize(sb.ToString());
        }

        private static async Task<string> ExtractDocxAsync(IFormFile file, CancellationToken ct)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);
            ms.Position = 0;

            using var doc = WordprocessingDocument.Open(ms, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            var text = body?.InnerText ?? string.Empty;
            return Normalize(text);
        }

        private static string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            // Elimina nulls, colapsa espacios, corta textos gigantes (protección)
            text = text.Replace("\0", " ");
            text = Regex.Replace(text, @"\s+", " ").Trim();

            // Corte de seguridad (por ejemplo, 1 MB aprox)
            const int maxChars = 1_000_000;
            if (text.Length > maxChars)
                text = text.Substring(0, maxChars);

            return text;
        }
    }
}
