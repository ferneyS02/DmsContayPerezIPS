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
    /// Extrae texto de PDF (texto “nativo”) e imágenes incrustadas como texto no son soportadas aquí (OCR no incluido).
    /// Extrae texto de DOCX vía OpenXML.
    /// </summary>
    public class PdfDocxTextExtractor : ITextExtractor
    {
        public async Task<string?> ExtractAsync(IFormFile file, CancellationToken ct = default)
        {
            if (file == null || file.Length == 0) return null;

            var name = (file.FileName ?? string.Empty).ToLowerInvariant();
            var ctType = (file.ContentType ?? string.Empty).ToLowerInvariant();

            // PDF
            if (name.EndsWith(".pdf") || ctType.Contains("pdf"))
                return await ExtractPdfAsync(file, ct);

            // DOCX
            if (name.EndsWith(".docx") || ctType.Contains("officedocument.wordprocessingml.document"))
                return await ExtractDocxAsync(file, ct);

            // Otros tipos (doc/xlsx/jpg/png): por ahora no se extraen aquí (devuelve null para que el caller decida)
            return null;
        }

        private static async Task<string?> ExtractPdfAsync(IFormFile file, CancellationToken ct)
        {
            // iText7 requiere un stream seekable → copiamos a MemoryStream
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);
            ms.Position = 0;

            try
            {
                using var reader = new PdfReader(ms);
                using var pdf = new PdfDocument(reader);

                var sb = new StringBuilder();

                int pages = pdf.GetNumberOfPages();
                for (int i = 1; i <= pages; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    var page = pdf.GetPage(i);

                    // Estrategia simple de extracción de texto (no OCR)
                    var strategy = new SimpleTextExtractionStrategy();
                    var text = PdfTextExtractor.GetTextFromPage(page, strategy);

                    if (!string.IsNullOrWhiteSpace(text))
                        sb.AppendLine(text);
                }

                var raw = sb.ToString();
                return Normalize(raw);
            }
            catch
            {
                // Si el PDF está cifrado, corrupto o solo tiene imágenes, iText no extraerá texto.
                // Devuelve null para que el caller pueda decidir (p.ej., usar OCR si está disponible).
                return null;
            }
        }

        private static async Task<string?> ExtractDocxAsync(IFormFile file, CancellationToken ct)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);
            ms.Position = 0;

            try
            {
                using var doc = WordprocessingDocument.Open(ms, false);
                var body = doc.MainDocumentPart?.Document?.Body;
                var text = body?.InnerText ?? string.Empty;
                return Normalize(text);
            }
            catch
            {
                // DOCX inválido o dañado
                return null;
            }
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
                text = text[..maxChars];

            return text;
        }
    }
}
