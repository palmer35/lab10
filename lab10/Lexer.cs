namespace Task0_1_Lexical;

public class Lexer : IDisposable
{
    private readonly InputModule _input;
    private readonly HashSet<string> _keywords = new HashSet<string>
    {
        "program", "var", "const", "begin", "end", "integer", "array", "of"
    };

    public Lexer(string filename)
    {
        _input = new InputModule(filename);
    }

    public void GenerateCharCodes(string outputFilename)
    {
        using var writer = new StreamWriter(outputFilename);
        while (!_input.endOfFile)
        {
            char ch = _input.NextChar();
            if (!_input.endOfFile && ch != '\r' && ch != '\n')
                writer.Write((int)ch + " ");
        }
    }

    public void Analyze()
    {
        _input.Dispose();
        var text = File.ReadAllText("test.pas");

        var words = text.Split(" \n\t;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            if (long.TryParse(word, out long value) && (value > int.MaxValue || value < int.MinValue))
            {
                Console.WriteLine("[Ошибка 200]: Число вне диапазона - " + word);
                ErrorTable.Report(200);
            }
        }

        foreach (var word in words)
        {
            if (_keywords.Contains(word.ToLower()))
                Console.WriteLine($"Ключевое слово: {word}");
        }

    }
 
    public void Dispose() => _input.Dispose();
}