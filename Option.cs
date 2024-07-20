namespace interpreter
{
    public abstract class Option<T>
    {
        public static implicit operator Option<T>(T some) => new Some<T>(some);
        public static implicit operator Option<T>(None none) => new None<T>();
    }

    public sealed class Some<T>(T Content) : Option<T>
    {
        public T Content { get; } = Content;
    }

    public sealed class None<T> : Option<T>
    {

    }

    public sealed class None
    {
        public static None Value { get; } = new None();
        private None() { }
    }

}