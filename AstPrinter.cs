using Ast;

namespace interpreter
{
    class AstPrinter : Expr.IVisitor<string>, Stmt.IVisitor<string>
    {
        public string Print(Stmt stmt)
        {
            return stmt.Accept(this);
        }
        public string Visit(Expr.Binary expr)
        {
            return Parenthesize(expr.Op.lexeme, expr.Left, expr.Right);
        }

        public string Visit(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.Expression);
        }

        public string Visit(Expr.Literal expr)
        {
            if (expr.Value == null)
            {
                return "nil";
            }
            return expr.Value.ToString()!;
        }

        public string Visit(Expr.Unary expr)
        {
            return Parenthesize(expr.Op.lexeme, expr.Right);
        }

        public string Visit(Expr.Variable expr)
        {
            return expr.name.lexeme;
        }

        public string Visit(Expr.Assign expr)
        {
            return Parenthesize($"= {expr.name.lexeme}", expr.value);
        }

        public string Visit(Expr.Logical expr)
        {
            return Parenthesize($"{expr.op.lexeme}", expr.left, expr.right);
        }

        public string Visit(Stmt.Block stmt)
        {
            string res = "(BLOCK ";
            foreach (var s in stmt.statements)
            {
                res += s.Accept(this) + " ";
            }
            return res[..(res.Length - 1)] + ")";
        }

        public string Visit(Stmt.Expression stmt)
        {
            return stmt.expression.Accept(this);
        }

        public string Visit(Stmt.If stmt)
        {
            string left = stmt.then_branch.Accept(this);
            string right = stmt.else_branch == null ? "null" : stmt.else_branch.Accept(this);
            return $"(IF {left} {right})";
        }

        public string Visit(Stmt.Print stmt)
        {
            return $"(PRINT {stmt.expression.Accept(this)})";
        }

        public string Visit(Stmt.Var stmt)
        {
            return $"(VAR {stmt.name} {(stmt.initializer == null ? "null" : stmt.initializer.Accept(this))})";
        }

        public string Visit(Stmt.While stmt)
        {
            return $"(WHILE COND {stmt.condition.Accept(this)} BODY {stmt.body.Accept(this)}) INC {(stmt.end_of_loop == null ? "" : stmt.end_of_loop.Accept(this))} FINALLY {(stmt.final == null ? "" : stmt.final.Accept(this))}";
        }

        public string Visit(Stmt.Break stmt)
        {
            return "(BREAK)";
        }

        public string Visit(Stmt.Continue stmt)
        {
            return "(CONTINUE)";
        }

        string Parenthesize(string name, params Expr[] exprs)
        {
            string s = $"({name}";
            foreach (Expr e in exprs)
            {
                s += $" {e.Accept(this)}";
            }
            s += ")";
            return s;
        }
    };
}