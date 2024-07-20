namespace GenerateAst
{
    public class GenerateAst
    {
        public static void Generate(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage GenerateAst <output_directory>");
                Environment.Exit(1);
            }
            string output = args[0];
            DefineAst(output, "Expr", [
                "Binary  : Expr Left, Token Op, Expr Right",
                "Grouping: Expr Expression",
                "Literal : object? Value",
                "Unary   : Token Op, Expr Right",
                "Variable: Token name"
            ]);
            DefineAst(output, "Stmt", [
                "Expression : Expr expression",
                "Print      : Expr expression",
                "Var        : Token name, Expr? initializer"
            ]);
        }

        static void DefineAst(string output, string basename, List<string> types)
        {
            string filename = Path.Join(output, basename + ".cs");
            using (StreamWriter outputFile = new StreamWriter(filename, false))
            {
                outputFile.WriteLine("using interpreter;");
                outputFile.WriteLine("namespace Ast {");
                outputFile.WriteLine($"\tpublic abstract class {basename} {{");
                DefineVisitor(outputFile, basename, types);
                outputFile.WriteLine("\t\tpublic abstract T Accept<T>(IVisitor<T> visitor);");
                foreach (string type in types)
                {
                    string className = type.Split(':')[0].Trim();
                    string fields = type.Split(':')[1].Trim();
                    DefineType(outputFile, basename, className, fields);
                }
                outputFile.WriteLine("\t}");
                outputFile.WriteLine("}");
            };
        }

        static void DefineVisitor(StreamWriter writer, string basename, List<string> types)
        {
            writer.WriteLine($"\t\tpublic interface IVisitor<T> {{");
            foreach (string type in types)
            {
                string name = type.Split(':')[0].Trim();
                writer.WriteLine($"\t\t\tpublic T Visit({name} {basename.ToLower()});");
            }
            writer.WriteLine("\t\t}");
        }

        static void DefineType(StreamWriter writer, string basename, string className, string fields)
        {
            writer.WriteLine($"\t\tpublic class {className}({fields}) : {basename} {{");
            string[] fieldList = fields.Split(", ");
            foreach (string field in fieldList)
            {
                string[] typename = field.Split(' ');
                writer.WriteLine($"\t\t\tpublic {typename[0]} {typename[1]} = {typename[1]};");
            }
            writer.WriteLine("\t\t\tpublic override T Accept<T>(IVisitor<T> visitor) {");
            writer.WriteLine("\t\t\t\treturn visitor.Visit(this);");
            writer.WriteLine("\t\t\t}");
            writer.WriteLine("\t\t}");
        }
    }
}