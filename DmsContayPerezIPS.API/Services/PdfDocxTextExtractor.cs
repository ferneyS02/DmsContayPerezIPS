using DocumentFormat.OpenXml.Packaging;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace DmsContayPerezIPS.API.Services
{
    public class PdfDocxTextExtractor : ITextExtractor
    {
        public async Task<string> ExtractAsync(IFormFile file, CancellationToken ct = default)
        {
            if (file == null || file.Length == 0) return string.Empty;

            var name = file.FileName?.ToLowerInvariant() ?? string.Empty;
            var ctType = file.ContentType?.ToLowerInvariant() ?? string.Empty;

            if (name.EndsWith(".pdf") || ctType.Contains("pdf"))
                return await ExtractPdfAsync(file, ct);

            if (name.EndsWith(".docx") || ctType.Contains("officedocument.wordprocessingml.document"))
                return await ExtractDocxAsync(file, ct);

            // Otros tipos: de momento no extraemos
            return string.Empty;
        }

        private static async Task<string> ExtractPdfAsync(IFormFile file, CancellationToken ct)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);
            ms.Position = 0;

            using var reader = new PdfReader(ms);
            using var pdf = new PdfDocument(reader);

            var sb = new StringBuilder();

            for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
            {
                ct.ThrowIfCancellationRequested();
                var page = pdf.GetPage(i);

                var strategy = new SimpleTextExtractionStrategy();
                var text = PdfTextExtractor.GetTextFromPage(page, strategy);

                if (!string.IsNullOrWhiteSpace(text))
                    sb.AppendLine(text);
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

            // Corte de seguridad (~1 MB)
            const int maxChars = 1_000_000;
            if (text.Length > maxChars)
                text = text.Substring(0, maxChars);

            return text;
        }
    }
}
