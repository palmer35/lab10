namespace Task2_Parser
{
    class Program
    {
        static void Main()
        {
            string path = "test2.pas";

            if (!File.Exists(path))
            {
                Console.WriteLine("Файл не найден: " + path);
                return;
            }

            string code = File.ReadAllText(path);
            Lexer lexer = new Lexer(code);
            Parser parser = new Parser(lexer);

            parser.Parse();

            var errors = parser.GetErrors();
            if (errors.Count == 0)
            {
                Console.WriteLine("Парсинг прошёл успешно!");
            }
            else
            {
                Console.WriteLine("Найдены ошибки:");
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
            }
        }
    }
}
