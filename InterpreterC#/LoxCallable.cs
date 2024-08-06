using Ast;

namespace interpreter
{
    interface LoxCallable
    {
        public int Arity();
        public Option CallFunction(Interpreter interpreter, List<object?> args);
    }

    public class LoxFunction(Stmt.Function declaration, Environment closure, bool IsInitializer) : LoxCallable
    {
        private readonly Stmt.Function Declaration = declaration;
        private Environment closure = closure;
        public int Arity() => Declaration.args.Count;
        public bool IsInitializer = IsInitializer;

        public Option CallFunction(Interpreter interpreter, List<object?> args)
        {
            Environment env = new(closure);
            for (int i = 0; i < args.Count; ++i)
            {
                env.Define(Declaration.args[i], args[i]);
            }
            try
            {
                Option result = interpreter.ExecuteBlock(Declaration.body, env);
                if (IsInitializer)
                {
                    return new Some(closure.GetAt("this", 0));
                }
                return result;
            }
            catch (Return value)
            {
                if (IsInitializer)
                {
                    return new Some(closure.GetAt("this", 0));
                }
                return value.Value;
            }
        }

        public LoxFunction Bind(LoxInstance instance)
        {
            Environment env = new(closure);
            env.Define("this", instance);
            return new LoxFunction(Declaration, env, IsInitializer);
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
                env.Define(Declaration.args[i], args[i]);
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