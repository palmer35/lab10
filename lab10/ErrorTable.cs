public static class ErrorTable
{
    private static readonly Dictionary<int, string> Errors = new Dictionary<int, string>
    {
        {100, "Файл не найден"},
        {101, "Ошибка чтения файла"},
        {200, "Числовой литерал вне диапазона int"},
        {201, "Отсутствует символ ';' после заголовка program"},
        {202, "Недопустимый токен"},
        {203, "Неизвестный идентификатор"},
        {204, "Ожидался оператор ':='"},
        {205, "Незакрытая скобка '('"},
        {206, "Отсутствует begin/end блок"},
        {207, "Ожидалось ключевое слово program/var"},
        {208, "Незакрытая одиночная кавычка"},
        {209, "Лишняя закрывающая }"},
        {210, "Незакрытая {"}
    };

    public static string GetMessage(int code)
    {
        return Errors.TryGetValue(code, out var msg)
            ? msg
            : "Неизвестная ошибка";
    }

    public static void Report(int code)
    {
        Console.WriteLine($"[Ошибка {code}]: {GetMessage(code)}");
    }
}
