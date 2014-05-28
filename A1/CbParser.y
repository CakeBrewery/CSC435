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

Program:        UsingList ClassList //Correct
        ;

UsingList:      /* empty */
        |       KWD_USING IDENT ';' UsingList
        ;

ClassList:	ClassList ClassDecl
	|	ClassDecl
	;

ClassDecl:	KWD_CLASS IDENT '{'  DeclList  '}' //Correct
        |       KWD_CLASS IDENT ':' IDENT '{' DeclList '}'
		;

DeclList:       DeclList ConstDecl
        |       DeclList MethodDecl
        |	DeclList FieldDecl
        |       /* empty */
        ;

ConstDecl:      KWD_PUBLIC KWD_CONST Type IDENT '=' InitVal ';' //correct
        ;

InitVal:        Number
        |       StringConst
        ;

FieldDeclList:  FieldDeclList FieldDecl
        |       /* empty */
        ;

FieldDecl:      KWD_PUBLIC Type IdentList ';' //correct
        ;

IdentList:      IdentList ',' IDENT 
        |       IDENT 
        ;

MethodDecl:     KWD_PUBLIC MethodType ReturnValue IDENT '(' OptFormals ')' Block //correct
        ;
        
MethodType:	KWD_STATIC
	|	KWD_VIRTUAL
	|	KWD_OVERRIDE
	;

ReturnValue:	KWD_VOID
	|	Type
	;

OptFormals:     /* empty */
        |       FormalPars
        ;

FormalPars:     FormalDecl //correct
        |       FormalPars ',' FormalDecl
        ;

FormalDecl:     Type IDENT //correct
        ;

Type:           TypeName //correct
        |       TypeName '[' ']'
        ;

TypeName:       IDENT
        |       KWD_INT
        |       KWD_STRING
        |       KWD_VOID
        ;

Statement:      Designator '=' Expr ';' //correct
        |       KWD_IF '(' Condition ')' Statement OptElsePart
        |       KWD_WHILE '(' Condition ')' Statement
        |       Designator '(' OptActuals ')' ';'
        |       Designator PLUSPLUS ';'
        |       Designator MINUSMINUS ';'
        |       KWD_BREAK ';'
        |       KWD_RETURN ';'
        |       KWD_RETURN Expr ';'
        |       Block
        |       ';'
        ;

OptActuals:     /* empty */
        |       ActPars
        ;

ActPars:        ActPars ',' Expr //correct
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

OptElsePart:    KWD_ELSE Statement
        |       /* empty */
        ;

Block:          '{' DeclsAndStmts '}' //correct
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
	|	KWD_NEW IDENT '[' Expr ']'
	|	KWD_NEW IDENT '(' ')'
	|	KWD_NULL
	|	'(' Type ')' Factor
	|	'(' Expr ')'
	;


ActParsOp:	'(' ')'
	|	'(' ActPars ')'
	|	/*empty*/
	;

Designator:     IDENT Qualifiers
        ;

Qualifiers:     '.' IDENT Qualifiers
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
    
KWD_BREAK:
    Kwd_break {if(flg_token) {
                push_id();writeln("Token.Kwd_break, text = ", pop_id());
           };
          } 
    ;
    
KWD_CLASS:
    Kwd_class {if(flg_token) {
                push_id();writeln("Token.Kwd_class, text = ", pop_id());
           };
          } 
    ;
    
KWD_CONST:
    Kwd_const {if(flg_token) {
                push_id();writeln("Token.Kwd_const, text = ", pop_id());
           };
          } 
    ;
    
KWD_ELSE:
    Kwd_else {if(flg_token) {
                push_id();writeln("Token.Kwd_else, text = ", pop_id());
           };
          } 
    ;
    
KWD_IF:
    Kwd_if {if(flg_token) {
                push_id();writeln("Token.Kwd_if, text = ", pop_id());
           };
          } 
    ;
    
KWD_NULL:
    Kwd_null {if(flg_token) {
                push_id();writeln("Token.Kwd_null, text = ", pop_id());
           };
          } 
    ;
    
KWD_NEW:
    Kwd_new {if(flg_token) {
                push_id();writeln("Token.Kwd_new, text = ", pop_id());
           };
          } 
    ;
    
KWD_RETURN:
    Kwd_return {if(flg_token) {
                push_id();writeln("Token.Kwd_return, text = ", pop_id());
           };
          } 
    ;
    
KWD_WHILE:
    Kwd_while {if(flg_token) {
                push_id();writeln("Token.Kwd_while, text = ", pop_id());
           };
          } 
    ;
    
KWD_STRING:
    Kwd_string {if(flg_token) {
                push_id();writeln("Token.Kwd_string, text = ", pop_id());
           };
          } 
    ;
    
KWD_INT:
    Kwd_int {if(flg_token) {
                push_id();writeln("Token.Kwd_int, text = ", pop_id());
           };
          } 
    ;
    
KWD_VOID:
    Kwd_void {if(flg_token) {
                push_id();writeln("Token.Kwd_void, text = ", pop_id());
           };
          } 
    ;
    
KWD_OVERRIDE:
    Kwd_override {if(flg_token) {
                push_id();writeln("Token.Kwd_override, text = ", pop_id());
           };
          } 
    ;
    
KWD_VIRTUAL:
    Kwd_virtual {if(flg_token) {
                push_id();writeln("Token.Kwd_virtual, text = ", pop_id());
           };
          } 
    ;
    
KWD_STATIC:
    Kwd_static {if(flg_token) {
                push_id();writeln("Token.Kwd_static, text = ", pop_id());
           };
          } 
    ;
    
KWD_PUBLIC:
    Kwd_public {if(flg_token) {
                push_id();writeln("Token.Kwd_public, text = ", pop_id());
           };
          } 
    ;
    
KWD_USING:
    Kwd_using {if(flg_token) {
                push_id();writeln("Token.Kwd_using, text = ", pop_id());
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
