using System;
using System.Collections.Generic;
using System.Linq;
using Task0_1_Lexical;
using static Task2_3_ParserSemantic.VarDeclNode;

namespace Task2_3_ParserSemantic
{
    public class Parser
    {
        private readonly List<string> _tokens;
        private int _pos = 0;
        private int _line = 1;

        private readonly Dictionary<string, string> tokenNames = new()
        {
            { "1", "program" }, { "2", "var" }, { "3", "const" }, { "4", "begin" }, { "5", "end" },
            { "6", "integer" }, { "7", "array" }, { "ID", "идентификатор" }, { ":=", ":=" },
            { ";", ";" }, { ":", ":" }, { ".", "." }, { "[", "[" }, { "]", "]" }, { "+", "+" }, { "-", "-" },
            { "..", ".." }, { "of", "of" }
        };

        public Parser(IEnumerable<string> tokens)
        {
            _tokens = tokens.ToList();
        }

        public ProgramNode ParseProgram()
        {
            var node = new ProgramNode();

            // program <ID> ;
            if (!Expect("1", "program")) SkipTo(new[] { "ID", ";" });
            if (!Expect("ID", "идентификатор")) SkipTo(new[] { ";" });
            Expect(";", ";");

            // var блок
            if (Peek() == "2") // var
            {
                Read(); // пропустить "var"
                while (Peek() == "ID")
                {
                    var name = Read(); // ID

                    if (!Expect(":", ":")) { SkipTo(new[] { ";" }); ReadIf(";"); continue; }

                    // Разбор типа: integer или array [ <число> .. <число> ] of integer
                    if (Peek() == "6")
                    {
                        // простые переменные
                        var typeText = GetReadable(Read()); // "integer"
                        node.Declarations.Add(new VarDeclNode { Name = name, Type = typeText });
                    }
                    else if (Peek() == "7")
                    {
                        Read(); // "array"
                        var typeText = "array";

                        if (!Expect("[", "[")) { SkipTo(new[] { ";" }); ReadIf(";"); continue; }

                        // нижняя граница
                        if (int.TryParse(Peek(), out var lower))
                        {
                            typeText += $"[{lower}";
                            Read();
                        }
                        else
                        {
                            ReportSyntaxError("число");
                            SkipTo(new[] { "..", "]", ";" });
                            if (Peek() == "..") { Read(); }
                            if (int.TryParse(Peek(), out lower))
                            {
                                typeText += $"[{lower}";
                                Read();
                            }
                        }

                        if (!Expect("..", "..")) { SkipTo(new[] { "]", ";" }); }

                        // верхняя граница
                        if (int.TryParse(Peek(), out var upper))
                        {
                            typeText += $"..{upper}]";
                            Read();
                        }
                        else
                        {
                            ReportSyntaxError("число");
                            SkipTo(new[] { "]", ";" });
                            if (int.TryParse(Peek(), out upper))
                            {
                                typeText += $"..{upper}]";
                                Read();
                            }
                        }

                        if (!Expect("]", "]")) { SkipTo(new[] { "of", ";" }); }

                        if (!Expect("of", "of")) { SkipTo(new[] { "6", ";" }); }

                        if (Peek() == "6")
                        {
                            var baseType = GetReadable(Read()); // "integer"
                            typeText += $" of {baseType}";
                        }
                        else
                        {
                            ReportSyntaxError("integer");
                            SkipTo(new[] { ";" });
                        }

                        node.Declarations.Add(new VarDeclNode { Name = name, Type = typeText });
                    }
                    else
                    {
                        ReportSyntaxError("тип (integer или array)");
                        SkipTo(new[] { ";" });
                        ReadIf(";");
                        continue;
                    }

                    Expect(";", ";");
                }
            }

            // begin ... end.
            if (!Expect("4", "begin"))
            {
                SkipTo(new[] { "5", "." });
            }
            else
            {
                // разобрали "begin"
                while (Peek() != "5" && Peek() != null) // пока не "end"
                {
                    var stmt = ParseStatement();
                    if (stmt != null)
                        node.Statements.Add(stmt);
                }

                Expect("5", "end");
                Expect(".", ".");
            }

            return node;
        }

