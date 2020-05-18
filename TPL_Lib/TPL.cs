//using Antlr4.Runtime;
using CSharpUtils.Extensions;
using Irony;
using Irony.Ast;
using Irony.Parsing;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TplLib.Extensions;
using TplLib.Functions;
using TplLib.Functions.File_IO_Functions;
using TplLib.Functions.String_Functions;
using TplLib.Tpl_Parser;
using TplParser;
using static TplLib.Functions.String_Functions.TplStringConcat;

namespace TplLib
{
    public static class Tpl
    {
        public static TplFunction Create(string query, out IReadOnlyList<LogMessage> errors)
        {
            var grammar = new TplGrammar();
            var tplLangData = new LanguageData(grammar);
            var tplParser = new Parser(tplLangData);

            var parseTree = tplParser.Parse(query);

            if (parseTree.HasErrors())
            {
                errors = parseTree.ParserMessages;
                return null;
            }
            else
            {
                errors = new List<LogMessage>(0);
                var functionNodes = parseTree.Root.ChildNodes.Select(node => node.ChildNodes);
                TplFunction rootFunction = null;

                foreach (var funcNode in functionNodes)
                {
                    TplFunction currentFunction;
                    var funcName = funcNode[0].FindToken().Text;
                    switch (funcName.ToLower())
                    {
                        case "between":
                            currentFunction = new TplBetween()
                            {
                                TargetField = GetOptionalVariable(funcNode[1]),
                                StartValue = funcNode[2].FindToken().ValueString,
                                EndValue = funcNode[3].FindToken().ValueString,
                                Inclusive = GetSwitchValue(funcNode[4], "inclusive"),
                            };
                            break;

                        case "dedup":
                            currentFunction = new TplDedup()
                            {
                                TargetFields = GetOptionalVariableList(funcNode[1]),
                                Mode = GetSwitchValue(funcNode[2], "all") ? TplDedup.DedupMode.All : TplDedup.DedupMode.Consecutive,
                            };
                            break;

                        //case "eval":    //TODO: This aint right....
                        //    currentFunction = new TplEval(funcNode[3].FindToken().ValueString)
                        //    {
                        //        NewFieldName = funcNode[1].FindToken().ValueString,
                        //    };
                        //    break;

                        case "group":
                            currentFunction = new TplGroup()
                            {
                                TargetField = GetOptionalVariable(funcNode[1]),
                                StartRegex = funcNode[2].FindToken().GetTokenAsRegex(),
                                EndRegex = funcNode[3].FindToken().GetTokenAsRegex(),
                            };
                            break;

                        case "kv":
                            currentFunction = new TplKeyValue()
                            {
                                TargetFields = GetOptionalVariableList(funcNode[1], TplResult.DEFAULT_FIELD),
                                KeyValueRegex = GetTokenAsRegex(funcNode[2].FindToken())
                            };
                            break;

                        case "rex":
                            ValidateArgumentList(funcNode[3], "passthru");
                            currentFunction = new TplRegex()
                            {
                                TargetField = GetOptionalVariable(funcNode[1]),
                                Rex = GetTokenAsRegex(funcNode[2].FindToken()),
                                PassThru = GetNamedArgumentValue(funcNode[3], "passthru", false),
                            };
                            break;

                        case "select":
                            currentFunction = new TplSelect()
                            {
                                SelectedFields = GetOptionalVariableList(funcNode[1], TplResult.DEFAULT_FIELD)
                            };
                            break;

                        case "sort":
                            currentFunction = new TplSort(
                                funcNode[1].ChildNodes
                                    .Select(sortNode => new TplSortField(sortNode.ChildNodes[0].FindToken().ValueString, (sortNode.ChildNodes.Count > 1 ? (sortNode.ChildNodes[1].FindToken().ValueString == "desc") : false)))
                                    .ToList()
                                );
                            break;

                        //case "stats":

                        //    break;

                        //case "where":
                            
                        //    break;

                        case "tolower":
                        case "toupper":
                            ValidateArgumentList(funcNode[2], "rex", "group");
                            {
                                var rex = GetNamedArgumentValue<Regex>(funcNode[2], "rex", null);
                                var group = GetNamedArgumentValue<string>(funcNode[2], "group", null);

                                if (rex == null && group != null)
                                    throw new InvalidOperationException($"Invalid argument 'group' in {funcName}. A group cannot be specified unless the 'rex' argument is also specified");
                                else if (rex != null && group != null && !rex.GetNamedCaptureGroupNames().Contains(group))
                                    throw new InvalidOperationException($"Invalid value '{group}' for 'group' argument in {funcName}. The capture group does not exist in the specified regex"); 

                                currentFunction = new TplChangeCase()
                                {
                                    TargetFields = GetOptionalVariableList(funcNode[1], TplResult.DEFAULT_FIELD),
                                    ToUpper = funcName.ToLower() == "toupper",
                                    TargetGroup = group,
                                    MatchingRegex = rex
                                };
                            }
                            break;

                        case "replace":
                            ValidateArgumentList(funcNode[4], "as", "rex", "group", "case");
                            {
                                var rex = new TplReplace()
                                {
                                    TargetFields = GetOptionalVariableList(funcNode[1], TplResult.DEFAULT_FIELD),
                                    Replace = funcNode[3].FindTokenAndGetText(),
                                    AsField = GetNamedArgumentValue<string>(funcNode[4], "as", null),
                                    TargetGroup = GetNamedArgumentValue<string>(funcNode[4], "group", null),
                                    CaseSensitive = GetNamedArgumentValue<bool>(funcNode[4], "case", false),
                                    RegexMode = GetNamedArgumentValue<bool>(funcNode[4], "as", false),
                                };
                                rex.Find = funcNode[2].FindTokenAndGetText();
                                currentFunction = rex;
                            }
                            break;

                        case "splice":
                            {
                                ValidateArgumentList(funcNode[3], "as");
                                var targetField = GetOptionalVariable(funcNode[1]);
                                currentFunction = new TplSplice(funcNode[2].FindTokenAndGetText())
                                {
                                    TargetField = targetField,
                                    AsField = GetNamedArgumentValue(funcNode[3], "as", targetField)
                                };
                            }
                            break;

                        case "concat":
                            {
                                var concatValues = funcNode[3].ChildNodes
                                    .Select(value => value.FindToken())
                                    .Select(t => new ConcatValue(t.ValueString, t.Text[0] == '$'))
                                    .ToList();
                                currentFunction = new TplStringConcat(concatValues, funcNode[1].FindToken().ValueString);
                            }
                            break;

                        case "padleft":
                        case "padright":
                            {
                                ValidateArgumentList(funcNode[3], "char");
                                currentFunction = new TplStringPad()
                                {
                                    TargetFields = GetOptionalVariableList(funcNode[1], TplResult.DEFAULT_FIELD),
                                    TotalLength = funcNode[2].FindTokenAndGetValue<int>(),
                                    PaddingChar = GetNamedArgumentValue<char>(funcNode[3], "char", ' '),
                                    PadRight = funcName == "padright",
                                };
                            }
                            break;

                        case "substr":
                            {
                                ValidateArgumentList(funcNode[4], "as");
                                var target = GetOptionalVariable(funcNode[1]);
                                currentFunction = new TplSubstring()
                                {
                                    TargetField = target,
                                    StartIndex = funcNode[2].FindTokenAndGetValue<int>(),
                                    MaxLength = funcNode[3].FindTokenAndGetValue<int>(-1),
                                    AsField = GetNamedArgumentValue<string>(funcNode[4], "as", target),
                                };
                            }
                            break;

                        case "readlines":
                            {
                                ValidateArgumentList(funcNode[2], "recurse");
                                currentFunction = new TplReadLines()
                                {
                                    FilePath = funcNode[1].FindTokenAndGetValue<string>(),
                                    Recurse = GetNamedArgumentValue(funcNode[2], "recurse", false),
                                };
                            }
                            break;

                        default:
                            throw new InvalidOperationException($"Invalid function name '{funcName}'");
                    }

                    if (rootFunction == null)
                        rootFunction = currentFunction;

                    else
                        rootFunction.AddToPipeline(currentFunction);
                }

                return rootFunction;
            }
        }

