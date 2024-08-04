using Ast;
namespace interpreter
{
    enum FunctionType
    {
        NONE,
        FUNCTION
    };

    class Resolver : Expr.IVisitor<Void>, Stmt.IVisitor<Void>
    {
        private Interpreter interpreter;
        private Stack<Dictionary<string, bool>> scopes = new();

        private FunctionType currentFunction = FunctionType.NONE;

        bool? GetValueOrNull<K>(Dictionary<K, bool> dict, K key) where K : notnull
        {
            return dict.TryGetValue(key, out bool value) ? value : null;
        }

        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
            BeginScope();
        }

        public void Resolve(List<Stmt?> statements)
        {
            foreach (var it in statements)
            {
                if (it == null) continue;
                Resolve(it);
            }
        }

        private void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        private void ResolveLocal(Expr expr, Token token)
        {
            int i = scopes.Count - 1;
            foreach (var scope in scopes)
            {
                if (scope.ContainsKey(token.lexeme))
                {
                    interpreter.Resolve(expr, scopes.Count - 1 - i);
                    return;
                }
                --i;
            }
        }

        private void ResolveFunction(Stmt.Function stmt, FunctionType type)
        {
            FunctionType enclosing_function = currentFunction;
            currentFunction = type;
            BeginScope();
            foreach (var token in stmt.args)
            {
                Declare(token);
                Define(token.lexeme);
            }
            Resolve(stmt.body);
            EndScope();
            currentFunction = enclosing_function;
        }

        private void BeginScope()
        {
            scopes.Push([]);
        }

        private void EndScope()
        {
            scopes.Pop();
        }

        private void Declare(Token name)
        {
            if (scopes.Count == 0)
            {
                return;
            }
            if (scopes.Peek().ContainsKey(name.lexeme))
            {
                Program.Error(name, "A variable with this name exists");
                return;
            }
            scopes.Peek().Add(name.lexeme, false);
        }

        private void Define(string name)
        {
            if (scopes.Count == 0) return;
            scopes.Peek()[name] = true;
        }

        public Void Visit(Expr.Assign expr)
        {
            Resolve(expr.value);
            ResolveLocal(expr, expr.name);
            return Void.unit;
        }

        public Void Visit(Expr.Binary expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return Void.unit;
        }

        public Void Visit(Expr.Call expr)
        {
            Resolve(expr.callee);
            foreach (var arg in expr.args)
            {
                Resolve(arg);
            }
            return Void.unit;
        }

        public Void Visit(Expr.Logical expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return Void.unit;
        }

        public Void Visit(Expr.Grouping expr)
        {
            Resolve(expr.Expression);
            return Void.unit;
        }

        public Void Visit(Expr.Literal expr)
        {
            return Void.unit;
        }

        public Void Visit(Expr.Unary expr)
        {
            Resolve(expr.Right);
            return Void.unit;
        }

        public Void Visit(Expr.Variable expr)
        {
            if (scopes.Count != 0 && GetValueOrNull(scopes.Peek(), expr.name.lexeme) == false)
            {
                Program.Error(expr.name, "Can't read local variable in its own initializer");
            }
            ResolveLocal(expr, expr.name);
            return Void.unit;
        }

        public Void Visit(Expr.AnonymousFn expr)
        {
            ResolveFunction(new Stmt.Function(new Token(TokenType.IDENTIFIER, "", null, 0), expr.args, expr.body), FunctionType.FUNCTION);
            return Void.unit;
        }

        public Void Visit(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.statements);
            EndScope();
            return Void.unit;
        }

        public Void Visit(Stmt.Expression stmt)
        {
            Resolve(stmt.expression);
            return Void.unit;
        }

        public Void Visit(Stmt.Function stmt)
        {
            Declare(stmt.name);
            Define(stmt.name.lexeme);
            ResolveFunction(stmt, FunctionType.FUNCTION);
            return Void.unit;
        }

        public Void Visit(Stmt.If stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.then_branch);
            if (stmt.else_branch != null) Resolve(stmt.else_branch);
            return Void.unit;
        }

        public Void Visit(Stmt.Print stmt)
        {
            Resolve(stmt.expression);
            return Void.unit;
        }

        public Void Visit(Stmt.Return stmt)
        {
            if (currentFunction == FunctionType.NONE)
            {
                Program.Error(stmt.keyword, "Can't return from top-level code");
            }
            if (stmt.value != null) Resolve(stmt.value);
            return Void.unit;
        }

        public Void Visit(Stmt.Var stmt)
        {
            Declare(stmt.name);
            if (stmt.initializer != null)
            {
                Resolve(stmt.initializer);
            }
            Define(stmt.name.lexeme);
            return Void.unit;
        }

        public Void Visit(Stmt.While stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.body);
            return Void.unit;
        }

        public Void Visit(Stmt.Break stmt)
        {
            return Void.unit;
        }

        public Void Visit(Stmt.Continue stmt)
        {
            return Void.unit;
        }
    }
}