        private StatementNode ParseStatement()
        {
            if (Peek() == "4") // вложенный begin
            {
                Read(); // "begin"
                var comp = new CompoundNode();
                while (Peek() != "5" && Peek() != null)
                {
                    var inner = ParseStatement();
                    if (inner != null)
                        comp.Statements.Add(inner);
                }
                Expect("5", "end");
                Expect(";", ";"); // точку с запятой после end; (если нет — сообщаем ошибку, но продолжаем)
                return comp;
            }

            if (Peek() == "ID")
            {
                var varName = Read(); // имя переменной
                ExpressionNode index = null;

                if (Peek() == "[")
                {
                    Read(); // "["
                    index = ParseExpression();
                    Expect("]", "]");
                }

                if (!Expect(":=", ":="))
                {
                    SkipTo(new[] { ";" });
                    ReadIf(";");
                    return null;
                }

                var expr = ParseExpression();

                Expect(";", ";");

                return new AssignmentNode
                {
                    Variable = varName,
                    IndexExpression = index,
                    Expression = expr
                };
            }

            ReportError($"Ожидалось начало оператора, найдено '{GetReadable(Peek())}'");
            Read(); // пропустить неверный токен
            return null;
        }

        private ExpressionNode ParseExpression()
        {
            var left = ParseTerm();
            while (Peek() == "+" || Peek() == "-")
            {
                var op = Read();
                var right = ParseTerm();
                left = new BinaryExprNode { Op = op, Left = left, Right = right };
            }
            return left;
        }

        private ExpressionNode ParseTerm()
        {
            if (int.TryParse(Peek(), out var num))
            {
                Read();
                return new NumberNode { Value = num };
            }

            if (Peek() == "ID")
            {
                var name = Read();
                if (Peek() == "[")
                {
                    Read(); // "["
                    var idx = ParseExpression();
                    Expect("]", "]");
                    return new IdentifierNode { Name = $"{name}[{FormatIndex(idx)}]" };
                }
                return new IdentifierNode { Name = name };
            }

            ReportError($"Неожиданный токен в выражении: '{GetReadable(Peek())}'");
            Read();
            return null;
        }

        private string Peek() => _pos < _tokens.Count ? _tokens[_pos] : null;

        private string Read()
        {
            if (_pos >= _tokens.Count) return null;
            var tok = _tokens[_pos++];
            if (tok == "EOL") _line++;
            return tok;
        }

        /// <summary>
        /// Проверяет, что Peek() равен expectedCode; если нет, вызывает ReportSyntaxError,
        /// затем читает текущий токен (если он не null). Возвращает true, если ожидание совпало.
        /// </summary>
        private bool Expect(string expectedCode, string expectedName)
        {
            if (Peek() != expectedCode)
            {
                ReportSyntaxError(expectedName);
                if (Peek() != null)
                    Read();
                return false;
            }
            Read();
            return true;
        }

        /// <summary>
        /// Если Peek() равен токену code, читает его и возвращает true; иначе возвращает false.
        /// </summary>
        private bool ReadIf(string code)
        {
            if (Peek() == code)
            {
                Read();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Пропускает токены до тех пор, пока не встретит любой из синхронизирующих токенов из syncSet
        /// или не дойдёт до конца. Затем останавливается перед этим токеном.
        /// </summary>
        private void SkipTo(string[] syncSet)
        {
            while (Peek() != null && !syncSet.Contains(Peek()))
            {
                Read();
            }
        }

        private void ReportSyntaxError(string expected)
        {
            Console.WriteLine($"[Синтаксическая ошибка] Строка {_line}: Ожидалось '{expected}', найдено '{GetReadable(Peek())}'");
        }

        private void ReportError(string msg)
        {
            Console.WriteLine($"[Синтаксическая ошибка] Строка {_line}: {msg}");
        }

        private string GetReadable(string token)
        {
            if (token == null) return "EOF";
            return tokenNames.TryGetValue(token, out var name) ? name : token;
        }

        /// <summary>
        /// Вспомогательный метод для формирования текстового представления индексированного идентификатора.
        /// </summary>
        private string FormatIndex(ExpressionNode idx)
        {
            return idx switch
            {
                NumberNode n => n.Value.ToString(),
                IdentifierNode id => id.Name,
                BinaryExprNode b => $"({FormatIndex(b.Left)} {b.Op} {FormatIndex(b.Right)})",
                _ => ""
            };
        }


    }
}
