namespace interpreter
{
    public abstract class Option
    {
    }

    public sealed class Some(object? Content) : Option
    {
        public object? Content { get; } = Content;
    }

    public sealed class None : Option
    {
        public static None Value { get; } = new None();
    }

    public sealed class Break : Option { }
    public sealed class Continue : Option { }
}