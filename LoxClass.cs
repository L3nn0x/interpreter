
namespace interpreter
{
    public class LoxClass(string name, Dictionary<string, LoxFunction> methods) : LoxCallable
    {
        public readonly string Name = name;
        public readonly Dictionary<string, LoxFunction> Methods = methods;

        public int Arity()
        {
            LoxFunction? initializer = Methods.TryGetValue("init", out var method) ? method : null;
            if (initializer != null)
            {
                return initializer.Arity();
            }
            return 0;
        }
        public Option CallFunction(Interpreter interpreter, List<object?> args)
        {
            LoxInstance instance = new(this);
            LoxFunction? initializer = Methods.TryGetValue("init", out var method) ? method : null;
            if (initializer != null)
            {
                initializer.Bind(instance).CallFunction(interpreter, args);
            }
            return new Some(instance);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class LoxInstance(LoxClass klass)
    {
        private readonly LoxClass Klass = klass;
        private Dictionary<string, object?> Properties = [];

        public LoxFunction? FindFunction(string name)
        {
            LoxFunction? f = klass.Methods.TryGetValue(name, out var method) ? method : null;
            if (f != null)
            {
                return f.Bind(this);
            }
            return f;
        }

        public object? Get(Token name)
        {
            if (Properties.TryGetValue(name.lexeme, out object? value))
            {
                return value;
            }
            var method = FindFunction(name.lexeme);
            if (method != null)
            {
                return method;
            }
            throw new RuntimeException(name, $"Undefined property '{name.lexeme}'.");
        }

        public void Set(Token name, object? value)
        {
            Properties[name.lexeme] = value;
        }

        public override string ToString()
        {
            return $"{Klass} instance";
        }
    }
}