        private static string GetOptionalVariable(ParseTreeNode node)
        {
            if (node.ChildNodes.Any())
                return node.ChildNodes[0].FindToken().ValueString;

            return TplResult.DEFAULT_FIELD;
        }

        private static List<string> GetOptionalVariableList(ParseTreeNode node, params string[] defaultValues)
        {
            var list = node.ChildNodes.Select(n => n.FindToken().ValueString).ToList();
            if (!list.Any() && defaultValues.Length > 0)
                list = defaultValues.ToList();

            return list;
        }

        private static bool GetSwitchValue(this ParseTreeNode switchArgNode, string argumentName, bool defaultValue = false)
        {
            if (switchArgNode.ChildNodes.Any() && switchArgNode.ChildNodes[0].FindToken().ValueString.ToLower() != argumentName.ToLower())
            {
                throw switchArgNode.GetException("Invalid argument");
            }

            switch (switchArgNode.ChildNodes.Count)
            {
                case 0:
                    return defaultValue;
                case 1:
                    return true;
                case 2:
                    return switchArgNode.ChildNodes[1].FindToken().ValueString.As<bool>();
                default:
                    throw switchArgNode.GetException("Invalid switch argument");
            }
        }

        private static T GetNamedArgumentValue<T>(ParseTreeNode argsListNode, string argName, T fallbackValue)
        {
            if (!TryGetNamedArgumentValue(argsListNode, argName, out T value, false))
                value = fallbackValue;

            return value;
        }

