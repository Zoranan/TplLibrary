﻿using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TplParser
{
    [Language("TPL", "0.75", "Text Processing Language")]
    public class TplGrammar : Grammar
    {
        public TplGrammar() : base(false)
        {
            var lineComment = new CommentTerminal("LineComment", "//", "\r", "\n", "\u2085", "\u2028", "\u2029");
            var blockComment = new CommentTerminal("BlockComment", "/*", "*/");
            NonGrammarTerminals.Add(lineComment);
            NonGrammarTerminals.Add(blockComment);

            var comma = ToTerm(",");
            var pipe = ToTerm("|", "pipe");

            var boolean = new RegexBasedTerminal("boolean", @"\b(?:true|false)\b") 
            { EditorInfo = new TokenEditorInfo(TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None) };

            var integer = new RegexBasedTerminal("integer", @"-?\d+") 
            { EditorInfo = new TokenEditorInfo(TokenType.Literal, TokenColor.Number, TokenTriggers.None) };
            
            var _decimal = new RegexBasedTerminal("decimal", @"-?\d+\.\d+") 
            { EditorInfo = new TokenEditorInfo(TokenType.Literal, TokenColor.Number, TokenTriggers.None) };

            var typeConvert = new CustomRegexBasedTerminal("type_convert_op", @"\[(?<type>number|string|bool)\]")
            {
                EditorInfo = new TokenEditorInfo(TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None),
                ValueSelector = m =>
                {
                    switch (m.Groups["type"].Value)
                    {
                        case "number":
                            return TypeCode.Double;
                        case "string":
                            return TypeCode.String;
                        case "bool":
                            return TypeCode.Boolean;
                        default:
                            return null;
                    }
                }
            };

            var variable = new CustomRegexBasedTerminal("variable", @"\$[0-9A-Za-z_]+")
            {
                EditorInfo = new TokenEditorInfo(TokenType.Identifier, TokenColor.Identifier, TokenTriggers.None),
                ValueSelector = s => s.Value.Substring(1)
            };

            var argument = new CustomRegexBasedTerminal("ArgumentName", "-[A-Za-z]+")
            {
                EditorInfo = new TokenEditorInfo(TokenType.Identifier, TokenColor.Identifier, TokenTriggers.None),
                ValueSelector = s => s.Value.Substring(1)
            };

            var word = new RegexBasedTerminal("word", @"\w+");
            argument.EditorInfo = new TokenEditorInfo(TokenType.Text, TokenColor.Text, TokenTriggers.None);

            var dblQuoteString = TerminalFactory.CreateCSharpString("DoubleQuoteString"); // Allows Literal strings (@"\string")
            var singleQuoteString = new StringLiteral("SingleQuoteString", "'");

            //Punctuation
            MarkPunctuation(comma);

            //Nonterminals
            var PIPELINE = new NonTerminal("PIPELINE");
            var FUNCTION = new NonTerminal("FUNCTION");

            var STRING = new NonTerminal("STRING");
            var OPTIONAL_INT = new NonTerminal("OPTIONAL_INT");
            var OPTIONAL_VAR = new NonTerminal("OPTIONAL_VAR");
            var SORT_VAR = new NonTerminal("SORT_VAR");
            var VALUE = new NonTerminal("VALUE");
            var LIST_OF_VARIABLES = new NonTerminal("VAR_LIST");
            var LIST_OF_SORT_VARIABLES = new NonTerminal("SORT_VAR_LIST");
            var LIST_OF_VALUES = new NonTerminal("VALUE_LIST");

            var NAMED_ARGUMENT = new NonTerminal("ARGUMENT");
            var SWITCH_ARGUMENT = new NonTerminal("SWITCH_ARGUMENT");
            var LIST_OF_ARGUMENTS = new NonTerminal("ARGUMENT_LIST");
            var OPTIONAL_BY_FIELDS = new NonTerminal("BY_FIELDS_LIST");

            var EXPRESSION_LVL0 = new NonTerminal("EXPRESSION LVL 0");
            var EXPRESSION_LVL1 = new NonTerminal("EXPRESSION LVL 1");
            var EXPRESSION_LVL2 = new NonTerminal("EXPRESSION LVL 2");
            var EXPRESSION_LVL3 = new NonTerminal("EXPRESSION LVL 3");
            var EXPRESSION_LVL4 = new NonTerminal("EXPRESSION LVL 4");
            var EXPRESSION_LVL5 = new NonTerminal("EXPRESSION LVL 5");
            var EXPRESSION_LVL6 = new NonTerminal("EXPRESSION LVL 6");
            var EXPRESSION_LVL7 = new NonTerminal("EXPRESSION LVL 7");
            var EXPRESSION_LVL8 = new NonTerminal("EXPRESSION LVL 8");

            LIST_OF_VARIABLES.Rule = MakeListRule(LIST_OF_VARIABLES, comma, variable, TermListOptions.AllowEmpty);
            LIST_OF_ARGUMENTS.Rule = MakeListRule(LIST_OF_ARGUMENTS, null, NAMED_ARGUMENT, TermListOptions.AllowEmpty);
            LIST_OF_SORT_VARIABLES.Rule = MakeListRule(LIST_OF_SORT_VARIABLES, comma, SORT_VAR, TermListOptions.AllowEmpty);
            LIST_OF_VALUES.Rule = MakeListRule(LIST_OF_VALUES, null, VALUE);

            STRING.Rule = dblQuoteString
                | singleQuoteString
                ;

            OPTIONAL_VAR.Rule = variable
                | Empty
                ;

            OPTIONAL_INT.Rule = integer
                | Empty
                ;

            SORT_VAR.Rule = variable + "asc"
                | variable + "desc"
                | variable
                ;

            VALUE.Rule = STRING
                | variable
                | boolean
                | integer
                | _decimal
                ;

            OPTIONAL_BY_FIELDS.Rule = "by" + LIST_OF_VARIABLES
                | Empty
                ;

            EXPRESSION_LVL0.Rule = EXPRESSION_LVL1
                | EXPRESSION_LVL0 + ToTerm("||", "or_op") + EXPRESSION_LVL1
                ;

            EXPRESSION_LVL1.Rule = EXPRESSION_LVL2
                | EXPRESSION_LVL1 + ToTerm("&&", "and_op") + EXPRESSION_LVL2
                ;

            EXPRESSION_LVL2.Rule = EXPRESSION_LVL3
                | EXPRESSION_LVL2 + ToTerm("==", "equal_op") + EXPRESSION_LVL3
                | EXPRESSION_LVL2 + ToTerm("!=", "ne_op") + EXPRESSION_LVL3
                | EXPRESSION_LVL2 + ToTerm("~==", "loose_eq_op") + EXPRESSION_LVL3
                | EXPRESSION_LVL2 + ToTerm("~!=", "loose_eq_op") + EXPRESSION_LVL3
                | EXPRESSION_LVL2 + ToTerm("like", "like_op") + EXPRESSION_LVL3
                | EXPRESSION_LVL2 + ToTerm("match", "match_op") + EXPRESSION_LVL3
                ;

            EXPRESSION_LVL3.Rule = EXPRESSION_LVL4
                | EXPRESSION_LVL3 + "<=" + EXPRESSION_LVL4
                | EXPRESSION_LVL3 + ">=" + EXPRESSION_LVL4
                | EXPRESSION_LVL3 + "<"  + EXPRESSION_LVL4
                | EXPRESSION_LVL3 + ">"  + EXPRESSION_LVL4
                ;

            EXPRESSION_LVL4.Rule = EXPRESSION_LVL5
                | EXPRESSION_LVL4 + ToTerm("+", "add_op") + EXPRESSION_LVL5
                | EXPRESSION_LVL4 + ToTerm("-", "sub_op") + EXPRESSION_LVL5
                ;

            EXPRESSION_LVL5.Rule = EXPRESSION_LVL6
                | EXPRESSION_LVL5 + ToTerm("*", "mult_op") + EXPRESSION_LVL6
                | EXPRESSION_LVL5 + ToTerm("/", "div_op") + EXPRESSION_LVL6
                | EXPRESSION_LVL5 + ToTerm("%", "mod_op") + EXPRESSION_LVL6
                ;

            EXPRESSION_LVL6.Rule = EXPRESSION_LVL7
                | EXPRESSION_LVL7 + ToTerm("^", "pow_op") + EXPRESSION_LVL6
                ;

            EXPRESSION_LVL7.Rule = EXPRESSION_LVL8
                | ToTerm("!", "not_op") + EXPRESSION_LVL8
                | ToTerm("abs", "round_op") + EXPRESSION_LVL8
                | ToTerm("cos", "round_op") + EXPRESSION_LVL8
                | ToTerm("ceiling", "round_op") + EXPRESSION_LVL8
                | ToTerm("floor", "round_op") + EXPRESSION_LVL8
                | ToTerm("log", "round_op") + EXPRESSION_LVL8
                | ToTerm("log10", "round_op") + EXPRESSION_LVL8
                | ToTerm("round", "round_op") + EXPRESSION_LVL8
                | ToTerm("sin", "round_op") + EXPRESSION_LVL8
                | ToTerm("tan", "round_op") + EXPRESSION_LVL8
                | ToTerm("sqrt", "round_op") + EXPRESSION_LVL8
                | ToTerm("lengthof", "length_op") + EXPRESSION_LVL8
                | ToTerm("typeof", "type_op") + EXPRESSION_LVL8
                | typeConvert + EXPRESSION_LVL8
                ;

            EXPRESSION_LVL8.Rule = VALUE
                | "(" + EXPRESSION_LVL0 + ")"
                ;

            SWITCH_ARGUMENT.Rule = argument + boolean 
                | argument
                | Empty
                ;

            NAMED_ARGUMENT.Rule = argument + VALUE
                | argument
                ;

            FUNCTION.Rule = ToTerm("between") + OPTIONAL_VAR + STRING + STRING + SWITCH_ARGUMENT
                | "dedup"   + LIST_OF_VARIABLES + SWITCH_ARGUMENT
                | "delete"  + LIST_OF_VARIABLES
                | "eval"    + variable + "=" + EXPRESSION_LVL0
                | "first"   + integer
                | "last"    + integer
                | "group"   + OPTIONAL_VAR + STRING + STRING
                | "kv"      + LIST_OF_VARIABLES + STRING
                | "rex"     + OPTIONAL_VAR + STRING + LIST_OF_ARGUMENTS
                | "select"  + LIST_OF_VARIABLES
                | "sort"    + LIST_OF_SORT_VARIABLES
                | "stats"   + LIST_OF_VARIABLES + OPTIONAL_BY_FIELDS + LIST_OF_ARGUMENTS
                | "where"   + EXPRESSION_LVL0
                | "toupper" + LIST_OF_VARIABLES + LIST_OF_ARGUMENTS
                | "tolower" + LIST_OF_VARIABLES + LIST_OF_ARGUMENTS
                | "replace" + LIST_OF_VARIABLES + STRING + STRING + LIST_OF_ARGUMENTS
                | "splice"  + OPTIONAL_VAR + STRING + LIST_OF_ARGUMENTS
                | "concat"  + variable + "=" + LIST_OF_VALUES
                | "padleft" + LIST_OF_VARIABLES + integer + LIST_OF_ARGUMENTS
                | "padright"+ LIST_OF_VARIABLES + integer + LIST_OF_ARGUMENTS
                | "substr"  + OPTIONAL_VAR + integer + OPTIONAL_INT + LIST_OF_ARGUMENTS
                | "split"   + OPTIONAL_VAR + STRING
                | "readlines" + STRING + LIST_OF_ARGUMENTS
                ;
                
            PIPELINE.Rule = MakeListRule(PIPELINE, pipe, FUNCTION);
            Root = PIPELINE;
        }
    }
}
