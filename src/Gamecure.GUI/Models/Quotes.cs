using System;
using System.Collections.Generic;
using System.IO;

namespace Gamecure.GUI.Models;

public record Quote(string Text, string Author);
internal static class Quotes
{
    public static Quote[] ReadQuotes()
    {
        using var quotesStream = typeof(Quote).Assembly.GetManifestResourceStream("Gamecure.GUI.Assets.quotes.txt");

        if (quotesStream == null)
        {
            return Array.Empty<Quote>();
        }

        List<Quote> quotes = new();
        using var reader = new StreamReader(quotesStream);
        while (!reader.EndOfStream)
        {
            var text = reader.ReadLine();
            var author = reader.ReadLine();
            if (text != null && author != null)
            {
                quotes.Add(new Quote(text, author));
            }
        }
        return quotes.ToArray();
    }
}