        private static bool TryGetNamedArgumentValue<T>(ParseTreeNode argsListNode, string argName, out T value, bool required)
        {
            value = default;
            bool found = false;

            if (argsListNode.ChildNodes.Any())
            {
                var arg = argsListNode.ChildNodes.FirstOrDefault(a => a.ChildNodes[0].FindToken().ValueString.ToLower() == argName);
                if (arg != null)
                {
                    var actualArgName = arg.ChildNodes[0].FindToken().ValueString;

                    if (arg.ChildNodes.Count > 1)
                    {
                        var actualArgValue = arg.ChildNodes[1].FindToken().ValueString;

                        try 
                        { 
                            value = arg.ChildNodes[1].FindToken().ValueString.As<T>();
                            found = true;
                        }
                        catch (Exception e) when (e is InvalidCastException || e is FormatException) 
                        { throw arg.GetException($"Invalid value '{actualArgValue}' for argument '{actualArgName}'. Expected a {typeof(T)} type"); }
                    }
                    else if (typeof(T) == typeof(bool))
                    {
                        value = (T)(object)true;
                        found = true;
                    }
                    else if (typeof(T) == typeof(Regex))
                    {
                        value = (T)(object)GetTokenAsRegex(arg.ChildNodes[1].FindToken());
                    }
                    else
                    {
                        throw arg.GetException($"Expected a value of type {typeof(T)} for argument '{actualArgName}'");
                    }

                }
            }

            if (required && !found) throw argsListNode.GetException($"Missing required argument {argName}");
            return found;
        }

        private static void ValidateArgumentList(ParseTreeNode functionNode, params string[] validArgs)
        {
            var args = functionNode.ChildNodes.FirstOrDefault(n => n.ToString() == "ARGUMENT_LIST");
            if (args != null)
            {
                var dups = args.ChildNodes
                    .GroupBy(a => a.FindToken().ValueString.ToLower())
                    .Select(a => new { Name = a.Key, Count = a.Count() })
                    .Where(a => a.Count > 1)
                    .FirstOrDefault();

                if (dups != null)
                    throw functionNode.GetException($"Argument {dups.Name} was specified more than once in function {functionNode.FindToken().ValueString}");

                var invalidArgs = args.ChildNodes
                    .Select(a => a.FindToken())
                    .Where(a => !a.ValueString.ToLower().IsIn(validArgs));

                if (invalidArgs.Any())
                {
                    var badArg = invalidArgs.First();
                    throw badArg.GetException($"Invalid argument '{badArg.ValueString}'");
                }

            }

        }

        private static Regex GetTokenAsRegex(this Token t)
        {
            try
            {
                return new Regex(t.ValueString, RegexOptions.Compiled);
            }
            catch (ArgumentException e)
            {
                throw t.GetException($"Invalid Regex: {e.Message}");
            }
        }

        private static T FindTokenAndGetValue<T>(this ParseTreeNode node, T defaultValue)
        {
            try { return node.FindTokenAndGetValue<T>(); }
            catch { return defaultValue; }
        }

        private static T FindTokenAndGetValue<T>(this ParseTreeNode node)
        {
            try { return node.FindToken().ValueString.As<T>(); }
            catch { throw node.GetException($"Invalid token '{node.FindTokenAndGetText()}'. Expected a token of type {typeof(T)}"); }
        }

        //Getting exceptions
        private static InvalidOperationException GetException(this ParseTreeNode node, string message)
        {
            return node.FindToken().GetException(message);
        }

        private static InvalidOperationException GetException(this Token t, string message)
        {
            var l = t.Location;
            return new InvalidOperationException($"{message} on line {l.Line + 1}, col {l.Column}");
        }

        //private static readonly PreProcessor PreProcessor = new PreProcessor();

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="query"></param>
        ///// <exception cref="AggregateException"/>
        ///// <returns></returns>
        //public static TplFunction FromQuery(string query)
        //{
        //    var tokens = PreProcessor.Tokenize(query, out List<Token> errors);

        //    //Process tokenization errors
        //    if (errors.Any())
        //    {
        //        var exceptions = errors.Select(err => new InvalidOperationException($"Invalid token '{err.RawValue}' on line {err.LineNumber}, char {err.CharNumber}"));
        //        throw new AggregateException($"{errors.Count} Syntax Error(s)", exceptions);
        //    }

        //    TplFunction rootFunction = null;

