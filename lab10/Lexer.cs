using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Task0_1_Lexical
{
    public class Lexer : IDisposable
    {
        private readonly string _filename;
        private readonly InputModule _input;
        private readonly Dictionary<string, int> _tokenCodes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
                { "program",  1 }, { "var", 2 }, { "const", 3 }, { "begin", 4 },
                { "end", 5 }, { "integer", 6 }, { "array", 7 }, { "of", 8 },

                { ":=", 9 }, { "=", 10 }, { "+", 11 }, { "-", 12 }, { "*", 13 }, { "/", 14 },
                { "(", 15 }, { ")", 16 }, { ";", 17 }, { ",", 18 }, { ".", 19 }, { ":", 20 },

                { "0", 21 }, { "1", 22 }, { "2", 23 }, { "3", 24 }, { "4", 25 },
                { "5", 26 }, { "6", 27 }, { "7", 28 }, { "8", 29 }, { "9", 30 },

                { "ID", 31 }

        };
        private const int PascalMaxInt = 32767;
        private const int PascalMinInt = -32768;
        private readonly StringBuilder _tokenBuilder = new StringBuilder();

        public Lexer(string filename)
        {
            _filename = filename;
            _input = new InputModule(filename);
        }

        public void GenerateTokenCodes(string outputFilename)
        {
            using var writer = new StreamWriter(outputFilename);
            string sourceCode = File.ReadAllText(_filename);
            int position = 0;
            char currentChar;
            string charAsString;
            string token;

            while (position < sourceCode.Length)
            {
                currentChar = sourceCode[position];

                if (char.IsWhiteSpace(currentChar))
                {
                    position++;
                    continue;
                }
                if (currentChar == ':' && position + 1 < sourceCode.Length && sourceCode[position + 1] == '=')
                {
                    writer.Write($"{_tokenCodes[":="]} ");
                    position += 2;
                    continue;
                }

                charAsString = currentChar.ToString();

                if (_tokenCodes.ContainsKey(charAsString)
                    && !char.IsLetterOrDigit(currentChar)
                    && currentChar != '_')
                {
                    writer.Write($"{_tokenCodes[charAsString]} ");
                    position++;
                    continue;
                }

                if (char.IsDigit(currentChar))
                {
                    writer.Write($"{_tokenCodes[charAsString]} ");
                    position++;
                    continue;
                }

                if (char.IsLetter(currentChar) || currentChar == '_')
                {
                    _tokenBuilder.Clear();
                    while (position < sourceCode.Length &&
                          (char.IsLetterOrDigit(sourceCode[position]) ||
                           sourceCode[position] == '_'))
                    {
                        _tokenBuilder.Append(sourceCode[position]);
                        position++;
                    }

                    token = _tokenBuilder.ToString();
                    if (_tokenCodes.TryGetValue(token, out int code))
                    {
                        writer.Write($"{code} ");
                    }
                    else
                    {
                        writer.Write($"{_tokenCodes["ID"]} ");
                    }
                    continue;
                }

                writer.Write("0 ");
                position++;
            }
        }

        public void AnalyzeSourceCode()
        {
            string[] sourceLines = File.ReadAllLines(_filename);
            int errorCount = 0;
            bool inBeginBlock = false;
            bool inMultiLineComment = false;
            List<Match> tokenMatches = new List<Match>();
            string token;

            Regex tokenRegex = new Regex(@"(:=)|[A-Za-z_][A-Za-z0-9_]*|\d+|[{}();:,+\-*/.=']|\S", RegexOptions.Compiled);
            Regex varDeclarationRegex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_,\s]*:\s*\w+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex assignmentRegex = new Regex(@"^\s*\w+\s*:=", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            for (int lineIndex = 0; lineIndex < sourceLines.Length; lineIndex++)
            {
                string currentLine = sourceLines[lineIndex];
                Console.WriteLine($"{lineIndex + 1,2} {currentLine}");

                bool inSingleQuote = false;
                bool inBraceComment = false;
                bool quoteErrorReported = false;
                bool braceErrorReported = false;

                tokenMatches.Clear();
                tokenMatches.AddRange(tokenRegex.Matches(currentLine).Cast<Match>());

                for (int tokenIndex = 0; tokenIndex < tokenMatches.Count; tokenIndex++)
                {
                    token = tokenMatches[tokenIndex].Value;

                    if (token.Equals("begin", StringComparison.OrdinalIgnoreCase))
                    {
                        inBeginBlock = true;
                    }

                    if (long.TryParse(token, out long numericValue))
                    {
                        if (numericValue > PascalMaxInt || numericValue < PascalMinInt)
                        {
                            errorCount++;
                            LogError(errorCount, 200, $"Строка {lineIndex + 1}",
                                    $"Число {token} выходит за диапазон {PascalMinInt}..{PascalMaxInt}");
                        }
                        continue;
                    }

                    if (token == "'")
                    {
                        inSingleQuote = !inSingleQuote;
                        continue;
                    }

                    if (token == "{")
                    {
                        inBraceComment = true;
                        continue;
                    }

                    if (token == "}")
                    {
                        inBraceComment = false;
                        continue;
                    }

                    if (inSingleQuote && !quoteErrorReported)
                    {
                        errorCount++;
                        quoteErrorReported = true;
                        LogError(errorCount, 208, $"Строка {lineIndex + 1}",
                                $"Незакрытая кавычка перед '{token}'");
                    }

                    if (inBraceComment && !braceErrorReported)
                    {
                        errorCount++;
                        braceErrorReported = true;
                        LogError(errorCount, 210, $"Строка {lineIndex + 1}",
                                $"Незакрытый комментарий перед '{token}'");
                    }

                    if (!inSingleQuote && !inBraceComment)
                    {
                        if (!_tokenCodes.ContainsKey(token.ToLower()) && !IsValidIdentifier(token))
                        {
                            errorCount++;
                            LogError(errorCount, 202, $"Строка {lineIndex + 1}",
                                    $"Недопустимый токен '{token}'");
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(currentLine))
                {
                    string trimmedLine = currentLine.TrimEnd();
                    bool requiresSemicolon = false;

                    if (varDeclarationRegex.IsMatch(trimmedLine))
                    {
                        requiresSemicolon = true;
                    }

                    else if (assignmentRegex.IsMatch(trimmedLine))
                    {
                        requiresSemicolon = true;
                    }

                    else if (trimmedLine.StartsWith("const", StringComparison.OrdinalIgnoreCase))
                    {
                        requiresSemicolon = true;
                    }

                    if (requiresSemicolon &&
                        !trimmedLine.EndsWith(";") &&
                        !trimmedLine.EndsWith("begin", StringComparison.OrdinalIgnoreCase))
                    {
                        errorCount++;
                        LogError(errorCount, 201, $"Строка {lineIndex + 1}",
                                "Отсутствует точка с запятой");
                    }
                }
            }

            Console.WriteLine($"\nКомпиляция завершена. Найдено ошибок: {errorCount}");
        }

        private bool IsValidIdentifier(string token)
        {
            return token.Length > 0 &&
                   char.IsLetter(token[0]) &&
                   token.All(c => char.IsLetterOrDigit(c) || c == '_');
        }

        private void LogError(int errorIndex, int errorCode, string location, string details)
        {
            string message = ErrorTable.GetMessage(errorCode);
            Console.WriteLine($"    **{errorIndex:00}** ^ Ошибка {errorCode} в {location}");
            Console.WriteLine($"    ****** {message}: {details}");
        }

        public void Dispose()
        {
            _input?.Dispose();
            _tokenBuilder.Clear();
        }
    }
}