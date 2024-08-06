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

        public string Visit(Expr.Call expr)
        {
            string args = "";
            foreach (var arg in expr.args)
            {
                args += " " + arg.Accept(this);
            }
            return $"(CALL {expr.callee.Accept(this)} {args})";
        }

        public string Visit(Stmt.Function stmt)
        {
            string args = "";
            foreach (var arg in stmt.args)
            {
                args += " " + arg.lexeme;
            }
            string stmts = "";
            foreach (var s in stmt.body)
            {
                stmts += " " + s.Accept(this);
            }
            return $"(FUN {stmt.name.lexeme} ARGS {args} BODY {stmts})";
        }

        public string Visit(Stmt.Return stmt)
        {
            return $"(RET {(stmt.value == null ? stmt.value!.Accept(this) : "nil")})";
        }

        public string Visit(Expr.AnonymousFn expr)
        {
            string args = "";
            foreach (var arg in expr.args)
            {
                args += " " + arg.lexeme;
            }
            string stmts = "";
            foreach (var s in expr.body)
            {
                stmts += " " + s.Accept(this);
            }
            return $"(FUN <anonymous> ARGS {args} BODY {stmts})";
        }

        public string Visit(Stmt.Class stmt)
        {
            string out_var = $"(CLASS {stmt.name.lexeme}";

            foreach (var method in stmt.methods)
            {
                out_var += " " + method.Accept(this);
            }

            return out_var + ")";
        }

        public string Visit(Expr.Get expr)
        {
            return $"(GET {expr.name.lexeme} {expr.obj.Accept(this)})";
        }

        public string Visit(Expr.Set expr)
        {
            return $"(SET {expr.name.lexeme} {expr.obj.Accept(this)})";
        }

        public string Visit(Expr.This expr)
        {
            return "(THIS)";
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