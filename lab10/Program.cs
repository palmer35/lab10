namespace Task0_1_Lexical
{
    public class Program
    {
        static void Main()
        {
            string inputFile = "test.pas";
            string outputFile = "output.txt";

            Console.WriteLine("=== Тест 1: Чтение файла и анализ ===");
            try
            {
                var lexer = new Lexer(inputFile);

                lexer.GenerateTokenCodes(outputFile);
                Console.WriteLine($"Коды лексем записаны в {outputFile}");
                Console.WriteLine("Содержимое: " + File.ReadAllText(outputFile) + "\n");

                lexer.AnalyzeSourceCode();
                lexer.Dispose();
            }
            catch (FileNotFoundException)
            {
                ErrorTable.Report(100);
            }
            catch (IOException)
            {
                ErrorTable.Report(101);
            }

            Console.WriteLine("\n=== Тест 2: Обработка ошибки ===");
            try
            {
                new Lexer("invalid.pas");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Ожидаемая ошибка: файл не найден (тест пройден)");
            }

        }
    }
}
