using Ast;
namespace interpreter
{
    class RuntimeException(Token token, string message) : Exception(message)
    {
        public Token Token = token;
    }

    class Interpreter : Expr.IVisitor<object?>
    {
        public void Interpret(Expr expr)
        {
            try
            {
                object? value = Evaluate(expr);
                Console.WriteLine(Stringify(value));
            }
            catch (RuntimeException e)
            {
                Program.Error(e.Token.line, e.Message);
            }
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
    }
}