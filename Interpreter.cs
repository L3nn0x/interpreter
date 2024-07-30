using System.Collections;
using Ast;
namespace interpreter
{
    class RuntimeException(Token token, string message) : Exception(message)
    {
        public Token Token = token;
    }

    class Return(Option value) : Exception
    {
        public readonly Option Value = value;
    }

    class Interpreter : Expr.IVisitor<Option>, Stmt.IVisitor<Option>
    {
        private Environment Globals = new();
        private Environment Env;

        public Interpreter()
        {
            Globals.Define("clock", new Clock());
            Globals.Define("input", new Input());
            Env = new(Globals);
        }

        public Option Interpret(List<Stmt?> statements)
        {
            try
            {
                Option value = None.Value;
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

        private Option Execute(Stmt statement)
        {
            return statement.Accept(this);
        }

        private static string Stringify(Option value)
        {
            if (value is None) return "";
            Some v = (Some)value;
            if (v.Content == null) return "nil";
            if (v.Content.GetType() == typeof(Double))
            {
                string text = v.Content!.ToString()!;
                if (text.EndsWith(".0"))
                {
                    text = text[..^2];
                }
                return text;
            }
            return v.Content!.ToString()!;
        }

        private Option Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        public Option Visit(Expr.Binary expr)
        {
            Option left = Evaluate(expr.Left);
            Option right = Evaluate(expr.Right);

            U apply<T, U>(Func<T, T, U> fn) => fn((T)((Some)left).Content!, (T)((Some)right).Content!);

            switch (expr.Op.type)
            {
                case TokenType.MINUS:
                    CheckNumberOperands(expr.Op, left, right);
                    return new Some(apply<double, double>((a, b) => a - b));
                case TokenType.SLASH:
                    CheckNumberOperands(expr.Op, left, right);
                    return new Some(apply<double, double>((a, b) => a / b));
                case TokenType.STAR:
                    CheckNumberOperands(expr.Op, left, right);
                    return new Some(apply<double, double>((a, b) => a * b));
                case TokenType.PLUS:
                    if (left is Some sleft && right is Some sright)
                    {
                        if (sleft.Content!.GetType() == typeof(double) && sright.Content!.GetType() == typeof(double))
                        {
                            return new Some(apply<double, double>((a, b) => a + b));
                        }
                        if (sleft.Content!.GetType() == typeof(string) && sright.Content!.GetType() == typeof(string))
                        {
                            return new Some(apply<string, string>((a, b) => a + b));
                        }
                    }
                    throw new RuntimeException(expr.Op, "Operands must be numbers or strings");
                case TokenType.GREATER:
                    CheckNumberOperands(expr.Op, left, right);
                    return new Some(apply<double, bool>((a, b) => a > b));
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr.Op, left, right);
                    return new Some(apply<double, bool>((a, b) => a >= b));
                case TokenType.LESS:
                    CheckNumberOperands(expr.Op, left, right);
                    return new Some(apply<double, bool>((a, b) => a < b));
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr.Op, left, right);
                    return new Some(apply<double, bool>((a, b) => a <= b));
                case TokenType.BANG_EQUAL:
                    return new Some(!IsEqual(left, right));
                case TokenType.EQUAL_EQUAL:
                    return new Some(IsEqual(left, right));
            }
            return None.Value;
        }

        private static bool IsEqual(object? left, object? right)
        {
            if (left == null && right == null) return true;
            if (left == null || right == null) return false;
            return left.Equals(right);
        }

