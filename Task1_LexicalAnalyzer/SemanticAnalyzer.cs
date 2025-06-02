using System;
using System.Collections.Generic;
using static Task2_3_ParserSemantic.VarDeclNode;

namespace Task2_3_ParserSemantic
{
    /// <summary>
    /// Представляет информацию о типе переменной:
    /// - IsArray = false  => simpleType (“integer”)
    /// - IsArray = true   => массив с диапазоном [Lower..Upper] и элементарным типом BaseType
    /// </summary>
    public class TypeInfo
    {
        public bool IsArray { get; }
        public int Lower { get; }
        public int Upper { get; }
        public string BaseType { get; }  // всегда "integer" в текущем варианте

        public TypeInfo(string simpleType)
        {
            IsArray = false;
            BaseType = simpleType;
        }

        public TypeInfo(int lower, int upper, string baseType)
        {
            IsArray = true;
            Lower = lower;
            Upper = upper;
            BaseType = baseType;
        }

        public override string ToString()
        {
            return IsArray
                ? $"array[{Lower}..{Upper}] of {BaseType}"
                : BaseType;
        }
    }

    public class SemanticAnalyzer
    {
        // таблица символов: имя переменной → информация о типе
        private readonly Dictionary<string, TypeInfo> _symbolTable = new();
        private int _errorCount = 0;
        private const int MaxErrors = 2; // максимально выводим первые 2 ошибки

        public void Analyze(ProgramNode program)
        {
            Console.WriteLine("\nСемантический анализ:");

            // 1) Обработка объявлений
            foreach (var decl in program.Declarations)
            {
                if (_errorCount >= MaxErrors)
                    break;

                if (_symbolTable.ContainsKey(decl.Name))
                {
                    ReportError($"Повторное объявление переменной '{decl.Name}'");
                    continue;
                }

                // Разбираем строку decl.Type: либо "integer", либо "array[L..U] of integer"
                if (decl.Type.StartsWith("integer"))
                {
                    _symbolTable[decl.Name] = new TypeInfo("integer");
                    Console.WriteLine($"Переменная '{decl.Name}' объявлена как 'integer'");
                }
                else if (decl.Type.StartsWith("array"))
                {
                    // пример: "array[3..10] of integer"
                    try
                    {
                        // находим цифры между '[' и '..', а затем между '..' и ']'
                        var insideBrackets = decl.Type.Substring(
                            decl.Type.IndexOf('[') + 1,
                            decl.Type.IndexOf(']') - decl.Type.IndexOf('[') - 1
                        );
                        var parts = insideBrackets.Split("..", StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length != 2)
                            throw new Exception();

                        int lower = int.Parse(parts[0]);
                        int upper = int.Parse(parts[1]);

                        // проверяем, что дальше идёт " of integer"
                        var ofIndex = decl.Type.IndexOf("of", StringComparison.Ordinal);
                        if (ofIndex < 0)
                            throw new Exception();

                        var baseType = decl.Type.Substring(ofIndex + 2).Trim();
                        if (baseType != "integer")
                            throw new Exception();

                        _symbolTable[decl.Name] = new TypeInfo(lower, upper, "integer");
                        Console.WriteLine($"Переменная '{decl.Name}' объявлена как '{decl.Type}'");
                    }
                    catch
                    {
                        ReportError($"Некорректное описание массива для '{decl.Name}'");
                    }
                }
                else
                {
                    ReportError($"✖ Неизвестный тип для переменной '{decl.Name}'");
                }
            }

            // 2) Обработка операторов
            foreach (var stmt in program.Statements)
            {
                if (_errorCount >= MaxErrors)
                    break;

                AnalyzeStatement(stmt);
            }

            // 3) Итог
            if (_errorCount == 0)
                Console.WriteLine("Семантический анализ завершён успешно.");
            else
                Console.WriteLine($"Семантический анализ завершён с {_errorCount} ошибкой(ами).");
        }

        private void AnalyzeStatement(StatementNode stmt)
        {
            if (stmt is CompoundNode compound)
            {
                // рекурсивно разбираем вложенные операторы
                foreach (var inner in compound.Statements)
                {
                    if (_errorCount >= MaxErrors) break;
                    AnalyzeStatement(inner);
                }
            }
            else if (stmt is AssignmentNode assign)
            {
                // 1) Проверяем, есть ли переменная в таблице
                if (!_symbolTable.ContainsKey(assign.Variable))
                {
                    ReportError($"Переменная '{assign.Variable}' не объявлена");
                }
                else
                {
                    var typeInfo = _symbolTable[assign.Variable];

                    // 2) Индексирование: если IndexExpression != null, то переменная должна быть массивом
                    if (assign.IndexExpression != null)
                    {
                        if (!typeInfo.IsArray)
                        {
                            ReportError($"✖ Переменная '{assign.Variable}' не является массивом");
                        }
                        else
                        {
                            // Если индекс — константа, проверяем диапазон
                            if (_errorCount < MaxErrors && assign.IndexExpression is NumberNode numNode)
                            {
                                var idx = numNode.Value;
                                if (idx < typeInfo.Lower || idx > typeInfo.Upper)
                                {
                                    ReportError($"✖ Индекс {idx} выходит за границы массива '{assign.Variable}' [{typeInfo.Lower}..{typeInfo.Upper}]");
                                }
                            }
                        }
                    }
                    else
                    {
                        // Если переменная — массив, но без индексации — ошибка
                        if (typeInfo.IsArray)
                        {
                            ReportError($"✖ Нельзя присваивать элементу массива '{assign.Variable}' без индекса");
                        }
                    }
                }

                // 3) Проверяем выражение справа (если ещё не превысили лимит ошибок)
                if (_errorCount < MaxErrors)
                    AnalyzeExpression(assign.Expression);

                // 4) Проверяем индекс (если он есть и лимит ошибок не превышен)
                if (_errorCount < MaxErrors && assign.IndexExpression != null)
                    AnalyzeExpression(assign.IndexExpression);
            }
        }

        private void AnalyzeExpression(ExpressionNode expr)
        {
            if (_errorCount >= MaxErrors || expr == null)
                return;

            switch (expr)
            {
                case BinaryExprNode bin:
                    AnalyzeExpression(bin.Left);
                    AnalyzeExpression(bin.Right);
                    break;

                case IdentifierNode id:
                    // Может быть имя переменной; если содержит "[...]", проверим только имя до '['
                    var rawName = id.Name;
                    var varName = rawName.Contains("[")
                        ? rawName.Substring(0, rawName.IndexOf('['))
                        : rawName;

                    if (!_symbolTable.ContainsKey(varName))
                    {
                        ReportError($"✖ Идентификатор '{varName}' не объявлен");
                    }
                    break;

                case NumberNode _:
                    // литерал всегда корректен
                    break;

                default:
                    // другие узлы (если добавятся) пока игнорируем
                    break;
            }
        }

        private void ReportError(string msg)
        {
            if (_errorCount < MaxErrors)
            {
                Console.WriteLine(msg);
            }
            _errorCount++;
        }
    }
}
