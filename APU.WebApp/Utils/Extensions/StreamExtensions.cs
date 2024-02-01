using System.Diagnostics;

namespace APU.WebApp.Utils.Extensions;

public static class StreamExtensions
{
    public static async Task<List<string>> ReadAllLinesAsync(this Stream stream)
    {
        try
        {
            if (stream.Position != 0)
                stream.Seek(0, SeekOrigin.Begin);

            using var reader = new StreamReader(stream);

            var lines = new List<string>();
            while (await reader.ReadLineAsync() is { } line)
                lines.Add(line);

            return lines;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return null;
        }
    }
}