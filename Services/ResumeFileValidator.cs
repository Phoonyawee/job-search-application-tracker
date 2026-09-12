using System.IO.Compression;

namespace CareerPilot.Services;

public static class ResumeFileValidator
{
    public const int MaxBytes = 5 * 1024 * 1024;

    public static string? GetContentType(string fileName, byte[] data)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension == ".pdf" && data.AsSpan().StartsWith("%PDF-"u8)) return "application/pdf";
        if (extension != ".docx") return null;

        try
        {
            using var archive = new ZipArchive(new MemoryStream(data), ZipArchiveMode.Read);
            return archive.GetEntry("[Content_Types].xml") is not null && archive.GetEntry("word/document.xml") is not null
                ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                : null;
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    public static void SelfCheck()
    {
        if (GetContentType("resume.pdf", "%PDF-1.7"u8.ToArray()) != "application/pdf") throw new InvalidOperationException("PDF validation failed.");
        if (GetContentType("resume.exe", "%PDF-1.7"u8.ToArray()) is not null) throw new InvalidOperationException("Unsafe extension accepted.");
        if (GetContentType("fake.docx", "not a zip"u8.ToArray()) is not null) throw new InvalidOperationException("Invalid DOCX accepted.");
    }
}
