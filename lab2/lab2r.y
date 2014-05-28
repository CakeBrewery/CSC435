%using System.IO;

// Token declarations
%token NUM
%token IDENT
%token ASSIGN

%namespace LexScanner 

/* operator precedences & associativities in expressions */

%right '='
%left '-' '+'
%left '*' '/'
%left UMINUS
%right '^'

// Start parsing at rule Statement
%start StatementList

%{
  public void yyerror( string format, params Object[] args ) {
    Console.Write("{0}: ", 99);//LineNumber);
    Console.WriteLine(format, args);
  }
%}

%%

StatementList:
      StatementList Statement '\n' {writeln(";----");}
    | 
    ;
Statement:
      Expr
    |
    ;
Expr:
      Expr '+' Expr     {writeln("add");}
    | Expr '-' Expr     {writeln("sub");}
    | Expr '*' Expr     {writeln("mul");}
    | Expr '/' Expr     {writeln("div");}
    | Factor
    ;
Factor:
      Atom
    ;
Atom: NUM               {writeln("ldc",token_text());}
    | Ident
    ;
Ident:
      IDENT             {push_id();writeln("ldi",pop_id());} 
    ;

%%

public Stack<string> id_stack = new Stack<string>();

public void push_id() {
  string t = ((LexScanner.Scanner)Scanner).last_token_text;
  id_stack.Push(t);
}
public string pop_id() {
  return id_stack.Pop();
}

public string token_text() {
  return ((LexScanner.Scanner)Scanner).last_token_text;
}

public void writeln() {
  writeln(null,null);
}
public void writeln(string opcode) {
  writeln(opcode,null);
}

public void writeln(string opcode, string value) {
  if (opcode != null) {
    System.Console.Write(opcode);
    if (value != null) {
      System.Console.Write(' '+value);
    }
  }
  System.Console.Write('\n');
}

public void openFile(FileStream file){
   this.Scanner = new LexScanner.Scanner(file);
}

// The parser needs a constructor
public Parser() : base(null) { }




