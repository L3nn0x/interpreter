using Ast;

namespace interpreter
{
    interface LoxCallable
    {
        public int Arity();
        public Option CallFunction(Interpreter interpreter, List<object?> args);
    }

    public class LoxFunction(Stmt.Function declaration, Environment closure) : LoxCallable
    {
        private readonly Stmt.Function Declaration = declaration;
        private Environment closure = closure;
        public int Arity() => Declaration.args.Count;

        Option LoxCallable.CallFunction(Interpreter interpreter, List<object?> args)
        {
            Environment env = new(closure);
            for (int i = 0; i < args.Count; ++i)
            {
                env.Define(Declaration.args[i].lexeme, args[i]);
            }
            try
            {
                return interpreter.ExecuteBlock(Declaration.body, env);
            }
            catch (Return value)
            {
                return value.Value;
            }
        }

        public override string ToString() => $"<fn {(Declaration.name != null ? Declaration.name.lexeme : "anonymous")}>";
    }

    public class LoxAnonymousFunction(Expr.AnonymousFn declaration, Environment closure) : LoxCallable
    {
        private readonly Expr.AnonymousFn Declaration = declaration;
        private Environment closure = closure;
        public int Arity() => Declaration.args.Count;

        Option LoxCallable.CallFunction(Interpreter interpreter, List<object?> args)
        {
            Environment env = new(closure);
            for (int i = 0; i < args.Count; ++i)
            {
                env.Define(Declaration.args[i].lexeme, args[i]);
            }
            try
            {
                return interpreter.ExecuteBlock(Declaration.body, env);
            }
            catch (Return value)
            {
                return value.Value;
            }
        }

        public override string ToString() => $"<fn anonymous>";
    }
}