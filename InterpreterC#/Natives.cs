namespace interpreter
{
    class Clock : LoxCallable
    {
        public int Arity()
        {
            return 0;
        }

        public Option CallFunction(Interpreter interpreter, List<object?> args)
        {
            return new Some((double)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond));
        }

        public override string ToString()
        {
            return "<native fn>";
        }
    }

    class Input : LoxCallable
    {
        public int Arity() { return 0; }
        public Option CallFunction(Interpreter interpreter, List<object?> args)
        {
            Console.Write(args.Count == 1 ? args[0] : "");
            return new Some(Console.ReadLine());
        }
    };
}