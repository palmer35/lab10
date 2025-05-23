class Program
{
    static void Main()
    {
        var lexer = new Lexer("test.pas");
        var parser = new Parser(lexer);

        parser.ParseProgram();

        if (parser.Errors.Count > 0)
        {
            foreach (var err in parser.Errors)
                System.Console.WriteLine(err);
        }
        else
        {
            System.Console.WriteLine("Программа успешно проанализирована.");
        }
    }
}
