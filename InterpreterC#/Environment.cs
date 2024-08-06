namespace interpreter
{
    public class Environment
    {
        public Environment() { }
        public Environment(Environment parent)
        {
            Enclosing = parent;
        }

        public Environment? Enclosing = null;
        private Dictionary<string, object?> Values = [];

        public void Define(Token name, object? value)
        {
            try
            {
                Values.Add(name.lexeme, value);
            }
            catch (ArgumentException)
            {
                throw new RuntimeException(name, "Variable already exists");
            }
        }

        public void Define(string name, object? value)
        {
            try
            {
                Values.Add(name, value);
            }
            catch (ArgumentException)
            {
                throw new RuntimeException($"Variable {name} already exists");
            }
        }

        public object? Get(Token name)
        {
            var (found, value) = GetInternal(name.lexeme);
            if (found) return value;
            if (Enclosing != null) return Enclosing.Get(name);

            throw new RuntimeException(name, $"Undefined variable '{name.lexeme}'");
        }

        private (bool, object?) GetInternal(string name)
        {
            bool contains = Values.TryGetValue(name, out var value);
            if (contains)
            {
                return (true, value);
            }
            return (false, null);
        }

        public object? GetAt(Token name, int distance)
        {
            if (distance == 0 || Enclosing == null)
            {
                var (found, value) = GetInternal(name.lexeme);
                if (!found)
                    throw new RuntimeException(name, $"Undefined variable '{name.lexeme}'");
                return value;
            }
            return Enclosing.GetAt(name, distance - 1);
        }

        public object? GetAt(string name, int distance)
        {
            if (distance == 0 || Enclosing == null)
            {
                var (found, value) = GetInternal(name);
                if (!found)
                    throw new RuntimeException($"Undefined variable '{name}'");
                return value;
            }
            return Enclosing.GetAt(name, distance - 1);
        }

        public void Assign(Token name, object? value)
        {
            if (Values.ContainsKey(name.lexeme))
            {
                Values[name.lexeme] = value;
            }
            else
            {
                if (Enclosing != null)
                {
                    Enclosing.Assign(name, value);
                }
                else
                {
                    throw new RuntimeException(name, $"Undefined variable '{name.lexeme}'");
                }
            }
        }

        public void AssignAt(int distance, Token name, object? value)
        {
            if (distance == 0 || Enclosing == null)
            {
                if (Values.ContainsKey(name.lexeme))
                {
                    Values[name.lexeme] = value;
                }
                else
                {
                    throw new RuntimeException(name, $"Undefined variable '{name.lexeme}'");
                }
                return;
            }
            Enclosing.AssignAt(distance - 1, name, value);
        }
    }
}