        //    //Begin processing tokens
        //    for (int i = 0; i < tokens.Count; i++)
        //    {
        //        TplFunction currentFunction;

        //        var currentToken = tokens[i];
        //        i++;
        //        switch (currentToken.TokenType)
        //        {
        //            case TokenType.BETWEEN:    
        //                var between = new TplBetween()
        //                {
        //                    TargetField = tokens.GetTargetField(ref i),
        //                    StartValue = tokens.GetNextRequired(ref i, TokenType.STRING, TokenType.LITERAL_STRING),
        //                    EndValue = tokens.GetNextRequired(ref i, TokenType.STRING, TokenType.LITERAL_STRING),
        //                };

        //                if (tokens.GetParameter(ref i, "inclusive", out bool inclusive))
        //                    between.Inclusive = inclusive;

        //                currentFunction = between;
        //                break;

        //            case TokenType.DEDUP:
        //                var dedup = new TplDedup() { TargetFields = tokens.GetTargetFields(ref i) }; //Needs to be done more manually to prevent setting a default target field

        //                while (tokens.PatternAt(i, TokenType.PARAMETER_SET))
        //                {
        //                    if (tokens.GetParameter(ref i, "consecutive", out bool consec))
        //                        dedup.Mode = consec ? TplDedup.DedupMode.Consecutive : TplDedup.DedupMode.All;

        //                    else if (tokens.GetParameter(ref i, "sort", out TplDedup.DedupSort sortMode))
        //                        dedup.SortMode = sortMode;

        //                    else
        //                        throw new InvalidOperationException($"Invalid parameter '{tokens[i].GetTokenValue()}' in Dedup function, on line {tokens[i].LineNumber}");

        //                    //TODO: Keep track of parameters and make sure they are not set twice
        //                }
        //                currentFunction = dedup;
        //                break;

        //            case TokenType.EVAL:
        //                var evalAssignToVar = tokens.GetNextRequired(ref i, TokenType.VARIABLE);
        //                tokens.GetNextRequired(ref i, TokenType.ASSIGNMENT_OP);

        //                var expressionSb = new StringBuilder();
        //                while (i < tokens.Count && tokens[i].TokenType != TokenType.PIPE_OP)
        //                {
        //                    var t = tokens[i++];
        //                    expressionSb.Append(t.RawValue);    //TODO: This is probably wrong. Look at how EVAL handles strings first
        //                }

        //                currentFunction = new TplEval(expressionSb.ToString()) { NewFieldName = evalAssignToVar };
        //                break;

        //            case TokenType.GROUP:
        //                currentFunction = new TplGroup() 
        //                { 
        //                    TargetField = tokens.GetTargetField(ref i),
        //                    StartRegex = tokens.GetNextRegexRequired(ref i),
        //                    EndRegex = tokens.GetNextRegexRequired(ref i)
        //                };
        //                break;

        //            case TokenType.KEYVALUE:
        //                currentFunction = new TplKeyValue() 
        //                { 
        //                    TargetFields = tokens.GetTargetFields(ref i), 
        //                    KeyValueRegex = tokens.GetNextRegexRequired(ref i) 
        //                };
        //                break;

        //            case TokenType.REX:
        //                var rex = new TplRegex()
        //                {
        //                    TargetField = tokens.GetTargetField(ref i),
        //                    Rex = tokens.GetNextRegexRequired(ref i)
        //                };

        //                if (tokens.GetParameter(ref i, "passthru", out bool passThru))
        //                    rex.PassThru = passThru;

        //                currentFunction = rex;
        //                break;

        //            case TokenType.SELECT:
        //                currentFunction = new TplSelect() { SelectedFields = tokens.GetTargetFields(ref i) };
        //                break;

        //            case TokenType.SORT:
        //                var sort = new TplSort();
        //                var fieldSortList = new List<string>();
        //                while (i < tokens.Count && tokens[i].TokenType != TokenType.PIPE_OP)
        //                {
        //                    if (tokens.PatternAt(i, TokenType.VARIABLE))
        //                        fieldSortList.Add($"+{tokens.GetNextRequired(ref i, TokenType.VARIABLE)}");

        //                    else if (tokens.PatternAt(i, TokenType.ADD_OP, TokenType.VARIABLE))
        //                    {
        //                        i++;
        //                        fieldSortList.Add($"+{tokens.GetNextRequired(ref i, TokenType.VARIABLE)}");
        //                    }

        //                    else if (tokens.PatternAt(i, TokenType.SUBTRACT_OP, TokenType.VARIABLE))
        //                    {
        //                        i++;
        //                        fieldSortList.Add($"-{tokens.GetNextRequired(ref i, TokenType.VARIABLE)}");
        //                    }

