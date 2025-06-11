grammar PaperScript;

options {
    language = CSharp;
}

@namespace { PaperScript.Compiler.Antlr }

@header {
using System.Collections.Generic;
}

file
    : directive* includeDirective* scriptDecl
    ;

directive
    : HASH_DEFINE IDENTIFIER STRING?
    ;

includeDirective
    : HASH_INCLUDE STRING?
    ;

scriptDecl
    : scriptFlag* 'script' IDENTIFIER ':' IDENTIFIER '{' scriptBody '}'
    ;

scriptBody
    : statement* 
    ;

statement
    : autoProperty
    | property
    | functionDecl
    | variableDecl
    | eventDecl
    ;

autoProperty
    : 'auto' propertyModifier? 'property' IDENTIFIER ':' type ( '=' expr )?
    ;

property
    : propertyModifier? 'property' IDENTIFIER ':' type propertyBlock
    ;

propertyBlock
    : '{' getterBlock? setterBlock? '}'
    ;
    
getterBlock
    : 'get' block
    ;
    
setterBlock
    : 'set' block
    ;

variableDecl
    : variableFlag? IDENTIFIER ':' type ( '=' expr )?
    ;

functionDecl
    : functionFlag* 'def' IDENTIFIER '(' paramList? ')' ( '->' type )? block
    ;

paramList
    : param (',' param)* 
    ;

param
    : IDENTIFIER ':' type ( '=' expr )? 
    ;

block
    : '{' statement* stmtBody* '}' 
    ;

stmtBody
    : whileStmt
    | rangeStmt
    | switchStmt
    | ifStmt
    | exprStmt
    | returnStmt
    | variableDecl
    | eventDecl
    | conditionalBlock
    ;
    
conditionalBlock 
    : directiveStart stmtBody* directiveEnd 
    ;

directiveStart 
    : HASH_IF IDENTIFIER 
    ;

directiveEnd
    : HASH_ENDIF
    ;

ifStmt
    : 'if' expr block ( 'else' block )? 
    ;

switchStmt
    : 'switch' expr switchBlock
    ;

switchBlock
    : '{' switchCase* switchDefaultCase? '}'
    ;
    
switchCase
    : 'case' expr '=>' expr ';' # singleLineCase
    | 'case' expr '=>' block # multiLineCase
    ;
    
switchDefaultCase
    : 'default' '=>' expr ';' # singleLineDefault
    | 'default' '=>' block # multiLineDefault
    ;

whileStmt
    : 'while' expr block 
    ;

rangeStmt
    : 'range' IDENTIFIER 'in' IDENTIFIER block 
    ;

returnStmt
    : 'return' expr 
    ;

exprStmt
    : expr 
    ;

expr
    : 'new' type '[' expr ']'                   # newArrayExpr
    | expr '[' expr ']'                         # indexExpr
    | expr '=' expr                             # assignmentExpr
    | <assoc=right> expr op=('*' | '/' ) expr   # MulDivExpr
    | <assoc=right> expr op=('+' | '-' ) expr   # AddSubExpr
    | <assoc=right> expr op=('==' | '!=' | '<' | '<=' | '>' | '>=' | '&&' | '||' ) expr # CompareExpr
    | functionCallExpr                          # FuncCall
    | qualifiedNameExpr                         # QualName
    | literalExpr                               # Lit
    | '(' expr ')'                              # parenExpr
    | '!' expr                                  # unaryNotExpr
    ;

literalExpr
    : literal
    ;

functionCallExpr
    : qualifiedName '(' argList? ')'
    ;

qualifiedNameExpr
    : qualifiedName
    ;

eventDecl
    : 'event' IDENTIFIER '(' paramList? ')' block
    ;

literal
    : STRING
    | BOOL
    | FLOAT
    | INT
    ;

argList
    : expr (',' expr)* 
    ;

qualifiedName
    : IDENTIFIER ('.' IDENTIFIER)* 
    ;

type
    : IDENTIFIER arrayModifier? 
    ;

arrayModifier
    : '[' ']' 
    ;

variableFlag
    : 'conditional' 
    ;

propertyModifier
    : 'readonly' 
    | 'conditional'
    | 'hidden'
    ;

scriptFlag
    : 'hidden'
    | 'conditional'
    ;

functionFlag
    : 'native'
    | 'global'
    ;

BOOL
    : 'true' | 'false' 
    ;

FLOAT
    : DIGIT+ '.' DIGIT* 
    ;

INT
    : DIGIT+ 
    ;

STRING
    : '"' (~["\\] | '\\' .)* '"' 
    ;

IDENTIFIER
    : [a-zA-Z_] [a-zA-Z0-9_]* 
    ;

HASH_DEFINE
    : '#define' 
    ;
    
HASH_IF
    : '#if'
    ;

HASH_ENDIF
    : '#endif'
    ;
    
HASH_INCLUDE
    : '#include'
    ;

WS
    : [ \t\r\n]+ -> skip 
    ;

COMMENT
    : '//' ~[\r\n]* -> skip 
    ;

fragment DIGIT
    : [0-9] 
    ;