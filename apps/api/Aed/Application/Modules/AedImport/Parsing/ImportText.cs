using System.Text;

namespace Application.Modules.AedImport.Parsing;

/// <summary>
/// Text of an imported file: UTF-8 (with or without BOM), else Windows-1250, the encoding of CSV files saved
/// by Polish Excel.
/// </summary>
public static class ImportText
{
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    static ImportText()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <summary>
    /// Decodes the bytes; the BOM is removed.
    /// </summary>
    public static string Decode(byte[] content)
    {
        int start = content.Length >= 3 && content[0] == 0xEF && content[1] == 0xBB && content[2] == 0xBF ? 3 : 0;
        try
        {
            return StrictUtf8.GetString(content, start, content.Length - start);
        }
        catch (DecoderFallbackException)
        {
            return Encoding.GetEncoding(1250).GetString(content);
        }
    }
}