        //                    else if (i < tokens.Count
        //                        && !tokens.PatternAt(i, TokenType.PIPE_OP)
        //                        && !tokens.PatternAt(i, TokenType.COMMA_OP, TokenType.VARIABLE)
        //                        && !tokens.PatternAt(i, TokenType.COMMA_OP, TokenType.SUBTRACT_OP, TokenType.VARIABLE)
        //                        && !tokens.PatternAt(i, TokenType.COMMA_OP, TokenType.ADD_OP, TokenType.VARIABLE))
        //                        throw tokens[i].GetException("Sort");

        //                    i++;
        //                }

        //                if (fieldSortList.Any())
        //                    sort.TargetFields = fieldSortList;

        //                currentFunction = sort;
        //                break;

        //            case TokenType.STATS:
        //                var stats = new TplStats();
        //                if (!tokens.GetTokenArray(ref i, out List<Token> modeTokens, TokenType.WORD))
        //                    throw new InvalidOperationException($"Must specify at least one stat type for the stats function on line {currentToken.LineNumber}, char {currentToken.CharNumber}. 'count', 'sum', and/or 'avg'");

        //                var modes = modeTokens.Select(m => m.GetTokenValue().ToLower()).ToList();

        //                var invalidModes = modes.Where(m => !m.IsIn("count", "sum", "avg"));
        //                if (invalidModes.Any())
        //                    throw new InvalidOperationException($"Invalid mode '{invalidModes.First()}' specified in Stats function. Expected count, sum, and/or avg");

        //                var duplicateModes = modes.GroupBy(m => m).Select(m => new { Mode = m.Key, Count = m.Count() }).Where(m => m.Count > 1);
        //                if (duplicateModes.Any())
        //                    throw new InvalidOperationException($"Mode '{duplicateModes.First()}' was specified more than once in Stats function");

        //                stats.Count = modes.Contains("count");
        //                stats.Sum = modes.Contains("sum");
        //                stats.Avg = modes.Contains("avg");
        //                stats.TargetFields = tokens.GetTargetFields(ref i);

        //                if (tokens.GetNext(ref i, out string v, TokenType.BY))
        //                {
        //                    if (!tokens.PatternAt(i, TokenType.VARIABLE))
        //                        throw new InvalidOperationException($"Expected a variable name after '{v}' in Stats function. Line {currentToken.LineNumber}, char {currentToken.CharNumber}");

        //                    stats.ByFields = tokens.GetTargetFields(ref i);
        //                }
        //                currentFunction = stats;
        //                break;

        //            case TokenType.WHERE:
        //                var whereSb = new StringBuilder();
        //                while (i < tokens.Count && !tokens.PatternAt(i, TokenType.PIPE_OP))
        //                {
        //                    whereSb.Append(tokens[i].RawValue); //TODO: This could be wrong. Need to look at Expression ev and see how it handles strings and var names
        //                    i++;
        //                }
        //                currentFunction = new TplWhere(whereSb.ToString());
        //                break;

        //            case TokenType.TOUPPER:
        //            case TokenType.TOLOWER:
        //                var changeCase = new TplChangeCase()
        //                {
        //                    ToUpper = currentToken.TokenType == TokenType.TOUPPER,
        //                    TargetFields = tokens.GetTargetFields(ref i),
        //                };

        //                if (tokens.GetNextRegex(ref i, out Regex regex))
        //                {
        //                    changeCase.MatchingRegex = regex;
        //                    if (tokens.GetParameter(ref i, "group", out string groupName))
        //                    {
        //                        if (!regex.GetNamedCaptureGroupNames().Contains(groupName))
        //                            throw new InvalidOperationException($"Invalid capture group name '{groupName}'. No such group was found");

        //                        changeCase.TargetGroup = groupName;
        //                    }

        //                }

        //                if (tokens.GetNext(ref i, out string v2, TokenType.BY))
        //                    changeCase.AsField = tokens.GetNextRequired(ref i, TokenType.VARIABLE);

        //                currentFunction = changeCase;
        //                break;

        //            case TokenType.REPLACE:
        //                var replace = new TplReplace() { TargetFields = tokens.GetTargetFields(ref i) };
        //                var findStr = tokens.GetNextRequired(ref i, TokenType.STRING, TokenType.LITERAL_STRING);
        //                replace.Replace = tokens.GetNextRequired(ref i, TokenType.STRING, TokenType.LITERAL_STRING);

        //                while (tokens.PatternAt(i, TokenType.PARAMETER_SET))
        //                {
        //                    if (tokens.GetParameter(ref i, "mode", out TplReplace.ReplaceMode repMode))
        //                        replace.RegexMode = repMode == TplReplace.ReplaceMode.REX;

        //                    else if (tokens.GetParameter(ref i, "group", out string groupName))
        //                        replace.TargetGroup = groupName;

