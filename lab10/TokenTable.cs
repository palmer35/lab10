namespace Task0_1_Lexical
{
    public static class TokenTable
    {
        public static readonly Dictionary<string, int> TokenCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        { "program", 1 }, { "var", 2 }, { "const", 3 }, { "begin", 4 }, { "end", 5 },
        { "integer", 6 }, { "array", 7 }, { "of", 8 },
        { ":=", 9 }, { "=", 10 }, { "+", 11 }, { "-", 12 }, { "*", 13 }, { "/", 14 },
        { "(", 15 }, { ")", 16 }, { ";", 17 }, { ",", 18 }, { ".", 19 }, { ":", 20 },
        { "0", 21 }, { "1", 22 }, { "2", 23 }, { "3", 24 }, { "4", 25 },
        { "5", 26 }, { "6", 27 }, { "7", 28 }, { "8", 29 }, { "9", 30 },
        { "ID", 31 }
    };

        public static readonly Dictionary<int, string> TokenNames = TokenCodes.ToDictionary(kv => kv.Value, kv => kv.Key);
    }

}
