using CSharpUtils.Extensions;
using Irony;
using Irony.Ast;
using Irony.Parsing;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TplLib.Extensions;
using TplLib.Functions;
using TplLib.Functions.File_IO_Functions;
using TplLib.Functions.String_Functions;
using TplLib.Tpl_Parser.ExpressionTree;
using TplLib.Tpl_Parser.ExpressionTree.Operators.Binary;
using TplLib.Tpl_Parser.ExpressionTree.Operators.Unary;
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

                        case "delete":
                            {
                                var fields = GetOptionalVariableList(funcNode[1]);

                                if (fields.Count < 1)
                                    throw funcNode[0].GetException("At least one field name is required in Delete function");

                                currentFunction = new TplDelete()
                                {
                                    SelectedFields = fields
                                };
                            }
                            break;

                        case "eval":
                            var expTree = GetAsExpressionTree(funcNode[3], null);

                            currentFunction = new TplEval(funcNode[3].GetAsExpressionTree(null))
                            {
                                NewFieldName = funcNode[1].FindTokenAndGetValue<string>(),
                            };
                            break;

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

                        case "stats":
                            {
                                ValidateArgumentList(funcNode[3], "count", "sum", "avg");
                                if (!funcNode[3].ChildNodes.Any())
                                    throw funcNode[0].GetException("Expected 1 or more of the following arguments: 'count', 'sum', or 'avg' in 'stats' function");

                                List<string> byFields;
                                if (funcNode[2].ChildNodes.Any())
                                {
                                    byFields = GetOptionalVariableList(funcNode[2].ChildNodes[1]);
                                    if (!byFields.Any())
                                        throw funcNode[2].GetException("Expected 1 or more variables in 'stats' function after 'by'");
                                }
                                else byFields = new List<string>(0);

                                currentFunction = new TplStats()
                                {
                                    TargetFields = GetOptionalVariableList(funcNode[1], TplResult.DEFAULT_FIELD),
                                    ByFields = byFields,
                                    Avg = GetNamedArgumentValue(funcNode[3], "avg", false),
                                    Count = GetNamedArgumentValue(funcNode[3], "count", false),
                                    Sum = GetNamedArgumentValue(funcNode[3], "sum", false),
                                };
                            }
                            break;

                        case "where":
                            currentFunction = new TplWhere(funcNode[1].GetAsExpressionTree(null));
                            break;

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
                                    Replace = funcNode[3].FindTokenAndGetValue<string>(),
                                    AsField = GetNamedArgumentValue<string>(funcNode[4], "as", null),
                                    TargetGroup = GetNamedArgumentValue<string>(funcNode[4], "group", null),
                                    CaseSensitive = GetNamedArgumentValue<bool>(funcNode[4], "case", false),
                                    RegexMode = GetNamedArgumentValue<bool>(funcNode[4], "rex", false),
                                };
                                rex.Find = funcNode[2].FindTokenAndGetValue<string>();
                                currentFunction = rex;
                            }
                            break;

                        case "splice":
                            {
                                ValidateArgumentList(funcNode[3], "as");
                                var targetField = GetOptionalVariable(funcNode[1]);
                                currentFunction = new TplSplice(funcNode[2].FindTokenAndGetValue<string>())
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

                        case "split":
                            {
                                currentFunction = new TplSplit()
                                {
                                    TargetField = GetOptionalVariable(funcNode[1]),
                                    SplitOn = funcNode[2].FindTokenAndGetValue<string>(),
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

                    if (typeof(T) == typeof(Regex))
                    {
                        value = (T)(object)GetTokenAsRegex(arg.ChildNodes[1].FindToken());
                        found = true;
                    }
                    else if (arg.ChildNodes.Count > 1)
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
                    else
                    {
                        throw arg.GetException($"Expected a value of type {typeof(T)} for argument '{actualArgName}'");
                    }

                }
            }

            if (required && !found) throw argsListNode.GetException($"Missing required argument {argName}");
            return found;
        }

        private static void ValidateArgumentList(ParseTreeNode args, params string[] validArgs)
        {
            if (args != null)
            {
                var dups = args.ChildNodes
                    .GroupBy(a => a.FindToken().ValueString.ToLower())
                    .Select(a => new { Name = a.Key, Count = a.Count() })
                    .Where(a => a.Count > 1)
                    .FirstOrDefault();

                if (dups != null)
                    throw args.GetException($"Argument {dups.Name} was specified more than once in function {args.FindToken().ValueString}");

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

        
        private static ExpTreeNode GetAsExpressionTree(this ParseTreeNode parsedNode, ExpTreeNode parentExpNode)
        {
            //The current node is a variable / value token. Create a value node and return it back
            if (!parsedNode.ChildNodes.Any())
            {
                switch (parsedNode.Term.Name)
                {
                    case "variable":
                        {
                            var varName = parsedNode.FindTokenAndGetValue<string>();
                            return new VariableValue(varName, parentExpNode);
                        }

                    case "boolean":
                        return new LiteralValue(parsedNode.FindTokenAndGetValue<bool>(), parentExpNode);

                    case "integer":
                    case "decimal":
                        return new LiteralValue(parsedNode.FindTokenAndGetValue<double>(), parentExpNode);

                    case "SingleQuoteString":
                    case "DoubleQuoteString":
                        return new LiteralValue(parsedNode.FindTokenAndGetValue<string>(), parentExpNode);

                    default:
                        throw parsedNode.GetException($"Invalid token type '{parsedNode.Term.Name}' in expression");

                }
            }

            // Look on the next node down
            else if (parsedNode.ChildNodes.Count == 1)
            {
                return GetAsExpressionTree(parsedNode.ChildNodes[0], parentExpNode);
            }

            //Ignore parenthesis, the middle non-terminal is what we want
            // Look on the next node down
            else if (parsedNode.ChildNodes.Count == 3 && parsedNode.ChildNodes[0]?.Token?.Text == "(")
            {
                return GetAsExpressionTree(parsedNode.ChildNodes[1], parentExpNode);
            }

            //Binary operator
            else if (parsedNode.ChildNodes.Count == 3)
            {
                BinaryOperatorBase binaryOp;
                var opStr = parsedNode.ChildNodes[1].FindToken().ValueString;
                switch (opStr)
                {
                    // Math
                    case "+":
                        binaryOp = new AdditionOperator(parentExpNode);
                        break;
                    case "-":
                        binaryOp = new SubtractionOperator(parentExpNode);
                        break;
                    case "*":
                        binaryOp = new MultiplacationOperator(parentExpNode);
                        break;
                    case "/":
                        binaryOp = new DivisionOperator(parentExpNode);
                        break;
                    case "%":
                        binaryOp = new ModulusOperator(parentExpNode);
                        break;
                    case "^":
                        binaryOp = new PowerOperator(parentExpNode);
                        break;
                    
                    // Bool
                    case "~==":
                        binaryOp = new LooseEqualsOperator(parentExpNode);
                        break;
                    case "~!=":
                        binaryOp = new LooseNotEqualsOperator(parentExpNode);
                        break;
                    case "==":
                        binaryOp = new EqualsOperator(parentExpNode);
                        break;
                    case "!=":
                        binaryOp = new NotEqualsOperator(parentExpNode);
                        break;
                    case "like":
                        binaryOp = new LikeOperator(parentExpNode);
                        break;
                    case "match":
                        binaryOp = new MatchOperator(parentExpNode);
                        break;
                    case ">":
                        binaryOp = new GreaterThanOperator(parentExpNode);
                        break;
                    case ">=":
                        binaryOp = new GreaterThanOrEqualOperator(parentExpNode);
                        break;
                    case "<":
                        binaryOp = new LessThanOperator(parentExpNode);
                        break;
                    case "<=":
                        binaryOp = new LessThanOrEqualOperator(parentExpNode);
                        break;

                    case "&&":
                        binaryOp = new AndOperator(parentExpNode);
                        break;
                    case "||":
                        binaryOp = new OrOperator(parentExpNode);
                        break;

                    default:
                        throw parsedNode.ChildNodes[1].GetException($"Unrecognized operator '{opStr}'");
                }

                binaryOp.LeftOperand = GetAsExpressionTree(parsedNode.ChildNodes[0], binaryOp);
                binaryOp.RightOperand = GetAsExpressionTree(parsedNode.ChildNodes[2], binaryOp);

                //Optimize
                if (binaryOp.LeftOperand is LiteralValue && binaryOp.RightOperand is LiteralValue)
                    return new LiteralValue(binaryOp.Eval(), parentExpNode);

                return binaryOp;
            }

            // Unary operator
            else if (parsedNode.ChildNodes.Count == 2)
            {
                var opVal = parsedNode.ChildNodes[0].FindToken().Value;
                UnaryOperatorBase unaryOp;
                if (parsedNode.ChildNodes[0].FindToken().Value is TypeCode convertType)
                {
                    unaryOp = new TypeConversionOperator(convertType, parentExpNode);
                }
                else
                {
                    var opStr = opVal.ToString();
                    switch (opStr)
                    {
                        case "!":
                            unaryOp = new NotOperator(parentExpNode);
                            break;

                        //Property Checking
                        case "lengthof":
                            unaryOp = new LengthOperator(parentExpNode);
                            break;
                        case "typeof":
                            unaryOp = new TypeOperator(parentExpNode);
                            break;

                        default:
                            unaryOp = new GenericUnaryMathFunctionOperator(opStr, parentExpNode);
                            break;
                    }
                }

                unaryOp.Operand = GetAsExpressionTree(parsedNode.ChildNodes[1], unaryOp);

                //Optimize
                if (unaryOp.Operand is LiteralValue)
                    return new LiteralValue(unaryOp.Eval(), parentExpNode);

                return unaryOp;
            }

            else
            {
                throw parsedNode.GetException($"Invalid number of tokens ({parsedNode.ChildNodes.Count})");
            }
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
    }
}