        //                    else if (tokens.GetParameter(ref i, "casesensitive", out bool caseSens))
        //                        replace.CaseSensitive = caseSens;
        //                }
        //                replace.InitRegex(findStr);

        //                if (tokens.GetNext(ref i, out string v3, TokenType.AS))
        //                    replace.AsField = tokens.GetNextRequired(ref i, TokenType.VARIABLE);

        //                currentFunction = replace;
        //                break;

        //            case TokenType.SPLICE:
        //                var targField = tokens.GetTargetField(ref i);
        //                var splice = new TplSplice(tokens.GetNextRequired(ref i, TokenType.STRING, TokenType.LITERAL_STRING)) { TargetField = targField };

        //                if (tokens.GetNext(ref i, out string v4, TokenType.AS))
        //                    splice.AsField = tokens.GetNextRequired(ref i, TokenType.VARIABLE);

        //                currentFunction = splice;
        //                break;

        //            case TokenType.CONCAT:
        //                var concat = new TplStringConcat();
        //                while (i < tokens.Count && tokens[i].TokenType != TokenType.AS)
        //                {
        //                    switch (tokens[i].TokenType)
        //                    {
        //                        case TokenType.STRING:
        //                        case TokenType.LITERAL_STRING:
        //                        case TokenType.INTEGER:
        //                        case TokenType.DECIMAL:
        //                            concat.Strings.Add(tokens[i].GetTokenValue());
        //                            concat.Fields.Add(null);
        //                            break;

        //                        case TokenType.VARIABLE:
        //                            concat.Fields.Add(tokens[i].GetTokenValue());
        //                            concat.Strings.Add(null);
        //                            break;

        //                        case TokenType.PIPE_OP:
        //                            throw new InvalidOperationException($"Missing 'as' token in Concat function on line {currentToken.LineNumber}, char {currentToken.CharNumber}");

        //                        default:
        //                            throw tokens[i].GetException();
        //                    }
        //                    i++;
        //                }

        //                if (!tokens.GetNext(ref i, out string v5, TokenType.AS))
        //                    throw new InvalidOperationException($"Missing 'as' token in Concat function on line {currentToken.LineNumber}, char {currentToken.CharNumber}");

        //                concat.AsField = tokens.GetNextRequired(ref i, TokenType.VARIABLE);
        //                currentFunction = concat;
        //                break;

        //            case TokenType.PAD:
        //                var pad = new TplStringPad() { TargetFields = tokens.GetTargetFields(ref i) };

        //                if (tokens.GetNext(ref i, out string leftRightWord, TokenType.WORD, TokenType.LITERAL_STRING, TokenType.STRING))
        //                {
        //                    if (!leftRightWord.ToLower().IsIn("left", "right"))
        //                        throw tokens[i - 1].GetException("Pad", "Expected either left or right");

        //                    pad.PadRight = leftRightWord.ToLower() == "right";
        //                }

        //                pad.TotalLength = tokens.GetNextRequired(ref i, TokenType.INTEGER).As<int>();
        //                if (tokens.GetNext(ref i, out string padChar, TokenType.STRING, TokenType.LITERAL_STRING))
        //                {
        //                    if (padChar.Length > 1)
        //                        throw tokens[i - 1].GetException("Pad", "Only 1 character is allowed");

        //                    pad.PaddingChar = padChar[0];    
        //                }

        //                currentFunction = pad;
        //                break;

        //            case TokenType.SUBSTRING:
        //                var substr = new TplSubstring()
        //                {
        //                    TargetField = tokens.GetTargetField(ref i),
        //                    StartIndex = tokens.GetNextRequired(ref i, TokenType.INTEGER).As<int>(),
        //                };

        //                if (tokens.GetNext(ref i, out string substrLen, TokenType.INTEGER))
        //                    substr.MaxLength = substrLen.As<int>();

        //                if (tokens.GetNext(ref i, out string v6, TokenType.AS))
        //                    substr.AsField = tokens.GetNextRequired(ref i, TokenType.VARIABLE);

        //                currentFunction = substr;
        //                break;

        //            default:
        //                throw currentToken.GetException();
        //        }

        //        //Make sure we are at the PIPE operator
        //        if (i < tokens.Count && (tokens[i].TokenType != TokenType.PIPE_OP || i >= tokens.Count - 1))
        //            throw tokens[i].GetException();

        //        //Add what we have to the pipeline
        //        if (rootFunction == null)
        //            rootFunction = currentFunction;
        //        else
        //            rootFunction.AddToPipeline(currentFunction);
        //    }

        //    return rootFunction;
        //}

        //private static string GetTargetField(this List<Token> tokens, ref int startIndex)
        //{
        //    if (tokens.PatternAt(startIndex, TokenType.VARIABLE))
        //        return tokens[startIndex++].GetTokenValue();

