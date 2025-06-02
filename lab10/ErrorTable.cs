public static class ErrorTable
{
    private static readonly Dictionary<int, string> Errors = new Dictionary<int, string>
    {
        {100, "Файл не найден"},
        {101, "Ошибка чтения файла"},
        {200, "Число вне диапазона"},
        {201, "Отсутствует символ ';' после заголовка program"},
        {202, "Не объявлена переменная"},
        {203, "Неизвестный идентификатор"},
        {204, "Ожидался оператор ':='"},
        {205, "Несоответствие типов"},
        {206, "Отсутствует begin/end блок"},
        {207, "Ожидался ключевое слово"},
    };

    public static void Report(int code)
    {
        if (Errors.TryGetValue(code, out string message))
            Console.WriteLine($"[Ошибка {code}]: {message}");
        else
            Console.WriteLine($"[Ошибка {code}]: Неизвестная ошибка");
    }
}
