using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dan.Plugin.Tilda.Utils;

public static class ResourceManager
{
    public static async Task<List<string>> GetParagraph(string paragraph)
    {
        List<string> resultList = new List<string>();
        var assembly = Assembly.GetExecutingAssembly();
        string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith($"p{paragraph}.txt"));

        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        using (var reader = new StreamReader(stream))
        {
            while (!reader.EndOfStream)
            {
                var contents = await reader.ReadLineAsync();
                resultList.Add(FormatLine(contents));
            }
        }

        return resultList;
    }

    private static string FormatLine(string input)
    {
        return input.Replace(" ", "").Replace(Environment.NewLine, "");
    }
}