        //    return "_raw";
        //}

        //private static List<string> GetTargetFields (this List<Token> tokens, ref int startIndex)
        //{
        //    var targetedFields = new List<Token>();

        //    if (!tokens.GetTokenArray(ref startIndex, out targetedFields, TokenType.VARIABLE))
        //        targetedFields = new List<Token>() { new Token("_raw", TokenType.VARIABLE, -1, -1) };

        //    return targetedFields.Select(t => t.GetTokenValue()).ToList();
        //}

        //private static bool GetTokenArray (this List<Token> tokens, ref int startIndex, out List<Token> tokenArray, params TokenType[] validTokenTypes)
        //{
        //    tokenArray = new List<Token>();

        //    while (startIndex < tokens.Count && validTokenTypes.Contains(tokens[startIndex].TokenType))
        //    {
        //        tokenArray.Add(tokens[startIndex]);
        //        startIndex++;
        //        if (startIndex < tokens.Count)
        //        {
        //            if (tokens[startIndex].TokenType == TokenType.COMMA_OP)
        //                startIndex++;

        //            else
        //                break;
        //        }
        //    }

        //    return tokenArray.Any();
        //}

        //private static Regex GetNextRegexRequired(this List<Token> tokens, ref int startIndex)
        //{
        //    if (!tokens.GetNextRegex(ref startIndex, out Regex regex))
        //        throw tokens[startIndex].GetException();

        //    return regex;
        //}

        //private static bool GetNextRegex(this List<Token> tokens, ref int startIndex, out Regex regex)
        //{
        //    regex = null;
        //    var regexToken = tokens[startIndex];
        //    if (!tokens.GetNext(ref startIndex, out string strVal, TokenType.STRING, TokenType.LITERAL_STRING))
        //        return false;

        //    try
        //    {
        //        regex = new Regex(strVal, RegexOptions.Compiled);
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception($"Invalid Regex on line {regexToken.LineNumber}, char {regexToken.CharNumber}: {e.Message}");
        //    }
        //}

        //private static string GetNextRequired(this List<Token> tokens, ref int startIndex, params TokenType[] types)
        //{
        //    if (!tokens.GetNext(ref startIndex, out string value, types))
        //        if (startIndex < tokens.Count)
        //            throw tokens[startIndex].GetException();
        //        else
        //        {
        //            var acceptedValues = types.Select(t => t.ToString()).ToArray();
        //            var sb = new StringBuilder(acceptedValues[0]);

        //            for (int i = 1; i < acceptedValues.Length - 1; i++)
        //                sb.Append($", {acceptedValues[i]}");

        //            if (acceptedValues.Length > 1)
        //                sb.Append($" or {acceptedValues.Last()}");

        //            throw new InvalidOperationException($"Missing arguments. Expected one of the following tokens: {sb}");
        //        }

        //    return value;
        //}

        //private static bool GetNext(this List<Token> tokens, ref int startIndex, out string value, params TokenType[] types)
        //{
        //    value = null;

        //    if (startIndex >= tokens.Count)
        //        return false;

        //    if (types.Contains(tokens[startIndex].TokenType))
        //    {
        //        value = tokens[startIndex].GetTokenValue();
        //        startIndex++;
        //        return true;
        //    }

        //    return false;
        //}

        //private static bool GetParameter<T>(this List<Token> tokens, ref int startIndex, string paramName, out T paramValue)
        //{
        //    if (startIndex >= tokens.Count - 1)
        //    {
        //        paramValue = default;
        //        return false;
        //    }    

        //    var paramToken = tokens[startIndex];
        //    var paramTokenValue = paramToken.GetTokenValue();

        //    if (paramToken.TokenType != TokenType.PARAMETER_SET || paramTokenValue != paramName.ToLower())
        //    {
        //        paramValue = default;
        //        return false;
        //    }
        //    else
        //        startIndex++;

        //    var valueToken = tokens[startIndex];
        //    var valueTokenValue = valueToken.GetTokenValue();
        //    startIndex++;

        //    //Get parameter based on type
        //    if (typeof(T) == typeof(string))
        //    {
        //        if (!valueToken.TokenType.IsIn(TokenType.STRING, TokenType.LITERAL_STRING))
        //            throw valueToken.GetException();

        //        paramValue = valueTokenValue.As<T>();
        //        return true;
        //    }
        //    else if (typeof(T).IsEnum)
        //    {
        //        if (!valueToken.TokenType.IsIn(TokenType.WORD, TokenType.STRING, TokenType.LITERAL_STRING))
        //            throw valueToken.GetException();