        public Option Visit(Expr.Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        public Option Visit(Expr.Literal expr)
        {
            return new Some(expr.Value);
        }

        public Option Visit(Expr.Unary expr)
        {
            Option right = Evaluate(expr.Right);

            switch (expr.Op.type)
            {
                case TokenType.MINUS:
                    CheckNumberOperand(expr.Op, right);
                    return new Some(-(double?)((Some)right).Content);
                case TokenType.BANG: return new Some(!IsTruhty(right));
            }
            return None.Value;
        }

        public static bool IsTruhty(Option value)
        {
            if (value.GetType() == typeof(None)) return false;
            Some v = (Some)value;
            if (v.Content == null) return false;
            if (v.Content.GetType() == typeof(bool)) return (bool)v.Content;
            if (v.Content.GetType() == typeof(double)) return ((double)v.Content) != 0;
            return true;
        }

        public static void CheckNumberOperand(Token op, Option operand)
        {
            if (operand is Some some)
            {
                if (some.Content != null && some.Content.GetType() == typeof(Double))
                {
                    return;
                }
            }
            throw new RuntimeException(op, "Operand must be a number");
        }

        public static void CheckNumberOperands(Token op, Option left, Option right)
        {
            if ((left is Some lsome) && (right is Some rsome))
            {
                if (lsome.Content != null && rsome.Content != null && lsome.Content.GetType() == typeof(Double) && rsome.Content.GetType() == typeof(Double))
                {
                    return;
                }
            }
            throw new RuntimeException(op, "Operands must be numbers");
        }

        public Option Visit(Stmt.Expression stmt)
        {
            return Evaluate(stmt.expression);
        }

        public Option Visit(Stmt.Print stmt)
        {
            Option value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return None.Value;
        }

        public Option Visit(Expr.Variable expr)
        {
            return new Some(Env.Get(expr.name));
        }

        public Option Visit(Stmt.Var stmt)
        {
            Option value = None.Value;
            if (stmt.initializer != null)
            {
                value = Evaluate(stmt.initializer);
            }
            if (value is Some some)
            {
                Env.Define(stmt.name.lexeme, some.Content);
            }
            else
            {
                throw new RuntimeException(stmt.name, "Cannot assign None to variable");
            }
            return None.Value;
        }

        public Option Visit(Expr.Assign expr)
        {
            Option value = Evaluate(expr.value);
            if (value is Some some)
            {
                Env.Assign(expr.name, some.Content);
                return value;
            }
            else
            {
                throw new RuntimeException(expr.name, "Cannot assign None to variable");
            }
        }

        public Option Visit(Stmt.Block stmt)
        {
            return ExecuteBlock(stmt.statements, new Environment(Env));
        }

        public Option ExecuteBlock(List<Stmt> statements, Environment env)
        {
            Environment previous = Env;
            try
            {
                Env = env;
                foreach (Stmt stmt in statements)
                {
                    Option value = Execute(stmt);
                    if (value.GetType() == typeof(Continue) || value.GetType() == typeof(Break))
                    {
                        return value;
                    }
                }
            }
            finally
            {
                Env = previous;
            }
            return None.Value;
        }

        public Option Visit(Stmt.If stmt)
        {
            if (IsTruhty(Evaluate(stmt.condition)))
            {
                return Execute(stmt.then_branch);
            }
            else if (stmt.else_branch != null)
            {
                return Execute(stmt.else_branch);
            }
            return None.Value;
        }

        public Option Visit(Expr.Logical expr)
        {
            Option left = Evaluate(expr.left);
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

        public Option Visit(Stmt.While stmt)
        {
            while (IsTruhty(Evaluate(stmt.condition)))
            {
                Option value = Execute(stmt.body);
                if (value.GetType() == typeof(Break))
                {
                    break;
                }
                if (stmt.end_of_loop != null)
                {
                    Evaluate(stmt.end_of_loop);
                }
            }
            if (stmt.final != null)
            {
                Execute(stmt.final);
            }
            return None.Value;
        }

        public Option Visit(Stmt.Break stmt)
        {
            return new Break();
        }

        public Option Visit(Stmt.Continue stmt)
        {
            return new Continue();
        }

        public Option Visit(Expr.Call expr)
        {
            Option callee = Evaluate(expr.callee);
            if (callee is Some some)
            {
                object value = some.Content ?? throw new RuntimeException(expr.paren, "Cannnot call null");
                if (value is not LoxCallable)
                {
                    throw new RuntimeException(expr.paren, "Cannot call a non-callable object.");
                }
                List<object?> args = [];
                foreach (var arg in expr.args)
                {
                    Option aa = Evaluate(arg);
                    if (aa is Some sarg)
                    {
                        args.Add(sarg.Content);
                    }
                    else
                    {
                        throw new RuntimeException(expr.paren, "Cannot pass None argument to call");
                    }
                }
                LoxCallable callable = (LoxCallable)value;
                if (args.Count != callable.Arity())
                {
                    throw new RuntimeException(expr.paren, $"Expected {callable.Arity()} arguments but got {args.Count}.");
                }
                return callable.CallFunction(this, args);
            }
            else
            {
                throw new RuntimeException(expr.paren, "Cannot call null");
            }
        }

        public Option Visit(Stmt.Function stmt)
        {
            LoxFunction func = new(stmt, Env);
            Env.Define(stmt.name.lexeme, func);
            return None.Value;
        }

        public Option Visit(Stmt.Return stmt)
        {
            Option value = None.Value;
            if (stmt.value != null)
            {
                value = Evaluate(stmt.value);
            }
            throw new Return(value);
        }

        public Option Visit(Expr.AnonymousFn expr)
        {
            return new Some(new LoxAnonymousFunction(expr, Env));
        }
    }
}