namespace Task2_3_ParserSemantic
{
    /// <summary>
    /// Базовый класс для всех узлов AST.
    /// </summary>
    public abstract class AstNode { }

    /// <summary>
    /// Узел, соответствующий программе в целом:
    /// содержит список объявлений переменных и список операторов.
    /// </summary>
    public class ProgramNode : AstNode
    {
        public List<VarDeclNode> Declarations { get; set; } = new();
        public List<StatementNode> Statements { get; set; } = new();
    }

    /// <summary>
    /// Описание одной переменной в разделе var.
    /// Поле Type хранит описание типа в текстовом виде:
    /// - если простая: "integer"
    /// - если массив: "array[<lower>..<upper>] of integer"
    /// </summary>
    public class VarDeclNode : AstNode
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    /// <summary>
    /// Базовый абстрактный класс для всех операторов (Statement).
    /// </summary>
    public abstract class StatementNode : AstNode { }

    /// <summary>
    /// Оператор присваивания:
    /// - Variable содержит имя переменной (без индекса).
    /// - IndexExpression != null ↔ это массивный элемент.
    /// - Expression — узел, описывающий выражение справа от :=.
    /// </summary>
    public class AssignmentNode : StatementNode
    {
        public string Variable { get; set; }
        public ExpressionNode IndexExpression { get; set; } // если индексированная переменная, иначе null
        public ExpressionNode Expression { get; set; }
    }

    /// <summary>
    /// Составной оператор (compound): begin … end;
    /// Содержит вложенный список StatementNode.
    /// </summary>
    public class CompoundNode : StatementNode
    {
        public List<StatementNode> Statements { get; set; } = new();
    }

    /// <summary>
    /// Базовый абстрактный класс для всех выражений.
    /// </summary>
    public abstract class ExpressionNode : AstNode { }

    /// <summary>
    /// Двоичное выражение: Left Op Right, где Op = "+" или "-".
    /// </summary>
    public class BinaryExprNode : ExpressionNode
    {
        public string Op { get; set; }
        public ExpressionNode Left { get; set; }
        public ExpressionNode Right { get; set; }
    }

    /// <summary>
    /// Числовая константа.
    /// </summary>
    public class NumberNode : ExpressionNode
    {
        public int Value { get; set; }
    }

    /// <summary>
    /// Идентификатор простой переменной (без индекса).
    /// </summary>
    public class IdentifierNode : ExpressionNode
    {
        public string Name { get; set; }
    }

    /// <summary>
    /// Обращение к элементу массива: ArrayName[ IndexExpression ].
    /// </summary>
    public class ArrayAccessNode : ExpressionNode
    {
        public string ArrayName { get; set; }
        public ExpressionNode IndexExpression { get; set; }
    }
}
