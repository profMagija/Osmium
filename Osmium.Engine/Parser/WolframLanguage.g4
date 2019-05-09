grammar WolframLanguage;

atomic : NUMBER						# AtomNumber
	   | SYMBOL						# AtomSymbol
	   | CHAR_STRING				# AtomCharString
	   | SLOTFORM					# AtomSlot
	   | OUTFORM					# AtomOut
	   | BLANKFORM					# AtomBlank
	   | GETFORM					# AtomGet
	   | '<<' atomic				# AtomGetExpr
	   | '(' expr ')'				# AtomParen
	   | XLEFT csexpr XRIGHT		# AtomMatchfix
	   ;

csexpr : ( expr (',' optexpr)* ) ?;
optexpr : expr? ;

messageName : atomic messageNamePart* ;
messageNamePart : '::' atomic
				;

patternTest : messageName ( '?' messageName )? ;

bracketed 
	: patternTest											# nothing0
	| bracketed '[' csexpr ']'								# bracketedFunctionApplication
	| bracketed '[' '[' csexpr ']' ']'						# bracketedPartPlain
	| bracketed LeftDoubleBracket csexpr RightDoubleBracket	# bracketedPartFancy
	// LeftDoubleBracket  = 0x301a
	// RightDoubleBracket = 0x301b
	;

postfixIncDec 
	: bracketed				# nothing1
	| postfixIncDec '++'	# increment
	| postfixIncDec '--'	# decrement
	;

prefixIncDec 
	: postfixIncDec			# nothing2
	| '++' prefixIncDec		# preIncrement
	| '--' prefixIncDec		# preDecrement
	;

composition 
	: prefixIncDec							# nothing30
	| prefixIncDec ( '@*' prefixIncDec )+	# compositionRule
	;

rightComposition 
	: composition						# nothing31
	| composition ( '/*' composition )+ # rightCompositionRule
	;


application 
	: rightComposition												# nothing3
	| rightComposition ( '@' | InvisibleApplication ) application	# applicationRule
	// InvisibleApplication = 0xf76d
	;

// the a~b~c == b[a, c] thing
magicInfix 
	: application									# nothing4
	| magicInfix '~' application '~' application	# infixApplication
	;

mapApply 
	: magicInfix							# nothing5
	| magicInfix mapApplyOperator mapApply	# mapApplyRule
	;

mapApplyOperator : '/@' | '//@' | '@@' | '@@@' ;

factorial 
	: mapApply			# nothing6
	| factorial '!'		# factorialRule
	| factorial '!!'	# factorial2Rule
	;

conjTransp 
	: factorial					# nothing7
	| conjTransp conjTranspOp	# conjTransposeRule
	// f3c8, f3c7, f3c9, f3ce
	;

conjTranspOp : Conjugate | Transpose | ConjugateTranspose | HermitianTranspose ;

derivative : conjTransp DERIV* ;

stringJoin : derivative ( '<>' derivative )* ;

power : stringJoin				# nothing8
	  | stringJoin '^' power	# powerRule
	  ;

// TODO: all the shit that comes inbetween

prefixPm : power											# nothing9
		  | ( '-' | '+' | PlusMinus | MinusPlus ) prefixPm	# prefixPlusMinus
		  // PlusMinus = 0x00b1
		  // MinusPlus = 0x2213
		  ;

division : prefixPm								# nothing10
		 | division ( '/' | Divide ) prefixPm	# divisionRule
		 // Divide = 0x00f7
		 ;

// TODO : more shit here

times : division ( ( '*' | Times ) division )* ;
// Times = 0x00d7

// TODO : even more shit


// note +- and -+ bind TIGHTER than + and - and left associative
// TODO: +- and -+

plusMinus 
	: times plusMinusPart+ 
	| times 
	;

plusMinusPart
	: '+' times		# nothing40
	| '-' times		# negateRule
	;

// TODO intersection, union

// TODO: span goes here

comparison : plusMinus ( compOperator plusMinus )* ; // TODO: combined

compOperator : '==' | '!=' | '>' | '>=' | '<' | '<=' ;

// TODO: bunch more

sameQ 
	: comparison						# nothing41
	| comparison ( '===' comparison )+	# sameQRule
	| sameQ '=!=' comparison			# unsameQRule
	;

// TODO: element, firstorder

nots : sameQ				# nothing11
     | ( '!' | Not ) nots	# notRule
	 // Not = 0x00ac
	 ;

ands : nots ( ( '&&' ) nots )* ;

// TODO: xors

ors : ands ( ( '||' ) ands )* ;

// TODO: equivalent, implies, tees, suchthat

repeated 
	: ors				# nothing12
	| repeated '..'		# repeatedRule
	| repeated '...'	# repeatedNullRule
	;

alternatives : repeated ( '|' repeated )* ;

pattern 
	: alternatives					# nothing42
	| SYMBOL ':' alternatives		# namedPattern
	| BLANKFORM ':' alternatives	# optionalWithDefault
	;

stringExpr : pattern ( '~~' pattern )* ;

condition 
	: stringExpr					# nothing13
	| condition '/;' stringExpr		# conditionRule
	;

twoWayRule 
	: condition						# nothing14
	| condition '<->' twoWayRule	# twoWayRuleRule
	;

rules 
	: twoWayRule				# nothing15
    | twoWayRule '->' rules		# ruleRule
	| twoWayRule ':>' rules		# delayedRuleRule
	;

replaceAll
	: rules						# nothing16
	| replaceAll '/.' rules		# replaceAllRule
	| replaceAll '//.' rules	# replaceRepeatedRule
	;

