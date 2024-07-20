namespace interpreter
{
    class Environment
    {
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
            throw new RuntimeException(name, $"Undefined variable '{name.lexeme}'");
        }
    }
}