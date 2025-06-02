using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Task0_1_Lexical
{
    public class Lexer : IDisposable
    {
        private readonly string _filename;
        private readonly InputModule _input;
        private readonly Dictionary<string, int> _keywords = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "program", 1 },
            { "var",     2 },
            { "const",   3 },
            { "begin",   4 },
            { "end",     5 },
            { "integer", 6 },
            { "array",   7 },
            { "of",      8 }
        };

        private readonly HashSet<string> _symbols = new HashSet<string>
        {
            ":=", "=", "+", "-", "*", "/", "(", ")", ";", ",", ".", ":"
        };

        public Lexer(string filename)
        {
            _filename = filename;
            _input = new InputModule(filename);
        }

        public void GenerateCharCodes(string outputFilename)
        {
            var text = File.ReadAllText(_filename);
            char[] seps = new[] { ' ', '\r', '\n', '\t', ';', ',', '.', '(', ')', ':', '=', '+', '-', '*', '/' };
            var tokens = text.Split(seps, StringSplitOptions.RemoveEmptyEntries);

            using var writer = new StreamWriter(outputFilename);
            foreach (var token in tokens)
            {
                if (_keywords.TryGetValue(token, out int code))
                {
                    writer.Write($"{code} ");
                }
                else if (long.TryParse(token, out long num))
                {
                    if (num > int.MaxValue || num < int.MinValue)
                    {
                        writer.Write("ERR200 ");
                    }
                    else
                    {
                        writer.Write($"{token} ");
                    }
                }
                else
                {
                    writer.Write("ID ");
                }
            }
        }

        public void Analyze()
        {
            var lines = File.ReadAllLines(_filename);
            int errorCount = 0;
            bool hasBegin = false, hasEnd = false;

            var outputTokens = File.ReadAllText("output.txt").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            bool hasErr200 = outputTokens.Contains("ERR200");

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                Console.WriteLine($"{i + 1,2} {line}");
                string trimmed = line.TrimStart();

                // === Проверка 1: program ...;
                if (i == 0 && !trimmed.StartsWith("program", StringComparison.OrdinalIgnoreCase))
                {
                    errorCount++;
                    PrintInlineError(errorCount, 207, "Ожидалось ключевое слово 'program'");
                    continue;
                }
                else if (trimmed.StartsWith("program", StringComparison.OrdinalIgnoreCase) &&
                         !line.TrimEnd().EndsWith(";"))
                {
                    errorCount++;
                    PrintInlineError(errorCount, 201, "Отсутствует символ ';' после заголовка program");
                }

                // === Проверка 2: var должен быть обязательно (проверяем на второй строке)
                if (i == 1)
                {
                    if (trimmed.StartsWith("va", StringComparison.OrdinalIgnoreCase)
                        && !trimmed.StartsWith("var", StringComparison.OrdinalIgnoreCase))
                    {
                        errorCount++;
                        PrintInlineError(errorCount, 100, "тип метки не совпадает...");
                    }
                    else if (!lines.Any(l => l.TrimStart().StartsWith("var", StringComparison.OrdinalIgnoreCase)))
                    {
                        errorCount++;
                        PrintInlineError(errorCount, 207, "Ожидалось ключевое слово 'var'");
                    }
                }

                // === Проверка 4: есть ли begin/end
                if (trimmed.StartsWith("begin", StringComparison.OrdinalIgnoreCase)) hasBegin = true;
                if (trimmed.StartsWith("end", StringComparison.OrdinalIgnoreCase)) hasEnd = true;

                // === Проверка 3: ключевые слова, числа, идентификаторы и символы
                var tokens = line.Split(new[] { ' ', '\t', '(', ')', ':', ';', ',', '.' },
                                        StringSplitOptions.RemoveEmptyEntries);

                foreach (var token in tokens)
                {
                    if (_keywords.ContainsKey(token))
                        continue;

                    if (long.TryParse(token, out _))
                        continue;

                    if (_symbols.Contains(token))
                        continue;

                    if (IsIdentifier(token))
                        continue;

                    errorCount++;
                    PrintInlineError(errorCount, 203, $"Недопустимый токен '{token}'");
                }
            }

            // === Проверка 4 (продолжение): отсутствие begin/end
            if (!hasBegin || !hasEnd)
            {
                errorCount++;
                PrintInlineError(errorCount, 206, "Отсутствует блок begin ... end");
            }

            // === Проверка 5: есть ли ERR200 в output.txt
            if (hasErr200)
            {
                errorCount++;
                PrintInlineError(errorCount, 200, "Числовой литерал вне допустимого диапазона");
            }

            Console.WriteLine($"\nКомпиляция окончена: ошибок – {errorCount} !");
        }

        private bool IsIdentifier(string token)
        {
            return char.IsLetter(token[0]) && token.All(c => char.IsLetterOrDigit(c) || c == '_');
        }

        private void PrintInlineError(int errorIndex, int errorCode, string message)
        {
            Console.WriteLine($"    **{errorIndex:00}** ^ ошибка код {errorCode}");
            Console.WriteLine($"    ****** {message}");
        }

        public void Dispose() { }
    }
}