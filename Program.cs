using System.IO;

namespace interpreter
{
    class Program
    {
        static bool HadError = false;

        static private Interpreter interpreter = new();

        static private Resolver resolver = new(interpreter);

        static void Main(string[] args)
        {
            if (args.Length > 2)
            {
                Console.WriteLine("Usage: interpreter [script]");
                System.Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }

        static void RunFile(string filename)
        {
            StreamReader reader = new(filename);
            string contents = reader.ReadToEnd();
            reader.Close();
            Run(contents);
            if (HadError)
            {
                System.Environment.Exit(65);
            }
        }

        static void RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                string? line = Console.ReadLine();
                if (line == null)
                {
                    break;
                }
                Run(line!);
                HadError = false;
            }
        }

        static public void Error(Token token, string message)
        {
            if (token.type == TokenType.EOF)
            {
                Report(token.line, " at end", message);
            }
            else
            {
                Report(token.line, $"at '{token.lexeme}'", message);
            }
        }

        static public void Error(int line, string message)
        {
            Report(line, "", message);
        }

        static void Report(int line, string where, string message)
        {
            Console.WriteLine($"[line {line}] Error {where}: {message}");
            HadError = true;
        }

        static void Run(string source)
        {
            Scanner scanner = new(source);
            List<Token> tokens = scanner.ScanTokens();
            /*foreach (Token t in tokens)
            {
                Console.WriteLine(t.ToString());
            }*/
            Parser parser = new(tokens);
            List<Ast.Stmt?> statements;
            try
            {
                statements = parser.Parse();
            }
            catch (ParseError)
            {
                return;
            }
            if (HadError)
            {
                return;
            }
            resolver.Resolve(statements);
            if (HadError)
            {
                return;
            }
            /*AstPrinter printer = new();
            foreach (var s in statements)
            {
                Console.WriteLine(printer.Print(s!));
            }*/
            try
            {
                Option value = interpreter.Interpret(statements);
                if (value.GetType() == typeof(Some))
                {
                    Console.WriteLine(((Some)value).Content);
                }
            }
            catch (RuntimeException)
            {
                return;
            }
        }
    }
}