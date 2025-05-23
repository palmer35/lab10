namespace Task2_Parser
{
    public enum TokenType
    {
        EOF,
        Identifier,
        Number,
        Keyword,
        Symbol,
        Assign,      // :=
        Semicolon,   // ;
        Colon,       // :
        Comma,       // ,
        LBracket,    // [
        RBracket,    // ]
        Dot,         // .
        Unknown,
        Of
    }

    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }

        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString() => $"{Type}: '{Value}'";
    }
}
