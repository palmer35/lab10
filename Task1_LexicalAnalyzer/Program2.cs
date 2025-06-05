using System;
using System.IO;
using System.Collections.Generic;
using Task0_1_Lexical;
using Task2_3_ParserSemantic;

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
            lexer.GenerateTokenCodes("output_lexemes.txt");

            var raw = File.ReadAllText("output_lexemes.txt");
            var tokenStrings = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Преобразование строк в объекты Token
            var tokens = new List<Token>();
            foreach (var item in tokenStrings)
            {
                string code;
                string lexeme;

                if (item.Contains(":"))
                {
                    var parts = item.Split(':', 2);
                    code = parts[0];
                    lexeme = parts[1];
                }
                else
                {
                    code = item;
                    lexeme = null;
                }

                tokens.Add(new Token(code, lexeme));
            }

            Console.WriteLine("\nСинтаксический анализ:");
            var parser = new Parser(tokens);
            var tree = parser.ParseProgram();

            Console.WriteLine("\nСемантический анализ:");
            var semantic = new SemanticAnalyzer();
            semantic.Analyze(tree);
        }
    }
}
