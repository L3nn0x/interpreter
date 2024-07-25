from io import TextIOWrapper
import sys
from pathlib import Path

def define_visitor(fp: TextIOWrapper, basename: str, types: list[str]):
    fp.writelines([
        f"\t\tpublic interface IVisitor<T> {{\n"
    ])
    for type in types:
        name = type.split(":")[0].strip()
        fp.writelines([
            f"\t\t\tpublic T Visit({name} {basename.lower()});\n"
        ])
    fp.writelines(["\t\t}\n"])

def define_type(fp: TextIOWrapper, basename: str, class_name: str, fields: str):
    fp.writelines([f"\t\tpublic class {class_name}{'(' + fields + ')' if len(fields) else ''} : {basename} {{\n"])
    if len(fields):
        for field in fields.split(", "):
            typename = field.split(" ")
            fp.writelines([f"\t\t\tpublic {typename[0]} {typename[1]} = {typename[1]};\n"])
    fp.writelines([
        "\t\t\tpublic override T Accept<T>(IVisitor<T> visitor) {\n",
        "\t\t\t\treturn visitor.Visit(this);\n",
        "\t\t\t}\n",
        "\t\t}\n"
    ])

def define_ast(output: Path, basename: str, types: list[str]):
    filename = output.joinpath(basename + ".cs")
    with open(filename, "w") as fp:
        fp.writelines([
            "using interpreter;\n",
            "namespace Ast {\n",
            f"\tpublic abstract class {basename} {{\n",
        ])
        define_visitor(fp, basename, types)
        fp.write("\t\tpublic abstract T Accept<T>(IVisitor<T> visitor);\n")
        for type in types:
            class_name = type.split(":")[0].strip()
            fields = type.split(":")[1].strip()
            define_type(fp, basename, class_name, fields)
        fp.writelines([
            "\t}\n",
            "}\n"
        ])

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print(f"Usage {sys.argv[0]} <output_directory>")
        sys.exit(1)
    dir = Path(sys.argv[1])
    define_ast(dir, "Expr", [
        "Assign  : Token name, Expr value",
        "Binary  : Expr Left, Token Op, Expr Right",
        "Logical : Expr left, Token op, Expr right",
        "Grouping: Expr Expression",
        "Literal : object? Value",
        "Unary   : Token Op, Expr Right",
        "Variable: Token name"
    ])
    define_ast(dir, "Stmt", [
        "Block      : List<Stmt> statements",
        "Expression : Expr expression",
        "If         : Expr condition, Stmt then_branch, Stmt? else_branch",
        "Print      : Expr expression",
        "Var        : Token name, Expr? initializer",
        "While      : Expr condition, Stmt body, Expr? end_of_loop, Stmt? final",
        "Break      :",
        "Continue   :",
    ])