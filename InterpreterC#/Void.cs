namespace interpreter
{
    public readonly struct Void : IEquatable<Void>
    {
        public static readonly Void unit;
        public override readonly bool Equals(object? obj) => obj is Void;
        public override readonly int GetHashCode() => 0;
        public static bool operator ==(Void left, Void right) => left.Equals(right);
        public static bool operator !=(Void left, Void right) => !(left == right);
        public readonly bool Equals(Void other) => true;
        public override readonly string ToString() => "()";
    }
}