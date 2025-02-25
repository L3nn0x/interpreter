namespace interpreter
{
    public enum TokenType
    {
        // single char tokens
        LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE, COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,
        // one or two char tokens
        BANG, BANG_EQUAL, EQUAL, EQUAL_EQUAL, GREATER, GREATER_EQUAL, LESS, LESS_EQUAL,
        // literals
        IDENTIFIER, STRING, NUMBER,
        // keywords
        AND, CLASS, ELSE, FALSE, FUN, FOR, IF, NIL, OR, PRINT, RETURN, SUPER, THIS, TRUE, VAR, WHILE, BREAK, CONTINUE, FINALLY, EOF
    }

    public class Token(TokenType type, string lexeme, object? literal, int line)
    {
        public readonly TokenType type = type;
        public readonly string lexeme = lexeme;
        public readonly object? literal = literal;
        public readonly int line = line;

        public sealed override string ToString()
        {
            return $"{type} {lexeme} {literal}";
        }
    }
}