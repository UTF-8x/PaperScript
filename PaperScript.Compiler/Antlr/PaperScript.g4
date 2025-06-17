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
    
stateDecl
    : stateFlag? 'state' IDENTIFIER stateBlock
    ;

stateBlock
    : '{' statement* '}'
    ;
    
stateFlag
    : 'auto'
    ;
    
groupDecl
    : groupFlag* 'group' IDENTIFIER groupBlock
    ;

groupBlock
    : '{' groupStatement* '}'
    ;

groupFlag
    : 'collapsedOnRef'
    | 'collapsedOnBase'
    | 'collapsed'
    ;
    
structDecl
    : 'struct' IDENTIFIER structBlock
    ;

structBlock
    : '{' variableDecl* '}'
    ;

groupStatement
    : variableDecl
    | property
    | autoProperty
    ;

scriptBody
    : statement* stateDecl*
    ;

importStatement
    : 'import' STRING
    ;

statement
    : autoProperty
    | importStatement
    | property
    | functionDecl
    | variableDecl
    | inferredVariableDecl
    | eventDecl
    | groupDecl
    | structDecl
    ;

autoProperty
    : mandatoryModifier? constModifier? 'auto' propertyModifier? 'property' IDENTIFIER ':' type ( '=' expr )?
    ;

constModifier
    : 'const'
    ;

mandatoryModifier
    : 'mandatory'
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
    : constModifier? variableFlag? IDENTIFIER ':' type ( '=' expr )?
    ;

inferredVariableDecl
    : constModifier? variableFlag? 'var' IDENTIFIER ( '=' expr )?
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
    | voidReturnStmt
    | variableDecl
    | eventDecl
    | conditionalBlock
    | incrementDecrement
    ;

incrementDecrement
    : IDENTIFIER INCREMENT
    | IDENTIFIER DECREMENT
    ;
 
INCREMENT : '++' ;
DECREMENT : '--' ;
    
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
    : 'if' expr block elseIfBlock* ( 'else' block )? 
    ;

elseIfBlock
    : 'elseif' expr block
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
 
voidReturnStmt
    : 'return'
    ;

exprStmt
    : expr 
    ;

expr
    : '[' literal (',' literal)* ']'            # arrayInit
    | '{' structInitAssignment (',' structInitAssignment)* '}' # structInit
    | expr 'as' type                            # castExpr
    | expr 'is' expr                            # isExpr
    | 'new' type                                # newStruct
    | 'new' type '[' expr ']'                   # newArrayExpr
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
 
structInitAssignment
    : IDENTIFIER ':' literal
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
    | 'debug'
    | 'beta'
    | 'const'
    | 'native'
    ;

functionFlag
    : 'native'
    | 'global'
    ;

BOOL
    : 'true' | 'false' 
    ;

FLOAT
    : MINUS? DIGIT+ '.' DIGIT* 
    ;

INT
    : MINUS? DIGIT+ 
    ;

MINUS
    : '-'
    ;

STRING
    : '"' (~["\\] | '\\' .)* '"' 
    ;

IDENTIFIER
    : [a-zA-Z_] [a-zA-Z0-9_]* ('::' [a-zA-Z_][a-zA-Z0-9_]*)*
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

MULTILINE_COMMENT
    : '/*' .*? '*/' -> skip
    ;

DOC_COMMENT
    : '/**' .*? '*/' -> skip
    ;
   

fragment DIGIT
    : [0-9] 
    ;