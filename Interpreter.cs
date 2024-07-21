using System.Collections;
using Ast;
namespace interpreter
{
    class RuntimeException(Token token, string message) : Exception(message)
    {
        public Token Token = token;
    }

    class Interpreter : Expr.IVisitor<object?>, Stmt.IVisitor<Option<object?>>
    {
        private Environment Env = new();

        public Option<object?> Interpret(List<Stmt?> statements)
        {
            try
            {
                Option<object?> value = None.Value;
                foreach (Stmt? statement in statements)
                {
                    if (statement == null) continue;
                    value = Execute(statement!);
                }
                return value;
            }
            catch (RuntimeException e)
            {
                Program.Error(e.Token.line, e.Message);
            }
            return None.Value;
        }

        private Option<object?> Execute(Stmt statement)
        {
            return statement.Accept(this);
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

        public Option<object?> Visit(Stmt.Expression stmt)
        {
            return Evaluate(stmt.expression);
        }

        public Option<object?> Visit(Stmt.Print stmt)
        {
            object? value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return None.Value;
        }

        public object? Visit(Expr.Variable expr)
        {
            return Env.Get(expr.name);
        }

        public Option<object?> Visit(Stmt.Var stmt)
        {
            object? value = null;
            if (stmt.initializer != null)
            {
                value = Evaluate(stmt.initializer);
            }
            Env.Define(stmt.name.lexeme, value);
            return None.Value;
        }

        public object? Visit(Expr.Assign expr)
        {
            object? value = Evaluate(expr.value);
            Env.Assign(expr.name, value);
            return value;
        }

        public Option<object?> Visit(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new Environment(Env));
            return None.Value;
        }

        public void ExecuteBlock(List<Stmt> statements, Environment env)
        {
            Environment previous = Env;
            try
            {
                Env = env;
                foreach (Stmt stmt in statements)
                {
                    Execute(stmt);
                }
            }
            finally
            {
                Env = previous;
            }
        }

        public Option<object?> Visit(Stmt.If stmt)
        {
            if (IsTruhty(Evaluate(stmt.condition)))
            {
                Execute(stmt.then_branch);
            }
            else if (stmt.else_branch != null)
            {
                Execute(stmt.else_branch);
            }
            return None.Value;
        }

        public object? Visit(Expr.Logical expr)
        {
            object? left = Evaluate(expr.left);
            if (expr.op.type == TokenType.OR)
            {
                if (IsTruhty(left)) return left;
            }
            else
            {
                if (!IsTruhty(left)) return left;
            }
            return Evaluate(expr.right);
        }
    }
}