/* CbParser.y */

// The grammar shown in this file is INCOMPLETE!!
// It does not support class inheritance, it does not permit
// classes to contain methods (other than Main).
// Other language features may be missing too.

%namespace  FrontEnd
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

%%

/* *************************************************************************
   *                                                                       *
   *         PRODUCTION RULES AND ASSOCIATED SEMANTIC ACTIONS              *
   *                                                                       *
   ************************************************************************* */
  
Program:        { Kwd_using Ident ';' } ClassDecl { ClassDecl }
        ;

ClassDecl:	Kwd_class Ident [ ':' ident ] '{' { MemberDecl } '}'
	;
		
MemberDecl:	ConstDecl
	|	FieldDecl
	|	MethodDecl
	;
	
ConstDecl:      Kwd_public Kwd_static Kwd_const Type Ident '=' (number|stringConst) ';'
        ;
        
FieldDecl:	Kwd_public Type Ident { ',' Ident } ';'
	;
	
MethodDecl:	Kwd_public ( Kwd_static | Kwd_virtual | Kwd_override ) ( Kwd_void | Type ) Ident '(' {FormalPars} ')' Block
	;
	
LocalDecl:	Type Ident { ',' Ident } ';'
	;
	
FormalPars:	FormalDecl { ',' FormalDecl }
	;
	
FormalDecl:	Type Ident
	;
	
Type:		( Ident | Kwd_int | Kwd_string | Kwd_char ) [ '[' ']' ]
	;
	
Statement:	Designator '=' Expr ';'
	|	Kwd_if '(' Condition ')' Statement [ Kwd_else Statement ]
	|	Kwd_while '(' Condition ')' Statement
	|	Kwd_break ';'
	|	Kwd_return [ Expr ] ';'
	|	Designator '(' ActualPars ')' ';'
	|	Designator ( PLUSPLUS | MINUSMINUS ) ';'
	|	Block
	|	';'
	;
	
Block:		'{' { LocalDecl | Statement } '}'
	;
	
ActPars:	Expr { ',' Expr }
	;
	
Condition:	CondTerm { '||' CondTerm }
	;
	
CondTerm:	CondFact { '&&' CondFact }
	;
	
CondFact:	EqFact Eqop EqFact
	;

EqFact:		Expr Relop Expr
	;
	
Expr:		[ '+' | '-' ] Term { Addop Term }
	;
	
Term:		Factor { Mulop Factor }
	;
	
Factor:		Designator [ '(' [ ActPars ] ')' ]
	|	IntConst
	|	StringConst [ '.' Ident ]
	|	Kwd_new Ident '[' Expr ']'
	|	Kwd_new Ident '(' ')'
	|	Kwd_null
	|	'(' Type ')' Factor
	|	'(' Expr ')'
	;
	
Designator:	Ident { '.' Ident | '[' Expr ']' }
	;
	
EqOp:		'==' | '!='
	;
	
Relop:		'>' | '>=' | '<' | '<='
	;
	
Addop:		'+' | '-'
	;
	
Mulop:		'*' | '/' | '%'
	;


%%




