%using System.IO;
/* CbParser.y */

// The grammar shown in this file is INCOMPLETE!!
// It does not support class inheritance, it does not permit
// classes to contain methods (other than Main).
// Other language features may be missing too.

%tokentype  Tokens

// All tokens which can be used as operators in expressions
// they are ordered by precedence level (lowest first)
%right      '='
%left       OROR
%left       ANDAND
%nonassoc   EQEQ NOTEQ
%nonassoc   '>' GTEQ '<' LTEQ
%left       '+' '-'
%left       '*' '/' '%'
%left       UMINUS

// All other named tokens (i.e. the single character tokens are omitted)
// The order in which they are listed here does not matter.
%token      Kwd_break Kwd_class Kwd_const Kwd_else Kwd_if Kwd_int
%token      Kwd_new Kwd_out Kwd_public Kwd_return Kwd_static Kwd_string
%token      Kwd_using Kwd_void Kwd_while
%token      PLUSPLUS MINUSMINUS Ident Number StringConst

//Added tokens
%token      Kwd_null Kwd_virtual Kwd_override Char

%%

/* *************************************************************************
   *                                                                       *
   *         PRODUCTION RULES AND ASSOCIATED SEMANTIC ACTIONS              *
   *                                                                       *
   ************************************************************************* */

Program:        UsingList ClassList
        ;

UsingList:      /* empty */
        |       Kwd_using IDENT ';' UsingList
        ;

ClassList:	    ClassList ClassDecl
		|		ClassDecl
		;

ClassDecl:		Kwd_class Ident '{'  DeclList  '}'
        |       Kwd_class Ident ':' Ident '{' DeclList '}'
		;

DeclList:       DeclList ConstDecl
        |       DeclList MethodDecl
        |       /* empty */
        ;

ConstDecl:      Kwd_public Kwd_const Type IDENT '=' InitVal ';'
        |       Kwd_public Type IDENT ';'
        ;

InitVal:        Number
        |       StringConst
        ;

FieldDeclList:  FieldDeclList FieldDecl
        |       /* empty */
        ;

FieldDecl:      Kwd_public Type IdentList ';'
        |       Type IdentList ';'
        ;

IdentList:      IdentList ',' IDENT 
        |       IDENT 
        ;

MethodDecl:     Kwd_public Kwd_static Type Ident '(' OptFormals ')' Block
        |       Kwd_public Kwd_virtual Type Ident '(' OptFormals ')' Block
        |       Kwd_public Kwd_override Type Ident '(' OptFormals ')' Block
        |       Ident '.' MethodDecl
        ;

OptFormals:     /* empty */
        |       FormalPars
        ;

FormalPars:     FormalDecl
        |       FormalPars ',' FormalDecl
        ;

FormalDecl:     Type Ident
        ;

Type:           TypeName
        |       TypeName '[' ']'
        ;

TypeName:       Ident
        |       Kwd_int
        |       Kwd_string
        |       Kwd_void
        ;

Statement:      Designator '=' Expr ';'
        |       Designator '(' OptActuals ')' ';'
        |       Designator PLUSPLUS ';'
        |       Designator MINUSMINUS ';'
        |       Kwd_if '(' Expr ')' Statement OptElsePart
        |       Kwd_while '(' Expr ')' Statement
        |       Kwd_break ';'
        |       Kwd_return ';'
        |       Kwd_return Expr ';'
        |       Block
        |       ';'
        ;

OptActuals:     /* empty */
        |       ActPars
        ;

ActPars:        ActPars ',' Expr
        |       Expr
        ;

OptElsePart:    Kwd_else Statement
        |       /* empty */
        ;

Block:          '{' DeclsAndStmts '}'
        ;

LocalDecl:      Ident IdentList ';'
        |       Type IdentList
        |       Ident '[' ']' IdentList ';'
        ;

DeclsAndStmts:   /* empty */
        |       DeclsAndStmts Statement
        |       DeclsAndStmts LocalDecl
        ;

Expr:           Expr OROR Expr
        |       Expr ANDAND Expr
        |       Expr EQEQ Expr
        |       Expr NOTEQ Expr
        |       Expr LTEQ Expr
        |       Expr '<' Expr
        |       Expr GTEQ Expr
        |       Expr '>' Expr
        |       Expr '+' Expr
        |       Expr '-' Expr
        |       Expr '*' Expr
        |       Expr '/' Expr
        |       Expr '%' Expr
        |       '-' Expr %prec UMINUS
        |       Designator
        |       Designator '(' OptActuals ')'
        |       Number
        |       Char
        |       StringConst
        |       StringConst '.' Ident // Ident must be "Length"
        |       Kwd_new Ident '(' ')'
        |       Kwd_new Ident '[' Expr ']'
        |       '(' Expr ')'
        |       Kwd_null
        ;

Designator:     Ident Qualifiers
        ;

Qualifiers:     '.' Ident Qualifiers
        |       '[' Expr ']' Qualifiers
        |       /* empty */
        ;

IDENT:
    Ident {if(flg_token) {
                push_id();writeln("Token.Ident, text = ", pop_id());
           };
          } 
    ;

%%



//The following is how this was done in the lab

//Flags
public Boolean flg_debug; 
public Boolean flg_token; 


Boolean hasMain = false; 

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
