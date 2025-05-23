public static class ErrorTable
{
    private static readonly Dictionary<int, string> Errors = new Dictionary<int, string>
    {
        {100, "Файл не найден"},
        {101, "Ошибка чтения файла"}
    };

    public static void Report(int code)
    {
        if (Errors.TryGetValue(code, out string message))
            Console.WriteLine($"[Ошибка {code}]: {message}");
    }
}