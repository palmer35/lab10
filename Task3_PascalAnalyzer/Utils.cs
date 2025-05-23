using System;

public static class Utils
{
    // Пример: проверка, входит ли число в диапазон допустимых значений (32-бит int)
    public static bool CheckIntRange(string numberStr)
    {
        if (int.TryParse(numberStr, out int value))
        {
            return true;
        }
        return false;
    }

    // Можно добавить функцию пропуска токена, если надо (зависит от архитектуры)
    // Например, пропустить ожидаемый токен или выдать ошибку
    public static bool ExpectToken(ref Token currentToken, TokenType expectedType, string expectedValue, Lexer lexer)
    {
        if (currentToken.Type == expectedType && currentToken.Value == expectedValue)
        {
            currentToken = lexer.GetToken();
            return true;
        }
        ErrorHandler.Add($"Ошибка: ожидался токен '{expectedValue}', найден '{currentToken.Value}'");
        return false;
    }
}
