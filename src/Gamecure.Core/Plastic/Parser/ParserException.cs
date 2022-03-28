namespace Gamecure.Core.Plastic.Parser;

public class ParserException : Exception
{
    public string Line { get; }
    public ParserException(string message, string line)
        : base(message)
    {
        Line = line;
    }
}