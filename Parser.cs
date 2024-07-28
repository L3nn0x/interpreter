using Ast;
namespace interpreter
{
    class ParseError(string message) : Exception(message) { }

    public class Parser(List<Token> Tokens)
    {
        public List<Token> Tokens = Tokens;
        private int Current = 0;

        public List<Stmt?> Parse()
        {
            List<Stmt?> statements = [];

            while (!IsAtEnd())
            {
                statements.Add(Declaration(false));
            }
            return statements;
        }

        private Stmt? Declaration(bool is_in_loop)
        {
            try
            {
                if (Match(TokenType.FUN))
                {
                    return Function("function");
                }
                if (Match(TokenType.VAR))
                {
                    return VarDeclaration();
                }
                return Statement(is_in_loop);
            }
            catch (ParseError)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt Function(string kind)
        {
            Token name = Consume(TokenType.IDENTIFIER, $"Expected {kind} name.");
            Consume(TokenType.LEFT_PAREN, $"Expected '(' after {kind} name.");
            List<Token> parameters = [];
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count > 254)
                    {
                        Error(Peek(), "Cannot have more than 255 parameters.");
                    }
                    parameters.Add(Consume(TokenType.IDENTIFIER, "Expected parameter name"));
                } while (Match(TokenType.COMMA));
            }
            Consume(TokenType.RIGHT_PAREN, "Expected ')' after parameters");
            Consume(TokenType.LEFT_BRACE, $"Expected '{{' before {kind} body.");
            return new Stmt.Function(name, parameters, Block(false));
        }

        private Stmt VarDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expected variable name");
            Expr? expr = null;
            if (Match(TokenType.EQUAL))
            {
                expr = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expected ';' after variable declaration");
            return new Stmt.Var(name, expr);
        }

        private Stmt Statement(bool is_in_loop)
        {
            if (Match(TokenType.PRINT))
            {
                return PrintStatement();
            }
            if (Match(TokenType.RETURN))
            {
                return ReturnStatement();
            }
            if (Match(TokenType.LEFT_BRACE))
            {
                return new Stmt.Block(Block(is_in_loop));
            }
            if (Match(TokenType.IF))
            {
                return If(is_in_loop);
            }
            if (Match(TokenType.WHILE))
            {
                return While();
            }
            if (Match(TokenType.FOR))
            {
                return For();
            }
            if (Match(TokenType.BREAK))
            {
                return Break(is_in_loop);
            }
            if (Match(TokenType.CONTINUE))
            {
                return Continue(is_in_loop);
            }
            return ExpressionStatement();
        }

        private Stmt ReturnStatement()
        {
            Token keyword = Previous();
            Expr? value = null;
            if (!Check(TokenType.SEMICOLON))
            {
                value = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expected ';' after return");
            return new Stmt.Return(keyword, value);
        }

        private Stmt Break(bool is_in_loop)
        {
            if (!is_in_loop)
            {
                Token b = Previous();
                Error(b, "break can only appear in a loop body");
            }
            Consume(TokenType.SEMICOLON, "Expected ';' after break");
            return new Stmt.Break();
        }

        private Stmt Continue(bool is_in_loop)
        {
            if (!is_in_loop)
            {
                Token c = Previous();
                Error(c, "continue can only appear in a loop body");
            }
            Consume(TokenType.SEMICOLON, "Expected ';' after continue");
            return new Stmt.Continue();
        }

        private Stmt For()
        {
            Consume(TokenType.LEFT_PAREN, "Expected '(' after for");
            Stmt? initializer;
            if (Match(TokenType.SEMICOLON))
            {
                initializer = null;
            }
            else if (Match(TokenType.VAR))
            {
                initializer = VarDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            Expr? condition = null;
            if (!Check(TokenType.SEMICOLON))
            {
                condition = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expected ';' after loop condition");

            Expr? increment = null;
            if (!Check(TokenType.RIGHT_PAREN))
            {
                increment = Expression();
            }
            Consume(TokenType.RIGHT_PAREN, "Expected ')' after for");
            Stmt body = Statement(true);
            condition ??= new Expr.Literal(true);
            Stmt? final = null;
            if (Check(TokenType.FINALLY))
            {
                Advance();
                final = Statement(false);
            }
            body = new Stmt.While(condition, body, increment, final);
            if (initializer != null)
            {
                body = new Stmt.Block([initializer, body]);
            }
            return body;
        }

        private Stmt While()
        {
            Consume(TokenType.LEFT_PAREN, "Expected '(' after while");

            Expr condition = Expression();

            Consume(TokenType.RIGHT_PAREN, "Expected ')' after while condition");
            Stmt body = Statement(true);
            Stmt? final = null;
            if (Check(TokenType.FINALLY))
            {
                Advance();
                final = Statement(false);
            }
            return new Stmt.While(condition, body, null, final);
        }

        private Stmt If(bool is_in_loop)
        {
            Consume(TokenType.LEFT_PAREN, "Expected '(' after if.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expected ')' after if condition.");
            Stmt then_branch = Statement(is_in_loop);
            Stmt? else_branch = null;
            if (Match(TokenType.ELSE))
            {
                else_branch = Statement(is_in_loop);
            }
            return new Stmt.If(condition, then_branch, else_branch);
        }

        private List<Stmt> Block(bool is_in_loop)
        {
            List<Stmt> statements = [];

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration(is_in_loop)!);
            }
            Consume(TokenType.RIGHT_BRACE, "Expected '}' at the end of the block");

            return statements;
        }

        private Stmt PrintStatement()
        {
            Expr expr = Expression();
            Consume(TokenType.SEMICOLON, "Expected ';' after value");
            return new Stmt.Print(expr);
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(TokenType.SEMICOLON, "Expected ';' after value");
            return new Stmt.Expression(expr);
        }

        private bool IsAtEnd()
        {
            return Current >= Tokens.Count || Peek().type == TokenType.EOF;
        }

        private Expr Expression()
        {
            return Assignment();
        }

        private Expr Assignment()
        {
            Expr expr = Or();

            if (Match(TokenType.EQUAL))
            {
                Token equals = Previous();
                Expr value = Assignment();

                if (expr.GetType() == typeof(Expr.Variable))
                {
                    Token name = ((Expr.Variable)expr).name;
                    return new Expr.Assign(name, value);
                }
                Error(equals, "Invalid assignment target");
            }
            return expr;
        }

        private Expr Or()
        {
            Expr expr = And();

            while (Match(TokenType.OR) && !IsAtEnd())
            {
                Token op = Previous();
                Expr right = And();
                expr = new Expr.Logical(expr, op, right);
            }
            return expr;
        }

        private Expr And()
        {
            Expr expr = Equality();

            while (Match(TokenType.AND) && !IsAtEnd())
            {
                Token op = Previous();
                Expr right = Equality();
                expr = new Expr.Logical(expr, op, right);
            }
            return expr;
        }

        private Expr Equality()
        {
            Expr expr = Comparison();

            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token op = Previous();
                Expr right = Comparison();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Comparison()
        {
            Expr expr = Term();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token op = Previous();
                Expr right = Term();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Term()
        {
            Expr expr = Factor();

            while (Match(TokenType.PLUS, TokenType.MINUS))
            {
                Token op = Previous();
                Expr right = Factor();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Factor()
        {
            Expr expr = Unary();

            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                Token op = Previous();
                Expr right = Unary();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Unary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                Token op = Previous();
                Expr right = Unary();
                return new Expr.Unary(op, right);
            }
            return Call();
        }

        private Expr Call()
        {
            Expr expr = Primary();

            while (true)
            {
                if (Match(TokenType.LEFT_PAREN))
                {
                    expr = FinishCall(expr);
                }
                else
                {
                    break;
                }
            }
            return expr;
        }

        private Expr FinishCall(Expr expr)
        {
            List<Expr> args = [];
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (args.Count > 254)
                    {
                        Error(Peek(), "Can't have more than 255 arguments.");
                    }
                    args.Add(Expression());
                } while (Match(TokenType.COMMA));
            }
            Token paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");
            return new Expr.Call(expr, paren, args);
        }

        private Expr Primary()
        {
            if (Match(TokenType.FALSE)) return new Expr.Literal(false);
            if (Match(TokenType.TRUE)) return new Expr.Literal(true);
            if (Match(TokenType.NIL)) return new Expr.Literal(null);
            if (Match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(Previous().literal);
            }
            if (Match(TokenType.IDENTIFIER)) return new Expr.Variable(Previous());
            if (Match(TokenType.LEFT_PAREN))
            {
                Expr expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expected ')' after expression");
                return new Expr.Grouping(expr);
            }

            throw Error(Peek(), "Expected expression.");
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().type == TokenType.SEMICOLON) return;

                switch (Peek().type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                    case TokenType.CONTINUE:
                    case TokenType.BREAK:
                    case TokenType.FINALLY:
                        return;
                }

                Advance();
            }
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();
            throw Error(Peek(), message);
        }

        private ParseError Error(Token token, string message)
        {
            Program.Error(token, message);
            return new ParseError(message);
        }

        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd()) Current++;
            return Previous();
        }

        private Token Peek()
        {
            return Tokens[Current];
        }

        private Token Previous()
        {
            return Tokens[Current - 1];
        }
    }
}