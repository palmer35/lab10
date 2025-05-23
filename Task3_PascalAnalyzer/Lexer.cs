using System.IO;

public enum TokenType { Identifier, Keyword, Number, Operator, EOF }

public class Token
{
    public TokenType Type;
    public string Value;

    public Token(TokenType type, string value)
    {
        Type = type;
        Value = value;
    }
}

public class Lexer
{
    private StreamReader reader;
    private int currentChar;

    private string[] keywords = { "program", "var", "begin", "end", "integer" };

    public Lexer(string filename)
    {
        reader = new StreamReader(filename);
        currentChar = reader.Read();
    }

    private void NextChar()
    {
        currentChar = reader.Read();
    }

    public Token GetToken()
    {
        while (currentChar != -1 && char.IsWhiteSpace((char)currentChar))
            NextChar();

        if (currentChar == -1)
            return new Token(TokenType.EOF, "");

        if (char.IsLetter((char)currentChar))
        {
            string word = "";
            while (currentChar != -1 && (char.IsLetterOrDigit((char)currentChar) || (char)currentChar == '_'))
            {
                word += (char)currentChar;
                NextChar();
            }
            if (System.Array.Exists(keywords, k => k == word.ToLower()))
                return new Token(TokenType.Keyword, word.ToLower());
            return new Token(TokenType.Identifier, word);
        }

        if (char.IsDigit((char)currentChar))
        {
            string num = "";
            while (currentChar != -1 && char.IsDigit((char)currentChar))
            {
                num += (char)currentChar;
                NextChar();
            }
            return new Token(TokenType.Number, num);
        }

        // Операторы и символы
        char ch = (char)currentChar;
        NextChar();
        return new Token(TokenType.Operator, ch.ToString());
    }
}
