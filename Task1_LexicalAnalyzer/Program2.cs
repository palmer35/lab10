using System;
using System.IO;
using Task0_1_Lexical;

namespace Task2_3_ParserSemantic
{
    class Program2
    {
        static void Main(string[] args)
        {
            string path = "test.pas";

            if (!File.Exists(path))
            {
                Console.WriteLine("Файл не найден: " + path);
                return;
            }

            var lexer = new Lexer(path);
            lexer.GenerateCharCodes("output.txt");

            var raw = File.ReadAllText("output.txt");
            var tokens = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            Console.WriteLine("\nСинтаксический анализ:");
            var parser = new Parser(tokens);
            var tree = parser.ParseProgram();

            Console.WriteLine("\nСемантический анализ:");
            var semantic = new SemanticAnalyzer();
            semantic.Analyze(tree);
        }
    }
}
