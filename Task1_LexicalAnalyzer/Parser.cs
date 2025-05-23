using System;
using System.Collections.Generic;

namespace Task2_Parser
{
    public class Parser
    {
        private readonly Lexer _lexer;
        private Token _currentToken;

        private List<string> _errors = new List<string>();

        public Parser(Lexer lexer)
        {
            _lexer = lexer;
            _currentToken = _lexer.GetNextToken();
        }

        private void Error(string message)
        {
            string fullMessage = $"Ошибка: {message} на токене {_currentToken}";
            _errors.Add(fullMessage);
            Console.WriteLine(fullMessage); // ← Добавь это, чтобы видеть сразу
            _currentToken = _lexer.GetNextToken();
        }

        private void Eat(TokenType type)
        {
            if (_currentToken.Type == type)
            {
                _currentToken = _lexer.GetNextToken();
            }
            else
            {
                Error($"Ожидался токен типа {type}");
            }
        }

        public void Parse()
        {
            // Примерно так:
            while (_currentToken.Type != TokenType.EOF)
            {
                if (_currentToken.Type == TokenType.Keyword)
                {
                    switch (_currentToken.Value)
                    {
                        case "program":
                            ParseProgramHeader();
                            break;
                        case "const":
                            ParseConstBlock();
                            break;
                        case "var":
                            ParseVariableDeclaration();
                            break;
                        case "begin":
                            ParseCompoundStatement();
                            break;
                        default:
                            // Нейтрализация для неизвестных ключевых слов
                            Error($"Неожиданное ключевое слово '{_currentToken.Value}'");
                            break;
                    }
                }
                else
                {
                    Error("Неожиданный токен, ожидалось ключевое слово");
                }
            }
        }

        private void ParseProgramHeader()
        {
            Eat(TokenType.Keyword); // program

            if (_currentToken.Type == TokenType.Identifier)
            {
                Eat(TokenType.Identifier);
            }
            else
            {
                Error("Ожидался идентификатор после 'program'");
            }

            if (_currentToken.Type == TokenType.Semicolon)
            {
                Eat(TokenType.Semicolon);
            }
            else
            {
                Error("Ожидался ';' после заголовка программы");
            }
        }

        private void ParseConstBlock()
        {
            Eat(TokenType.Keyword); // const

            // Пример простой нейтрализации: пропускаем все до var или begin или EOF
            while (_currentToken.Type != TokenType.Keyword ||
                  (_currentToken.Value != "var" && _currentToken.Value != "begin"))
            {
                if (_currentToken.Type == TokenType.EOF)
                    break;
                _currentToken = _lexer.GetNextToken();
            }
        }

        private void ParseVariableDeclaration()
        {
            // var <id> {, <id>} : <type> ;
            Eat(TokenType.Keyword); // var

            while (_currentToken.Type == TokenType.Identifier)
            {
                List<string> identifiers = new List<string>();
                identifiers.Add(_currentToken.Value);
                Eat(TokenType.Identifier);

                while (_currentToken.Type == TokenType.Comma)
                {
                    Eat(TokenType.Comma);
                    if (_currentToken.Type == TokenType.Identifier)
                    {
                        identifiers.Add(_currentToken.Value);
                        Eat(TokenType.Identifier);
                    }
                    else
                    {
                        Error("Ожидался идентификатор после запятой");
                        break;
                    }
                }

                if (_currentToken.Type == TokenType.Colon)
                {
                    Eat(TokenType.Colon);
                    ParseType();
                }
                else
                {
                    Error("Ожидался ':' после списка идентификаторов");
                }

                if (_currentToken.Type == TokenType.Semicolon)
                {
                    Eat(TokenType.Semicolon);
                }
                else
                {
                    Error("Ожидался ';' после объявления переменных");
                    // Нейтрализация: пропускаем до ';' или следующего ключевого слова
                    while (_currentToken.Type != TokenType.Semicolon && _currentToken.Type != TokenType.Keyword && _currentToken.Type != TokenType.EOF)
                    {
                        _currentToken = _lexer.GetNextToken();
                    }
                    if (_currentToken.Type == TokenType.Semicolon)
                        Eat(TokenType.Semicolon);
                }

                if (_currentToken.Type != TokenType.Identifier)
                    break; // Нет больше переменных
            }
        }

        private void ParseType()
        {
            if (_currentToken.Type == TokenType.Keyword && _currentToken.Value == "integer")
            {
                Eat(TokenType.Keyword);
            }
            else if (_currentToken.Type == TokenType.Keyword && _currentToken.Value == "array")
            {
                Eat(TokenType.Keyword); // array
                Eat(TokenType.Of); // "of" — но мы не добавили TokenType.Of, потому что он в ключевых словах. Добавим!
                if (_currentToken.Type == TokenType.Keyword && _currentToken.Value == "of")
                {
                    Eat(TokenType.Keyword);
                }
                else
                {
                    Error("Ожидалось ключевое слово 'of' после 'array'");
                }
                if (_currentToken.Type == TokenType.Keyword && _currentToken.Value == "integer")
                {
                    Eat(TokenType.Keyword);
                }
                else
                {
                    Error("Ожидался тип 'integer' после 'array of'");
                }
            }
            else
            {
                Error("Ожидался тип 'integer' или 'array of integer'");
            }
        }

        private void ParseCompoundStatement()
        {
            // begin <statements> end
            Eat(TokenType.Keyword); // begin

            while (_currentToken.Type != TokenType.Keyword || _currentToken.Value != "end")
            {
                if (_currentToken.Type == TokenType.Identifier)
                {
                    ParseAssignmentStatement();
                }
                else
                {
                    Error("Ожидался идентификатор или 'end' в составном операторе");
                    if (_currentToken.Type == TokenType.EOF)
                        break;
                    else
                        _currentToken = _lexer.GetNextToken();
                }
            }

            if (_currentToken.Type == TokenType.Keyword && _currentToken.Value == "end")
            {
                Eat(TokenType.Keyword);
                if (_currentToken.Type == TokenType.Dot)
                    Eat(TokenType.Dot);
                else
                    Error("Ожидалась '.' после 'end'");
            }
            else
            {
                Error("Ожидался 'end' в составном операторе");
            }
        }

        private void ParseAssignmentStatement()
        {
            // <id> := <expression> ;
            Eat(TokenType.Identifier);

            if (_currentToken.Type == TokenType.Assign)
            {
                Eat(TokenType.Assign);
            }
            else
            {
                Error("Ожидался ':=' в операторе присваивания");
            }

            ParseExpression();

            if (_currentToken.Type == TokenType.Semicolon)
            {
                Eat(TokenType.Semicolon);
            }
            else
            {
                Error("Ожидался ';' после оператора присваивания");
            }
        }

        private void ParseExpression()
        {
            // Очень упрощённо: принимаем только числа или идентификаторы с операторами +, -, *, /
            ParseTerm();

            while (_currentToken.Type == TokenType.Symbol && ("+-*/".Contains(_currentToken.Value)))
            {
                Eat(TokenType.Symbol);
                ParseTerm();
            }
        }

        private void ParseTerm()
        {
            if (_currentToken.Type == TokenType.Number || _currentToken.Type == TokenType.Identifier)
            {
                Eat(_currentToken.Type);
            }
            else
            {
                Error("Ожидался идентификатор или число в выражении");
            }
        }

        public IReadOnlyList<string> GetErrors() => _errors;
    }
}
