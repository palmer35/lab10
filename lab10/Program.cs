using System;
using System.IO;

namespace Task0_1_Lexical
{
    class Program
    {
        static void Main()
        {
            string inputFile = "test.pas";
            string outputFile = "output.txt";

            Console.WriteLine("=== Тест 1: Чтение файла и анализ ===");
            try
            {
                var lexer = new Lexer(inputFile);

                // Первый этап: генерация кодов символов
                lexer.GenerateCharCodes(outputFile);
                Console.WriteLine($"Коды символов записаны в {outputFile}");
                Console.WriteLine("Содержимое: " + File.ReadAllText(outputFile));

                // Второй этап: анализ (проверка чисел и ключевых слов)
                lexer.Analyze();

                lexer.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.WriteLine("\n=== Тест 2: Обработка ошибки ===");
            try
            {
                new InputModule("invalid.pas");
            }
            catch
            {
                Console.WriteLine("Ожидаемая ошибка: файл не найден (тест пройден)");
            }
        }
    }
}
