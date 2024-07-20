using Ast;

namespace interpreter
{
    class AstPrinter : Expr.IVisitor<string>
    {
        public string Print(Expr expr)
        {
            return expr.Accept(this);
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