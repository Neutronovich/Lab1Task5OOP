grammar Expr;



compileUnit : expression EOF ;

expression
    : LPAREN expression RPAREN                       # ParenExpr
    | op=(PLUS | MINUS) expression                   # UnaryExpr
    | expression op=(EQ | LT | GT) expression        # CompareExpr
    | NOT expression                                 # NotExpr
    | expression AND expression                      # AndExpr
    | expression OR expression                       # OrExpr
    | expression EQV expression                      # EqvExpr
    | NUMBER                                         # NumberExpr
    | CELL                                           # CellExpr
    | (TRUE | FALSE)                                 # BoolExpr
    ;

/* Ключові слова (регістр не важливий) */
NOT   : [Nn][Oo][Tt] ;
AND   : [Aa][Nn][Dd] ;
OR    : [Oo][Rr] ;
EQV   : [Ee][Qq][Vv] ;
TRUE  : [Tt][Rr][Uu][Ee] ;
FALSE : [Ff][Aa][Ll][Ss][Ee] ;

PLUS   : '+' ;
MINUS  : '-' ;
EQ     : '=' ;
LT     : '<' ;
GT     : '>' ;
LPAREN : '(' ;
RPAREN : ')' ;

NUMBER : [0-9]+ ;                      
CELL   : [A-Za-z]+ [1-9] [0-9]* ;       

WS : [ \t\r\n]+ -> skip ;
