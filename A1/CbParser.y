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
        |	DeclList FieldDecl
        |       /* empty */
        ;

ConstDecl:      Kwd_public Kwd_const Type IDENT '=' InitVal ';'
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

MethodDecl:     Kwd_public MethodType ReturnValue Ident '(' OptFormals ')' Block
        ;
        
MethodType:	Kwd_static
	|	Kwd_virtual
	|	Kwd_override
	;

ReturnValue:	Kwd_void
	|	Type
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
        |       Kwd_if '(' Condition ')' Statement OptElsePart
        |       Kwd_while '(' Condition ')' Statement
        |       Designator '(' OptActuals ')' ';'
        |       Designator PLUSPLUS ';'
        |       Designator MINUSMINUS ';'
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
        
Condition:	CondTerm OROR CondTerm
	|	CondTerm
	;
	
CondTerm:	CondFact ANDAND CondFact
	|	CondFact
	;

CondFact:	Expr EqOp Expr
	|	Expr RelOp Expr
	;
	
EqFact:		Expr RelOp Expr
	;		

OptElsePart:    Kwd_else Statement
        |       /* empty */
        ;

Block:          '{' DeclsAndStmts '}'
        ;

LocalDecl:      Type IdentList
        ;

DeclsAndStmts:   /* empty */
        |       DeclsAndStmts Statement
        |       DeclsAndStmts LocalDecl
        ;

Expr:           Addop Term Expr
	|	Addop Term
	|	Term
	|	Term Expr
	;

Addop:		'+'
	|	'-'
	;
	
Term:		Factor	Mulop Factor
	|	Factor
	;
	
Mulop:		'*'
	|	'/'
	|	'%'
	;
	
Factor:		Designator ActParsOp
	|	Number
	|	Char
	|	StringConst
	|	StringConst '.' IDENT
	|	Kwd_new IDENT '[' Expr ']'
	|	Kwd_new IDENT '(' ')'
	|	Kwd_null
	|	'(' Type ')' Factor
	|	'(' Expr ')'
	;


ActParsOp:	'(' ')'
	|	'(' ActPars ')'
	|	/*empty*/
	;

Designator:     Ident Qualifiers
        ;

Qualifiers:     '.' Ident Qualifiers
        |       '[' Expr ']' Qualifiers
        |       /* empty */
        ;
        
EqOp:		EQEQ
	|	NOTEQ
	;
	
RelOp:		'>'
	|	'<'
	|	LTEQ
	|	GTEQ
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
