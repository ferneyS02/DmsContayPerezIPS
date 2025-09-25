using DocumentFormat.OpenXml.Packaging;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace DmsContayPerezIPS.API.Services
{
    /// <summary>
    /// Extractor SIN OCR: solo PDF con texto embebido (iText7) y DOCX (OpenXML).
    /// Para PDFs escaneados/imágenes NO habrá texto (devolverá string.Empty).
    /// </summary>
    public class PdfDocxTextExtractor : ITextExtractor
    {
        public async Task<string> ExtractAsync(IFormFile file, CancellationToken ct = default)
        {
            if (file == null || file.Length == 0) return string.Empty;

            var name = (file.FileName ?? string.Empty).ToLowerInvariant();
            var ctType = (file.ContentType ?? string.Empty).ToLowerInvariant();

            if (name.EndsWith(".pdf") || ctType.Contains("pdf"))
                return await ExtractPdfTextAsync(file, ct);

            if (name.EndsWith(".docx") || ctType.Contains("officedocument.wordprocessingml.document"))
                return await ExtractDocxTextAsync(file, ct);

            // Otros tipos: sin soporte de texto
            return string.Empty;
        }

        private static async Task<string> ExtractPdfTextAsync(IFormFile file, CancellationToken ct)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);
            ms.Position = 0;

            var sb = new StringBuilder();

            using (var pdfReader = new PdfReader(ms))
            using (var pdfDoc = new PdfDocument(pdfReader))
            {
                int pages = pdfDoc.GetNumberOfPages();
                for (int i = 1; i <= pages; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    var page = pdfDoc.GetPage(i);
                    var strategy = new LocationTextExtractionStrategy();
                    var text = PdfTextExtractor.GetTextFromPage(page, strategy);
                    if (!string.IsNullOrWhiteSpace(text))
                        sb.AppendLine(text);
                }
            }

            return Normalize(sb.ToString());
        }

        private static async Task<string> ExtractDocxTextAsync(IFormFile file, CancellationToken ct)
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
            text = text.Replace("\0", " ");
            text = Regex.Replace(text, @"\s+", " ").Trim();

            const int maxChars = 1_000_000; // protección
            if (text.Length > maxChars)
                text = text[..maxChars];

            return text;
        }
    }
}
