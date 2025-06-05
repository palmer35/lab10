using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Task0_1_Lexical;    // Для доступа к глобальному словарю кодов-токенов

namespace Task2_3_ParserSemantic
{

    /// <summary>
    /// Пара (Code, Lexeme). 
    /// Code   — строковый код токена (например, "31" для идентификатора, "22" для цифры '1' и т.д.).
    /// Lexeme — сам текст (например, "test", "a", "42"), либо null, если лексемы нет.
    /// </summary>
    public class Token
    {
        public string Code { get; }
        public string Lexeme { get; }
        public int Line { get; }

        public Token(string code, string lexeme, int line = 1)
        {
            Code = code;
            Lexeme = lexeme;
            Line = line;
        }

        public override string ToString()
        {
            return Lexeme == null ? Code : $"{Code}:{Lexeme}";
        }
    }


    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _pos = 0;
        private int _line = 1;

        // Алиасы кодов из Lexer.TokenCodes
        private const string TOK_PROGRAM = "1";   // "program"
        private const string TOK_VAR = "2";   // "var"
        private const string TOK_CONST = "3";   // "const"
        private const string TOK_BEGIN = "4";   // "begin"
        private const string TOK_END = "5";   // "end"
        private const string TOK_INTEGER = "6";   // "integer"
        private const string TOK_ARRAY = "7";   // "array"
        private const string TOK_OF = "8";   // "of"

        private const string TOK_ASSIGN = "9";   // ":="
        private const string TOK_EQ = "10";  // "="
        private const string TOK_PLUS = "11";  // "+"
        private const string TOK_MINUS = "12";  // "-"
        private const string TOK_MUL = "13";  // "*"
        private const string TOK_DIV = "14";  // "/"

        private const string TOK_LPAREN = "15";  // "("
        private const string TOK_RPAREN = "16";  // ")"
        private const string TOK_SEMI = "17";  // ";"
        private const string TOK_COMMA = "18";  // ","
        private const string TOK_DOT = "19";  // "."
        private const string TOK_COLON = "20";  // ":"

        // «21»..«30» — это коды однодигитных чисел ('0'..'9')
        private static readonly HashSet<string> DigitTokenCodes = new HashSet<string>
        {
            "21","22","23","24","25","26","27","28","29","30"
        };

        private const string TOK_ID = "31"; // «ID»

        // Для дебаг-логов: человекочитаемое имя каждого кода
        private static readonly Dictionary<string, string> TokenNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { TOK_PROGRAM, "program" },
            { TOK_VAR,     "var"     },
            { TOK_CONST,   "const"   },
            { TOK_BEGIN,   "begin"   },
            { TOK_END,     "end"     },
            { TOK_INTEGER, "integer" },
            { TOK_ARRAY,   "array"   },
            { TOK_OF,      "of"      },

            { TOK_ASSIGN,  ":="    },
            { TOK_EQ,      "="     },
            { TOK_PLUS,    "+"     },
            { TOK_MINUS,   "-"     },
            { TOK_MUL,     "*"     },
            { TOK_DIV,     "/"     },
            { TOK_LPAREN,  "("     },
            { TOK_RPAREN,  ")"     },
            { TOK_SEMI,    ";"     },
            { TOK_COMMA,   ","     },
            { TOK_DOT,     "."     },
            { TOK_COLON,   ":"     },

