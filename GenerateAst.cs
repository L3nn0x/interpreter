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
                "Literal : object Value",
                "Unary   : Token Op, Expr Right"
            ]);
        }

        public static void DefineAst(string output, string basename, List<string> types)
        {
            string filename = Path.Join(output, basename + ".cs");
            using (StreamWriter outputFile = new StreamWriter(filename, false))
            {
                outputFile.WriteLine("using interpreter;");
                outputFile.WriteLine("namespace Ast {");
                outputFile.WriteLine($"\tpublic abstract class {basename} {{");
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

        public static void DefineType(StreamWriter writer, string basename, string className, string fields)
        {
            writer.WriteLine($"\t\tclass {className}({fields}) : {basename} {{");
            string[] fieldList = fields.Split(", ");
            foreach (string field in fieldList)
            {
                string[] typename = field.Split(' ');
                writer.WriteLine($"\t\t\tpublic {typename[0]} {typename[1]} = {typename[1]};");
            }
            writer.WriteLine("\t\t}");
        }
    }
}