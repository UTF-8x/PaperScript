grammar PaperScript;

options {
    language = CSharp;
}

@namespace { PaperScript.Compiler.Antlr }

@header {
using System.Collections.Generic;
}

// ---------- Top-Level Rules ----------

script: 'script' IDENTIFIER ':' IDENTIFIER '{' scriptBody '}' ;

scriptBody: statement* ;

statement
    : autoProperty
    | functionDecl
    | variableDecl
    | eventDecl
    ;

// ---------- Declarations ----------

autoProperty
    : 'auto' 'property' IDENTIFIER ':' type ( '=' expr )?
    ;

variableDecl
    : visibility? IDENTIFIER ':' type ( '=' expr )?
    ;

functionDecl
    : 'def' IDENTIFIER '(' paramList? ')' ( '->' type )? block
    ;

paramList: param (',' param)* ;
param: IDENTIFIER ':' type ( '=' expr )? ;

// ---------- Blocks and Control Flow ----------

block: '{' statement* stmtBody* '}' ;

stmtBody
    : ifStmt
    | whileStmt
    | rangeStmt
    | exprStmt
    | returnStmt
    | variableDecl
    | eventDecl
    ;

ifStmt: 'if' expr block ( 'else' block )? ;
whileStmt: 'while' expr block ;
rangeStmt: 'range' IDENTIFIER 'in' IDENTIFIER block ;
returnStmt: 'return' expr ;
exprStmt: expr ;

// ---------- Expressions ----------

expr
    : 'new' type '[' expr ']'                         # newArrayExpr
    | expr '[' expr ']'                 # indexExpr
    | expr '=' expr # assignmentExpr
    | <assoc=right> expr op=('*' | '/' ) expr     # MulDivExpr
    | <assoc=right> expr op=('+' | '-' ) expr     # AddSubExpr
    | <assoc=right> expr op=('==' | '!=' | '<' | '<=' | '>' | '>=' ) expr # CompareExpr
    | functionCallExpr # FuncCall
    | qualifiedNameExpr # QualName
    | literalExpr # Lit
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


argList: expr (',' expr)* ;
qualifiedName: IDENTIFIER ('.' IDENTIFIER)* ;

binaryOp: '==' | '!=' | '<' | '<=' | '>' | '>=' | '+' | '-' | '*' | '/' ;

// ---------- Types & Modifiers ----------

type: IDENTIFIER arrayModifier? ;
arrayModifier: '[' ']' ;
visibility: 'private' | 'public' ;

// ---------- Terminals ----------

BOOL: 'true' | 'false' ;
FLOAT: DIGIT+ '.' DIGIT* ;
INT: DIGIT+ ;
STRING: '"' (~["\\] | '\\' .)* '"' ;
IDENTIFIER: [a-zA-Z_] [a-zA-Z0-9_]* ;

// ---------- Helpers ----------

fragment DIGIT: [0-9] ;

WS: [ \t\r\n]+ -> skip ;
COMMENT: '//' ~[\r\n]* -> skip ;