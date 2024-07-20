namespace interpreter
{
    class Environment
    {
        public Environment() { }
        public Environment(Environment parent)
        {
            Enclosing = parent;
        }

        private Environment? Enclosing = null;
        private Dictionary<string, object?> Values = [];

        public void Define(string name, object? value)
        {
            Values.Add(name, value);
        }

        public object? Get(Token name)
        {
            bool contains = Values.TryGetValue(name.lexeme, out var value);
            if (contains)
            {
                return value;
            }
            if (Enclosing != null) return Enclosing.Get(name);

            throw new RuntimeException(name, $"Undefined variable '{name.lexeme}'");
        }

        public void Assign(Token name, object? value)
        {
            if (Values.ContainsKey(name.lexeme))
            {
                Values.Add(name.lexeme, value);
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
    }
}