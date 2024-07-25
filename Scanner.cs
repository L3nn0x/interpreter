namespace interpreter
{
    class Scanner(string Source)
    {
        private readonly Dictionary<string, TokenType> Keywords = new() {
        {"and", TokenType.AND},
        {"or", TokenType.OR},
        {"class", TokenType.CLASS},
        {"else", TokenType.ELSE},
        {"false", TokenType.FALSE},
        {"fun", TokenType.FUN},
        {"for", TokenType.FOR},
        {"if", TokenType.IF},
        {"nil", TokenType.NIL},
        {"print", TokenType.PRINT},
        {"return", TokenType.RETURN},
        {"super", TokenType.SUPER},
        {"this", TokenType.THIS},
        {"true", TokenType.TRUE},
        {"var", TokenType.VAR},
        {"while", TokenType.WHILE},
        {"continue", TokenType.CONTINUE},
        {"break", TokenType.BREAK}
    };


        private readonly string Source = Source;
        private List<Token> Tokens = [];
        private int Start = 0;
        private int Current = 0;
        private int Line = 1;

        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                Start = Current;
                ScanToken();
            }
            Tokens.Add(new Token(TokenType.EOF, "", null, Line));
            return Tokens;
        }

        private bool IsAtEnd()
        {
            return Current >= Source.Length;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;

                case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
                case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
                case '<': AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
                case '>': AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;

                case '/':
                    if (Match('/'))
                    {
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;

                case ' ':
                case '\r':
                case '\t':
                    break; // ignore whitespace
                case '\n':
                    ++Line;
                    break;

                case '"': String(); break;

                default:
                    if (IsDigit(c))
                    {
                        Number();
                    }
                    else if (IsAlpha(c))
                    {
                        Identifier();
                    }
                    else
                    {
                        Program.Error(Line, $"Unexpected character {c}");
                    }
                    break;
            }
        }

        private char Advance()
        {
            return Source[Current++];
        }

        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return Source[Current];
        }

        private char PeekNext()
        {
            if (Current + 1 >= Source.Length) return '\0';
            return Source[Current + 1];
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object? literal)
        {
            string text = Source[Start..Current];
            Tokens.Add(new(type, text, literal, Line));
        }

        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (Source[Current] != expected) return false;
            ++Current;
            return true;
        }

        private void String()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') ++Line;
                Advance();
            }
            if (IsAtEnd())
            {
                Program.Error(Line, "Unterminated string.");
                return;
            }
            Advance(); // the closing '"'
            string value = Source[(Start + 1)..(Current - 1)];
            AddToken(TokenType.STRING, value);
        }

        private static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private static bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
        }

        private static bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private void Number()
        {
            while (IsDigit(Peek()) && !IsAtEnd())
            {
                Advance();
            }

            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();
                while (IsDigit(Peek())) Advance();
            }

            AddToken(TokenType.NUMBER, Double.Parse(Source[Start..Current]));
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();
            string text = Source[Start..Current];
            if (!Keywords.TryGetValue(text, out TokenType type))
            {
                type = TokenType.IDENTIFIER;
            }
            AddToken(type);
        }
    }
}