augSet 
	: replaceAll					# nothing17
    | replaceAll augSetOp augSet	# augmentedSet
	;

augSetOp : '+=' | '-=' | '*=' | '/=' ;

function 
	: augSet		# nothing18
	| augSet '&'	# functionRule
	;

// TODO: colon

postfixApply 
	: function						# nothing19
	| postfixApply '//' function	# postfixApplyRule
	;


set : postfixApply							# nothing20
	| postfixApply setOp set				# setSimple
	| SYMBOL '/:' postfixApply setOp2 set	# tagSet
	| postfixApply '=.'						# clearSimple
	| SYMBOL '/:' postfixApply '=.'			# tagClear
	| postfixApply Function set				# lambda
	// Function = 0xf4a1
	;

setOp  : '=' | ':=' | '^=' | '^:=' ;
setOp2 : '=' | ':=' ;

put : set							# nothing21
	| set (PUTFORM | APPENDFORM)	# putRule
	| put putOp set					# putExprRule
	;

putOp : '>>' | '>>>' ;

compoundExpr : put (';' putopt)* ;
putopt : put? ;

expr : compoundExpr ; 

/*
 * LEXER
 */

CHAR_STRING : '"' CHAR_STRING_COMPONENT+ '"' ;

DERIV : '\'' ;

fragment CHAR_STRING_COMPONENT
	: ( CHAR_STRING_ESCAPE | ~["] )*
	;

fragment CHAR_STRING_ESCAPE
	: '\\' .
	;

SYMBOL
	: SYMBOL_NAME
	| '`' SYMBOL_NAME
	| CONTEXT_NAME SYMBOL_NAME
	| '`' CONTEXT_NAME SYMBOL_NAME
	;

fragment SYMBOL_NAME
	: LETTER (LETTER | DIGIT)*
	;
	
fragment CONTEXT_NAME 
	: CONTEXT_PART+
	;

fragment CONTEXT_PART
	: LETTER (LETTER | DIGIT)* '`'
	;

// TODO: add all Letters and Letter-Like Forms here
fragment LETTER
	: [A-Z] 
	| [a-z] 
	| '$'
	;

fragment DIGIT
	: [0-9]
	;

NUMBER
	: DECIMAL_NUM
	| INTEGER '^^' ANYBASE_NUM
	| DECIMAL_NUM '*^' EXPONENT
	| INTEGER '^^' ANYBASE_NUM '*^' EXPONENT
	;

fragment DECIMAL_NUM
	: DECIMAL_DIGITS
	| DECIMAL_DIGITS '`'
	| DECIMAL_DIGITS '`' INTEGER
	| DECIMAL_DIGITS '``' INTEGER
	;

fragment ANYBASE_NUM
	: ANYBASE_DIGITS
	| ANYBASE_DIGITS '`'
	| ANYBASE_DIGITS '`' INTEGER
	| ANYBASE_DIGITS '``' INTEGER
	;

fragment DECIMAL_DIGITS
	: DECIMAL_DIGIT+ ( '.' DECIMAL_DIGIT* )?
	| '.' DECIMAL_DIGIT+
	;

fragment ANYBASE_DIGITS
	: ANYBASE_DIGIT+ ( '.' DECIMAL_DIGIT* )?
	| '.' ANYBASE_DIGIT+
	;

fragment EXPONENT
	: ( '+' | '-' )? INTEGER
	;

fragment INTEGER
	: DECIMAL_DIGIT+
	;

fragment DECIMAL_DIGIT
	: [0-9]
	;

fragment ANYBASE_DIGIT
	: [0-9a-zA-Z]
	;

SLOTFORM
	: '#'
	| '#' INTEGER
	| '#' LETTER STRING
	| '##'
	| '##' INTEGER
	;

MESSAGE_NAME
	: '::' STRING
	;

fragment STRING
	: ( DIGIT | LETTER )+
	;

OUTFORM
	: '%'+
	| '%' INTEGER
	;

BLANKFORM
	: SYMBOL? '_.'
	| SYMBOL? ( '_' | '__' | '___' ) SYMBOL?
	;

GETFORM	: '<<' SPACE* FILENAME ;
PUTFORM	: '>>' SPACE* FILENAME	;
APPENDFORM : '>>>' SPACE* FILENAME ;

fragment SPACE : [ \n\t] ;

fragment FILENAME
	: FILEVALIDCHAR+
	;

fragment FILEVALIDCHAR
	: LETTER 
	| DIGIT
	| [`/.!_:$*~?]
	| '-'
	| '\\'
	;

XLEFT
	: '{'
	| '<|'
	;

XRIGHT
	: '}'
	| '|>'
	;

LeftDoubleBracket    : '\u301a' ;
RightDoubleBracket   : '\u301b' ;
InvisibleApplication : '\uf76d' ;
Conjugate            : '\uf3c8' ;
Transpose			 : '\uf3c7' ;
ConjugateTranspose   : '\uf3c9' ;
HermitianTranspose   : '\uf3ce' ;
PlusMinus			 : '\u00b1' ;
MinusPlus			 : '\u2213' ;
Divide				 : '\u00f7' ;
Times				 : '\u00d7' ;
Not				 	 : '\u00ac' ;
Function			 : '\uf4a1' ;

COMMENT : '(*' (COMMENT | .)*? '*)' -> channel(HIDDEN);
WHITESPACE : [ \t]+ -> channel(HIDDEN) ; // TODO: all other whitespace
NEWLINE : '\n'+ -> channel(HIDDEN) ;