using System.Text;

namespace Task2_Parser
{
    public class Lexer
    {
        private readonly string _text;
        private int _pos;
        private char Current => _pos < _text.Length ? _text[_pos] : '\0';

        private readonly HashSet<string> _keywords = new HashSet<string>
        {
            "program", "var", "const", "begin", "end", "integer", "array", "of"
        };

        public Lexer(string text)
        {
            _text = text;
            _pos = 0;
        }

        private void Advance() => _pos++;

        private void SkipWhitespace()
        {
            while (char.IsWhiteSpace(Current))
                Advance();
        }

        public Token GetNextToken()
        {
            SkipWhitespace();

            if (_pos >= _text.Length)
                return new Token(TokenType.EOF, "");

            // Идентификаторы или ключевые слова
            if (char.IsLetter(Current))
            {
                var sb = new StringBuilder();
                while (char.IsLetterOrDigit(Current))
                {
                    sb.Append(Current);
                    Advance();
                }
                var word = sb.ToString().ToLower();
                if (_keywords.Contains(word))
                    return new Token(TokenType.Keyword, word);
                return new Token(TokenType.Identifier, word);
            }

            // Числа (целые)
            if (char.IsDigit(Current))
            {
                var sb = new StringBuilder();
                while (char.IsDigit(Current))
                {
                    sb.Append(Current);
                    Advance();
                }
                return new Token(TokenType.Number, sb.ToString());
            }

            // Символы
            switch (Current)
            {
                case ':':
                    Advance();
                    if (Current == '=')
                    {
                        Advance();
                        return new Token(TokenType.Assign, ":=");
                    }
                    return new Token(TokenType.Colon, ":");
                case ';':
                    Advance();
                    return new Token(TokenType.Semicolon, ";");
                case ',':
                    Advance();
                    return new Token(TokenType.Comma, ",");
                case '[':
                    Advance();
                    return new Token(TokenType.LBracket, "[");
                case ']':
                    Advance();
                    return new Token(TokenType.RBracket, "]");
                case '.':
                    Advance();
                    return new Token(TokenType.Dot, ".");
            }

            // Неизвестный символ
            var unknown = Current.ToString();
            Advance();
            return new Token(TokenType.Unknown, unknown);
        }
    }
}