        //        try 
        //        { 
        //            paramValue = (T)Enum.Parse(typeof(T), valueTokenValue, true);
        //            return true;
        //        }
        //        catch
        //        {
        //            var acceptedValues = Enum.GetNames(typeof(T));
        //            var sb = new StringBuilder(acceptedValues[0]);

        //            for (int i = 1; i < acceptedValues.Length - 1; i++)
        //                sb.Append($", {acceptedValues[i]}");

        //            if (acceptedValues.Length > 1)
        //                sb.Append($" or {acceptedValues.Last()}");

        //            throw new InvalidOperationException($"Invalid value '{valueTokenValue}' for parameter '{paramTokenValue}'. Expected one of the following values: {sb}");
        //        }
        //    }

        //    else if (typeof(T) == typeof(bool))
        //    {
        //        if (valueToken.TokenType != TokenType.BOOL)
        //            throw new InvalidOperationException($"Invalid value '{valueTokenValue}' for parameter '{paramTokenValue}'. Expected one of the following values: true or false");

        //        paramValue = valueTokenValue.As<T>();
        //        return true;
        //    }

        //    else if (typeof(T) == typeof(int))
        //    {
        //        if (valueToken.TokenType != TokenType.INTEGER)
        //            throw new InvalidOperationException($"Invalid value '{valueTokenValue}' for parameter '{paramTokenValue}'. Expected an integer value");

        //        paramValue = valueTokenValue.As<T>();
        //        return true;
        //    }

        //    else if (typeof(T) == typeof(double))
        //    {
        //        if (valueToken.TokenType != TokenType.DECIMAL && valueToken.TokenType != TokenType.INTEGER)
        //            throw new InvalidOperationException($"Invalid value '{valueTokenValue}' for parameter '{paramTokenValue}'. Expected a decimal or integer value");

        //        paramValue = valueTokenValue.As<T>();
        //        return true;
        //    }

        //    else
        //    {
        //        throw valueToken.GetException();
        //    }
        //}

        //private static bool GetParameter(this List<Token> tokens, ref int startIndex, out string paramName, out string paramValue, params TokenType[] legalValueTypes)
        //{
        //    if (!legalValueTypes.Any())
        //    {
        //        legalValueTypes = new TokenType[] { TokenType.STRING, TokenType.LITERAL_STRING, TokenType.BOOL, TokenType.INTEGER, TokenType.DECIMAL, TokenType.WORD };
        //    }

        //    foreach (var type in legalValueTypes)
        //        if (tokens.GetTokenPattern(ref startIndex, out List<Token> paramTokens, TokenType.PARAMETER_SET, type))
        //        {
        //            paramName = paramTokens[0].GetTokenValue().ToLower();
        //            paramValue = paramTokens[1].GetTokenValue();
        //            return true;
        //        }

        //    paramName = null;
        //    paramValue = null;
        //    return false;
        //}

        //private static bool GetTokenPattern(this List<Token> tokens, ref int startIndex, out List<Token> matchedPattern, params TokenType[] tokenPattern)
        //{
        //    matchedPattern = new List<Token>();

        //    if (tokens.PatternAt(startIndex, tokenPattern))
        //        for (; startIndex < tokens.Count + tokenPattern.Length; startIndex++)
        //            matchedPattern.Add(tokens[startIndex]);

        //    return matchedPattern.Any();
        //}

        //private static bool PatternAt(this List<Token> tokens, int index, params TokenType[] tokenPattern)
        //{
        //    if (index + tokenPattern.Length > tokens.Count)
        //        return false;

        //    for (int i = 0; i < tokenPattern.Length; i++)
        //        if (tokens[i + index].TokenType != tokenPattern[i])
        //            return false;

        //    return true;

        //}

        //private static string GetTokenValue(this Token token)
        //{
        //    switch (token.TokenType)
        //    {
        //        case TokenType.PARAMETER_SET:
        //            return token.RawValue.Substring(0, token.RawValue.IndexOfAny(new char[] { ' ', '=', '\r', '\n' }));

        //        case TokenType.VARIABLE:
        //            return token.RawValue.StartsWith("$") ? token.RawValue.Substring(1) : token.RawValue;

        //        case TokenType.LITERAL_STRING:
        //            return token.RawValue.Substring(2, token.RawValue.Length - 3);

        //        case TokenType.STRING:
        //            return Regex.Unescape(token.RawValue.Substring(1, token.RawValue.Length - 2));

        //        default:
        //            return token.RawValue;
        //    }
        //}

        //private static InvalidOperationException GetException(this Token token, string functionName = null, string expectedValues = null)
        //{
        //    return new InvalidOperationException($"Invalid token '{token.RawValue}'{(functionName == null ? "" : $" in {functionName}")} on line {token.LineNumber}, char {token.CharNumber}. {expectedValues}");
        //}
    }
}
