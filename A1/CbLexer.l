%namespace LexScanner
%tokentype Tokens

%{
  public override void yyerror( string msg, params object[] args ) {
    Console.WriteLine("{0}: ", yyline);
    if (args == null || args.Length == 0)
      Console.WriteLine("{0}", msg);
    else
      Console.WriteLine(msg, args);
  }

  public void yyerror( int lineNum, string msg, params object[] args ) {
    Console.WriteLine("{0}: {1}", msg, args);
  }

  public int LineNumber { get{return yyline;} }    
%}

nl [\n]
space [ \r\t]
opchar [+\-*/\=\!\>] // must escape - as it signifies a range
parenthesis [\{\}\(\)]
symbol [\;\:\.\<\[\]\'\'\"]



%%
([/][*])([^/]|[\n])*([*][/]) {
    line_count += yytext.Split('\n').Length;
}
([/][*])(?>(([/][*])(?<Depth>))|(([*][/])(?<-Depth>))|((.|[\n])?))*([*][/]) {}

([/][/].*[\n]) {}

{nl}                    {line_count++;}
{space}                 {}
{symbol}                {return (int)(yytext[0]);}
{parenthesis}           {return (int)(yytext[0]);}

(!=)               {last_token_text=yytext;return (int)Tokens.NOTEQ;}
(++)               {last_token_text=yytext;return (int)Tokens.PLUSPLUS;}
(==)               {last_token_text=yytext;return (int)Tokens.EQEQ;}
(>=)               {last_token_text=yytext;return (int)Tokens.LTEQ;}
(<=)               {last_token_text=yytext;return (int)Tokens.GTEQ;}
(&&)               {last_token_text=yytext;return (int)Tokens.ANDAND;}


break {last_token_text=yytext; return (int)Tokens.Kwd_break;}
class {last_token_text=yytext; return (int)Tokens.Kwd_class;}
const {last_token_text=yytext; return (int)Tokens.Kwd_const;}
else  {last_token_text=yytext; return (int)Tokens.Kwd_else;}
if {last_token_text=yytext; return (int)Tokens.Kwd_if;}
int {last_token_text=yytext; return (int)Tokens.Kwd_int;}

new {last_token_text=yytext; return (int)Tokens.Kwd_new;}
out {last_token_text=yytext; return (int)Tokens.Kwd_out;}
public {last_token_text=yytext; return (int)Tokens.Kwd_public;}
return {last_token_text=yytext; return (int)Tokens.Kwd_return;}
static {last_token_text=yytext; return (int)Tokens.Kwd_static;}
virtual {last_token_text=yytext; return (int)Tokens.Kwd_virtual;}
override {last_token_text=yytext; return (int)Tokens.Kwd_override;}
void {last_token_text=yytext; return (int)Tokens.Kwd_void;}


using {last_token_text=yytext; return (int)Tokens.Kwd_using;}
while {last_token_text=yytext; return (int)Tokens.Kwd_while;}

null {last_token_text=yytext; return (int)Tokens.Kwd_null;}


(')([a-zA-Z0-9 ])(') {last_token_text=yytext;return (int)Tokens.Char;}
\"([^\\\"]|\\.)*\"  {last_token_text=yytext;return (int)Tokens.StringConst;}
0|[1-9][0-9]*         {last_token_text=yytext;return (int)Tokens.Number;}
[a-zA-Z][0-9a-zA-Z]*  {last_token_text=yytext;return (int)Tokens.Ident;}
{opchar}              {return (int)(yytext[0]);}


.                     { yyerror("illegal character ({0})", yytext); }




%%

public string last_token_text = "";
public int line_count = 0; 

