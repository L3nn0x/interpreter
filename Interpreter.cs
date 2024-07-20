using Ast;
namespace interpreter
{
    class RuntimeException(Token token, string message) : Exception(message)
    {
        public Token Token = token;
    }

    public readonly struct Void : IEquatable<Void>
    {
        public static readonly Void unit;
        public override readonly bool Equals(object? obj) => obj is Void;
        public override readonly int GetHashCode() => 0;
        public static bool operator ==(Void left, Void right) => left.Equals(right);
        public static bool operator !=(Void left, Void right) => !(left == right);
        public readonly bool Equals(Void other) => true;
        public override readonly string ToString() => "()";
    }

    class Interpreter : Expr.IVisitor<object?>, Stmt.IVisitor<Void>
    {
        private Environment Env = new();

        public void Interpret(List<Stmt?> statements)
        {
            try
            {
                foreach (Stmt? statement in statements)
                {
                    if (statement == null) continue;
                    Execute(statement!);
                }
            }
            catch (RuntimeException e)
            {
                Program.Error(e.Token.line, e.Message);
            }
        }

        private void Execute(Stmt statement)
        {
            statement.Accept(this);
        }

        private static string Stringify(object? value)
        {
            if (value == null) return "nil";
            if (value!.GetType() == typeof(Double))
            {
                string text = value!.ToString()!;
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }
            return value!.ToString()!;
        }

        private object? Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        public object? Visit(Expr.Binary expr)
        {
            object? left = Evaluate(expr.Left);
            object? right = Evaluate(expr.Right);

            switch (expr.Op.type)
            {
                case TokenType.MINUS:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left! - (double)right!;
                case TokenType.SLASH:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left! / (double)right!;
                case TokenType.STAR:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left! * (double)right!;
                case TokenType.PLUS:
                    if (left!.GetType() == typeof(double) && right!.GetType() == typeof(double))
                    {
                        return (double)left! + (double)right!;
                    }
                    if (left!.GetType() == typeof(string) && right!.GetType() == typeof(string))
                    {
                        return (string)left! + (string)right!;
                    }
                    throw new RuntimeException(expr.Op, "Operands must be numbers or strings");
                case TokenType.GREATER:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left! > (double)right!;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left! >= (double)right!;
                case TokenType.LESS:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left! < (double)right!;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left! <= (double)right!;
                case TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL:
                    return IsEqual(left, right);
            }
            return null;
        }

        private static bool IsEqual(object? left, object? right)
        {
            if (left == null && right == null) return true;
            if (left == null || right == null) return false;
            return left.Equals(right);
        }

        public object? Visit(Expr.Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        public object? Visit(Expr.Literal expr)
        {
            return expr.Value;
        }

        public object? Visit(Expr.Unary expr)
        {
            object? right = Evaluate(expr.Right);

            switch (expr.Op.type)
            {
                case TokenType.MINUS:
                    CheckNumberOperand(expr.Op, right);
                    return -(double?)right;
                case TokenType.BANG: return !IsTruhty(right);
            }
            return null;
        }

        public static bool IsTruhty(object? value)
        {
            if (value == null) return false;
            if (value!.GetType() == typeof(bool)) return (bool)value!;
            if (value!.GetType() == typeof(double)) return ((double)value!) != 0;
            return true;
        }

        public static void CheckNumberOperand(Token op, object? operand)
        {
            if (operand != null && operand!.GetType() == typeof(Double))
            {
                return;
            }
            throw new RuntimeException(op, "Operand must be a number");
        }

        public static void CheckNumberOperands(Token op, object? left, object? right)
        {
            if (left != null && right != null && left!.GetType() == typeof(Double) && right!.GetType() == typeof(Double))
            {
                return;
            }
            throw new RuntimeException(op, "Operands must be numbers");
        }

        public Void Visit(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);
            return Void.unit;
        }

        public Void Visit(Stmt.Print stmt)
        {
            object? value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return Void.unit;
        }

        public object? Visit(Expr.Variable expr)
        {
            return Env.Get(expr.name);
        }

        public Void Visit(Stmt.Var stmt)
        {
            object? value = null;
            if (stmt.initializer != null) {
                value = Evaluate(stmt.initializer);
            }
            Env.Define(stmt.name.lexeme, value);
            return Void.unit;
        }
    }
}