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

    // Первый этап: генерация кодов символов
    public void GenerateCharCodes(string outputFilename)
    {
        using var writer = new StreamWriter(outputFilename);
        while (!_input.EndOfFile)
        {
            char ch = _input.NextChar();
            if (!_input.EndOfFile && ch != '\r' && ch != '\n')
                writer.Write((int)ch + " ");
        }
    }


    // Второй этап: проверка чисел и ключевых слов
    public void Analyze()
    {
        _input.Dispose();
        var text = File.ReadAllText("test.pas");

        // Проверка чисел
        foreach (var word in text.Split(" \n\t;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
        {
            if (int.TryParse(word, out _) && IsNumberInvalid(word))
            {
                Console.WriteLine("[Ошибка 200]: Число вне диапазона - " + word);
                ErrorTable.Report(200);
            }
        }

        // Проверка ключевых слов
        foreach (var word in text.Split(" \n\t;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
        {
            if (_keywords.Contains(word.ToLower()))
                Console.WriteLine($"Ключевое слово: {word}");
        }
    }

    private bool IsNumberInvalid(string num)
    {
        return !int.TryParse(num, out int _) || long.Parse(num) > int.MaxValue;
    }

    public void Dispose() => _input.Dispose();
}