            { TOK_ID,      "ID"    }
        };

        public Parser(IEnumerable<Token> tokens)
        {
            // Конвертируем в List<Token> для удобства
            _tokens = tokens.ToList();
        }

        public ProgramNode ParseProgram()
        {
            Console.WriteLine("\nСинтаксический анализ:\n");
            Console.WriteLine($"[Debug] Tokens count: {_tokens.Count}");
            Console.WriteLine("[Debug] Start parsing program");

            var node = new ProgramNode();

            // === Разбор «program <ID> ;» ===
            Console.WriteLine("[Debug] Expect 'program'");
            if (!Expect(TOK_PROGRAM, "program"))
                SkipTo(new[] { TOK_ID, TOK_SEMI });

            Console.WriteLine("[Debug] Expect identifier after 'program'");
            if (!Expect(TOK_ID, "идентификатор"))
                SkipTo(new[] { TOK_SEMI });

            Console.WriteLine("[Debug] Expect ';' after program header");
            Expect(TOK_SEMI, ";");

            // === Блок «var ...» ===
            if (PeekToken()?.Code == TOK_VAR)
            {
                Console.WriteLine("[Debug] Found 'var' block");
                ReadToken(); // «2» = var

                while (PeekToken()?.Code == TOK_ID)
                {
                    var nameTok = ReadToken(); // «31» = ID
                    Console.WriteLine($"[Debug] Variable name: '{GetReadable(nameTok)}'");

                    Console.WriteLine("[Debug] Expect ':' before type");
                    if (!Expect(TOK_COLON, ":"))
                    {
                        SkipTo(new[] { TOK_SEMI });
                        ReadIf(TOK_SEMI);
                        continue;
                    }

                    if (PeekToken()?.Code == TOK_INTEGER)
                    {
                        var typeTok = ReadToken(); // «6» = integer
                        Console.WriteLine($"[Debug] Simple type: '{GetReadable(typeTok)}'");
                        node.Declarations.Add(new VarDeclNode
                        {
                            Name = nameTok.Lexeme,
                            Type = GetReadable(typeTok)
                        });
                    }
                    else if (PeekToken()?.Code == TOK_ARRAY)
                    {
                        ReadToken(); // «7» = array
                        Console.WriteLine("[Debug] Array declaration");
                        string typeText = "array";

                        Console.WriteLine("[Debug] Expect '['");
                        if (!Expect("[", "["))
                        {
                            SkipTo(new[] { TOK_SEMI });
                            ReadIf(TOK_SEMI);
                            continue;
                        }

                        // Нижняя граница массива (многозначный литерал)
                        if (TryCollectNumber(out int lower))
                        {
                            Console.WriteLine($"[Debug] Array lower bound: {lower}");
                            typeText += $"[{lower}";
                        }
                        else
                        {
                            ReportSyntaxError("число", PeekToken());
                            SkipTo(new[] { "..", "]", ";" });
                            if (PeekToken()?.Code == "..")
                                ReadToken();
                            if (TryCollectNumber(out lower))
                            {
                                Console.WriteLine($"[Debug] (recovered) Array lower bound: {lower}");
                                typeText += $"[{lower}";
                            }
                        }

                        Console.WriteLine("[Debug] Expect '..'");
                        if (!Expect("..", ".."))
                            SkipTo(new[] { "]", ";" });

                        // Верхняя граница массива
                        if (TryCollectNumber(out int upper))
                        {
                            Console.WriteLine($"[Debug] Array upper bound: {upper}");
                            typeText += $"..{upper}]";
                        }
                        else
                        {
                            ReportSyntaxError("число", PeekToken());
                            SkipTo(new[] { "]", ";" });
                            if (TryCollectNumber(out upper))
                            {
                                Console.WriteLine($"[Debug] (recovered) Array upper bound: {upper}");
                                typeText += $"..{upper}]";
                            }
                        }

                        Console.WriteLine("[Debug] Expect ']'");
                        if (!Expect("]", "]"))
                            SkipTo(new[] { TOK_OF, ";" });

                        Console.WriteLine("[Debug] Expect 'of'");
                        if (!Expect(TOK_OF, "of"))
                            SkipTo(new[] { TOK_INTEGER, ";" });

                        if (PeekToken()?.Code == TOK_INTEGER)
                        {
                            var baseTypeTok = ReadToken(); // «6» = integer
                            Console.WriteLine($"[Debug] Array base type: '{GetReadable(baseTypeTok)}'");
                            typeText += $" of {GetReadable(baseTypeTok)}";
                        }
                        else
                        {
                            ReportSyntaxError("integer", PeekToken());
                            SkipTo(new[] { ";" });
                        }

                        node.Declarations.Add(new VarDeclNode
                        {
                            Name = nameTok.Lexeme,
                            Type = typeText
                        });
                    }
                    else
                    {
                        ReportSyntaxError("тип (integer или array)", PeekToken());
                        SkipTo(new[] { TOK_SEMI });
                        ReadIf(TOK_SEMI);
                        continue;
                    }

                    Console.WriteLine("[Debug] Expect ';' after declaration");
                    Expect(TOK_SEMI, ";");
                }
            }

            // === Разбор «begin ... end.» ===
            Console.WriteLine("[Debug] Expect 'begin'");
            if (!Expect(TOK_BEGIN, "begin"))
                SkipTo(new[] { TOK_END, TOK_DOT });
            else
            {
                Console.WriteLine("[Debug] Entering begin-end block");
                while (PeekToken()?.Code != TOK_END && PeekToken() != null)
                {
                    var stmt = ParseStatement();
                    if (stmt != null)
                        node.Statements.Add(stmt);
                }

                Console.WriteLine("[Debug] Expect 'end'");
                Expect(TOK_END, "end");

                Console.WriteLine("[Debug] Expect '.' after end");
                Expect(TOK_DOT, ".");
            }

            Console.WriteLine("[Debug] Finished parsing program\n");
            return node;
        }

        private StatementNode ParseStatement()
        {
            if (PeekToken()?.Code == TOK_BEGIN)
            {
                Console.WriteLine("[Debug] Nested 'begin' found");
                ReadToken(); // «4» = begin
                var comp = new CompoundNode();
                while (PeekToken()?.Code != TOK_END && PeekToken() != null)
                {
                    var inner = ParseStatement();
                    if (inner != null)
                        comp.Statements.Add(inner);
                }
                Console.WriteLine("[Debug] Expect 'end' for nested");
                Expect(TOK_END, "end");
                Console.WriteLine("[Debug] Expect ';' after nested end");
                Expect(TOK_SEMI, ";");
                return comp;
            }

            if (PeekToken()?.Code == TOK_ID)
            {
                var nameTok = ReadToken(); // «31» = ID
                Console.WriteLine($"[Debug] Parsing assignment to '{GetReadable(nameTok)}'");

                ExpressionNode index = null;
                if (PeekToken()?.Code == "[")
                {
                    ReadToken(); // «[»
                    index = ParseExpression();
                    Console.WriteLine("[Debug] Expect ']' after index");
                    Expect("]", "]");
                }

                Console.WriteLine("[Debug] Expect ':='");
                if (!Expect(TOK_ASSIGN, ":="))
                {
                    SkipTo(new[] { TOK_SEMI });
                    ReadIf(TOK_SEMI);
                    return null;
                }

                var expr = ParseExpression();
                Console.WriteLine("[Debug] Expect ';' after assignment");
                Expect(TOK_SEMI, ";");

                return new AssignmentNode
                {
                    Variable = nameTok.Lexeme,
                    IndexExpression = index,
                    Expression = expr
                };
            }

            ReportError($"Ожидалось начало оператора, найдено '{GetReadable(PeekToken())}'");
            ReadToken();
            return null;
        }

        private ExpressionNode ParseExpression()
        {
            var left = ParseTerm();
            while (PeekToken()?.Code == TOK_PLUS || PeekToken()?.Code == TOK_MINUS)
            {
                var opTok = ReadToken();
                Console.WriteLine($"[Debug] Binary operator: '{GetReadable(opTok)}'");
                var right = ParseTerm();
                left = new BinaryExprNode
                {
                    Op = opTok.Lexeme,
                    Left = left,
                    Right = right
                };
            }
            return left;
        }

        private ExpressionNode ParseTerm()
        {
            if (PeekToken() != null && DigitTokenCodes.Contains(PeekToken().Code))
            {
                var sb = new StringBuilder();
                while (PeekToken() != null && DigitTokenCodes.Contains(PeekToken().Code))
                {
                    var digTok = PeekToken();
                    int digit = int.Parse(digTok.Code) - 21;
                    sb.Append(digit);
                    Console.WriteLine($"[Debug] Collect digit '{digit}' from token '{digTok.Code}'");
                    ReadToken();
                }
                string numStr = sb.ToString();
                if (int.TryParse(numStr, out int numValue))
                {
                    Console.WriteLine($"[Debug] Number literal: {numValue}");
                    return new NumberNode { Value = numValue };
                }
                else
                {
                    ReportError($"Невалидное число '{numStr}'");
                    return null;
                }
            }

            if (PeekToken()?.Code == TOK_ID)
            {
                var idTok = ReadToken();
                Console.WriteLine($"[Debug] Identifier: '{idTok.Lexeme}'");
                if (PeekToken()?.Code == "[")
                {
                    ReadToken(); // «[»
                    var idx = ParseExpression();
                    Expect("]", "]");
                    return new IdentifierNode { Name = $"{idTok.Lexeme}[{FormatIndex(idx)}]" };
                }
                return new IdentifierNode { Name = idTok.Lexeme };
            }

            ReportError($"Неожиданный токен в выражении: '{GetReadable(PeekToken())}'");
            ReadToken();
            return null;
        }

        private bool TryCollectNumber(out int value)
        {
            if (PeekToken() == null || !DigitTokenCodes.Contains(PeekToken().Code))
            {
                value = 0;
                return false;
            }

            var sb = new StringBuilder();
            while (PeekToken() != null && DigitTokenCodes.Contains(PeekToken().Code))
            {
                var digTok = PeekToken();
                int digit = int.Parse(digTok.Code) - 21;
                sb.Append(digit);
                Console.WriteLine($"[Debug] Collect digit '{digit}' from token '{digTok.Code}'");
                ReadToken();
            }

            string numStr = sb.ToString();
            if (int.TryParse(numStr, out int parsed))
            {
                value = parsed;
                return true;
            }
            else
            {
                value = 0;
                return false;
            }
        }

        private bool Expect(string expectedCode, string expectedName)
        {
            var tok = PeekToken();
            if (tok?.Code != expectedCode)
            {
                ReportSyntaxError(expectedName, tok);
                if (tok != null)
                {
                    Console.WriteLine($"[Debug] Read token in Expect (mismatch): '{tok.Code}' '{tok.Lexeme}'");
                    ReadToken();
                }
                return false;
            }
            ReadToken();
            return true;
        }

        private bool ReadIf(string code)
        {
            if (PeekToken()?.Code == code)
            {
                ReadToken();
                return true;
            }
            return false;
        }

        private void SkipTo(string[] syncSet)
        {
            Console.WriteLine($"[Debug] SkipTo synchronization tokens: {string.Join(", ", syncSet)}");
            while (PeekToken() != null && !syncSet.Contains(PeekToken().Code))
            {
                Console.WriteLine($"[Debug] Skipping token: '{PeekToken().Code}' '{PeekToken().Lexeme}'");
                ReadToken();
            }
        }

        private void ReportSyntaxError(string expected, Token actual)
        {
            string found = actual == null
                ? "EOF"
                : $"'{actual.Lexeme ?? actual.Code}' (код {actual.Code})";
            Console.WriteLine($"[Синтаксическая ошибка] Строка {_line}: ожидалось '{expected}', найдено {found}");
        }

        private void ReportError(string msg)
        {
            Console.WriteLine($"[Синтаксическая ошибка] Строка {_line}: {msg}");
        }

        private Token PeekToken() => _pos < _tokens.Count ? _tokens[_pos] : null;

        private Token ReadToken()
        {
            if (_pos >= _tokens.Count) return null;
            var tok = _tokens[_pos++];
            _line = tok.Line;
            Console.WriteLine($"[Debug] Read token: '{tok.Code}' '{tok.Lexeme}' (line {tok.Line}, pos {_pos})");
            return tok;
        }

        private string GetReadable(Token tok)
        {
            if (tok == null) return "EOF";
            if (!string.IsNullOrEmpty(tok.Lexeme))
                return tok.Lexeme;
            return TokenNames.TryGetValue(tok.Code, out var name)
                ? name
                : tok.Code;
        }

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
