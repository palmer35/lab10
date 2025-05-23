using System.Collections.Generic;

public class Parser
{
    private Lexer lexer;
    private Token currentToken;
    public List<string> Errors = new List<string>();

    private HashSet<string> variables = new HashSet<string>();

    public Parser(Lexer lexer)
    {
        this.lexer = lexer;
        currentToken = lexer.GetToken();
    }

    private void NextToken()
    {
        currentToken = lexer.GetToken();
    }

    private void Error(string message)
    {
        Errors.Add("Ошибка: " + message + " (токен: '" + currentToken.Value + "')");
        NextToken(); // Пытаемся двигаться дальше
    }

    public void ParseProgram()
    {
        if (currentToken.Type == TokenType.Keyword && currentToken.Value == "program")
        {
            NextToken();
            if (currentToken.Type == TokenType.Identifier)
            {
                NextToken();
                if (currentToken.Type == TokenType.Operator && currentToken.Value == ";")
                {
                    NextToken();
                    ParseBlock();
                }
                else Error("Ожидался символ ';' после имени программы");
            }
            else Error("Ожидалось имя программы");
        }
        else Error("Ожидалось ключевое слово 'program'");
    }

    private void ParseBlock()
    {
        if (currentToken.Type == TokenType.Keyword && currentToken.Value == "var")
        {
            ParseVarSection();
        }

        if (currentToken.Type == TokenType.Keyword && currentToken.Value == "begin")
        {
            NextToken();
            ParseStatements();

            if (currentToken.Type == TokenType.Keyword && currentToken.Value == "end")
            {
                NextToken();
                if (!(currentToken.Type == TokenType.Operator && currentToken.Value == "."))
                    Error("Ожидался символ '.' после 'end'");
            }
            else Error("Ожидалось ключевое слово 'end'");
        }
        else Error("Ожидалось ключевое слово 'begin'");
    }

    private void ParseVarSection()
    {
        NextToken(); // пропускаем var

        while (currentToken.Type == TokenType.Identifier)
        {
            string varName = currentToken.Value;
            NextToken();

            if (currentToken.Type == TokenType.Operator && currentToken.Value == ":")
            {
                NextToken();
                if (currentToken.Type == TokenType.Keyword && currentToken.Value == "integer")
                {
                    variables.Add(varName);
                    NextToken();

                    if (currentToken.Type == TokenType.Operator && currentToken.Value == ";")
                    {
                        NextToken();
                    }
                    else
                    {
                        Error("Ожидался символ ';' после объявления переменной");
                    }
                }
                else
                {
                    Error("Ожидался тип 'integer'");
                }
            }
            else
            {
                Error("Ожидался символ ':' после имени переменной");
            }
        }
    }

    private void ParseStatements()
    {
        while (currentToken.Type == TokenType.Identifier)
        {
            ParseAssignment();

            if (currentToken.Type == TokenType.Operator && currentToken.Value == ";")
                NextToken();
            else
                Error("Ожидался символ ';' после оператора");
        }
    }

    private void ParseAssignment()
    {
        string varName = currentToken.Value;
        if (!variables.Contains(varName))
            Error($"Переменная '{varName}' не объявлена");

        NextToken();

        if (currentToken.Type == TokenType.Operator && currentToken.Value == ":")
        {
            NextToken();
            if (currentToken.Type == TokenType.Operator && currentToken.Value == "=")
            {
                NextToken();
                ParseExpression();
            }
            else Error("Ожидался '=' после ':' в операторе присваивания");
        }
        else
        {
            Error("Ожидался оператор ':=' для присваивания");
        }
    }

    private void ParseExpression()
    {
        if (currentToken.Type == TokenType.Number || currentToken.Type == TokenType.Identifier)
        {
            NextToken();
        }
        else
        {
            Error("Ожидалось число или идентификатор в выражении");
        }
    }
}
