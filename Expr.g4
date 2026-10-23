grammar Expr;

/*
 * Варіант 69: унарні +/-; =, <, >; not; and, or; eqv.
 * У ANTLR4 пріоритет задається порядком альтернатив: чим вище альтернатива, тим вищий пріоритет.
 * Від вищого до нижчого: дужки / унарні +,- / = < > / not / and / or / eqv.
 */

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

NUMBER : [0-9]+ ;                       // ціле число довільної довжини
CELL   : [A-Za-z]+ [1-9] [0-9]* ;       // ім'я клітинки: A1, B12, AA3

WS : [ \t\r\n]+ -> skip ;
