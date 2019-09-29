using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TPL_Lib.Tpl_Parser
{
    enum TokenType { NULL, QUOTE, INT, DECIMAL, WORD, VAR_NAME, PARAMETER, COMMA, EQUALS, LIST, UNPROCESSED }
    internal class TokenResult
    {
        internal bool IsSuccess { get; private set; }
        internal TokenType TokenType { get; private set; }
        internal string OriginalValue { get; private set; }
        internal int Position { get; private set; }
        internal int Length { get => (OriginalValue == null ? 0 : OriginalValue.Length); }
        internal TokenResult ParamName { get; private set; }
        internal TokenResult ParamValue { get; private set; }
        internal ParsableString Source { get; private set; }
        private List<TokenResult> _resultList = null;
        internal List<TokenResult> ResultsList
        {
            get
            {
                return _resultList != null ? _resultList : new List<TokenResult>() { this };
            }
            private set
            {
                _resultList = value;
            }
        }
        internal TokenType ListType
        {
            get
            {
                var tt = TokenType.NULL;

                if (ResultsList != null && TokenType == TokenType.LIST)
                    foreach (var item in ResultsList)
                        if (item.TokenType > tt)
                            tt = item.TokenType;

                return tt;
            }
        }

        internal static Dictionary<TokenType, Regex> _typeValidationRegicies = new Dictionary<TokenType, Regex>()
        {
            { TokenType.NULL, new Regex("^$", RegexOptions.Compiled) },
            { TokenType.QUOTE, new Regex("^(\"|').*\\1$", RegexOptions.Compiled) },
            { TokenType.INT, new Regex("^-?[0-9]+$", RegexOptions.Compiled) },
            { TokenType.DECIMAL, new Regex(@"^-?[0-9]+(\.[0-9]+)?$", RegexOptions.Compiled) },
            { TokenType.WORD, new Regex("^[A-Z]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase) },
            { TokenType.VAR_NAME, new Regex(@"^(?:(\$[+-]?[_A-Z0-9]+)|([+-]?[_A-Z]+[_A-Z0-9]+))$", RegexOptions.Compiled | RegexOptions.IgnoreCase) },
            { TokenType.PARAMETER, new Regex(@"^(?<name>[A-Z]+?) *= *(?<value>[^\s].*?) *$", RegexOptions.Compiled | RegexOptions.IgnoreCase) },
            { TokenType.COMMA, new Regex(@"^,$", RegexOptions.Compiled) },
            { TokenType.EQUALS, new Regex(@"^=$", RegexOptions.Compiled) },
            //{ TokenType.LIST, new Regex(@"^([^,]+,[^,]+)+$", RegexOptions.Compiled | RegexOptions.IgnoreCase) },
        };

        private TokenResult() { }

        internal static TokenResult Fail(ParsableString source)
        {
            return new TokenResult
            {
                Source = source,
                OriginalValue = null,
                TokenType = TokenType.NULL,
                Position = -1,
                IsSuccess = false
            };
        }

        internal static TokenResult Create(ParsableString source, string originalValue, int position, List<TokenResult> resultsList = null)
        {
            //Figure out what type of item this token is
            var tokenType = TokenType.NULL;

            //If its a list of tokens
            if (resultsList != null)
                tokenType = TokenType.LIST;

            //Otherwise find out what type it is
            else
            foreach (var type in _typeValidationRegicies)
                if (type.Value.IsMatch(originalValue))
                {
                    tokenType = type.Key;
                    break;
                }

            //If this is a list token make sure all list items are the same or compatable types
            var listType = TokenType.NULL;
            if (tokenType == TokenType.LIST)
                foreach (var item in resultsList)
                    if (listType == TokenType.NULL)
                        listType = item.TokenType;

                    else if (!item.IsType(listType, true))
                        throw new ArgumentException($"Invalid item {item.OriginalValue} (type {item.TokenType}) cannot be added to a list of {listType} items.");

            //Create the new token result
            var newToken = new TokenResult()
            {
                IsSuccess = tokenType != TokenType.NULL,
                OriginalValue = originalValue,
                Source = source,
                TokenType = tokenType,
                Position = position,
                ResultsList = resultsList
            };

            //if (newToken.TokenType == TokenType.PARAMETER)
            //{
            //    var groups = _typeValidationRegicies[TokenType.PARAMETER].Match(originalValue).Groups;
            //    newToken.ParamName = groups["name"].Value;
            //    newToken.ParamValue = groups["value"].Value;
            //}

            return newToken;
        }

        internal static TokenResult CreateParam(ParsableString source, string originalValue, int position, TokenResult name, TokenResult value)
        {
            var param = Create(source, originalValue, position);
            param.ParamName = name;
            param.ParamValue = value;
            param.TokenType = TokenType.PARAMETER;

            return param;
        }

        /// <summary>
        /// Peek at the token beyond this token
        /// </summary>
        /// <returns>The token that comes directly after this token</returns>
        internal TokenResult PeekNext(bool recurse = true)
        {
            return Source.Peek(Position + Length, recurse);
        }

        internal TokenResult Take()
        {
            return Source.Take(this);
        }

        internal TokenResult GetNext()
        {
            return Source.GetNext();
        }

        internal TokenResult GetNext(TokenType tokenType)
        {
            return Source.GetNext(tokenType);
        }

        internal TokenResult GetNext(string value)
        {
            return Source.GetNext(value);
        }

        internal TokenResult GetNextList(TokenType listType)
        {
            return Source.GetNextList(listType);
        }

        internal TokenResult WhileGetNext(TokenType tokenType, Action<TokenResult> whileTokenSuccess)
        {
            return Source.WhileGetNext(tokenType, whileTokenSuccess);
        }

        internal TokenResult IfIsType (TokenType type, Action<TokenResult> action)
        {
            if (IsType(type))
                action.Invoke(this);

            return this;
        }

        internal bool IsListType (TokenType type)
        {
            bool isType = true;

            foreach (var item in ResultsList)
                isType &= item.IsType(type);

            return isType;
        }

        internal TokenResult IfIsListType(TokenType type, Action<TokenResult> action)
        {
            if (IsListType(type))
                action.Invoke(this);

            return this;
        }

        internal TokenResult AssertIsListType(TokenType type)
        {
            if (IsSuccess)
                foreach (var item in ResultsList)
                    if (!item.IsType(type))
                        throw new ArgumentException($"Invalid item/list type! Expected type of {type}. Item {item.OriginalValue} is of type {item.TokenType}");

            return this;
        }

        internal TokenResult OnSuccess(Action<TokenResult> action)
        {
            if (IsSuccess)
                action.Invoke(this);

            return this;
        }

        internal TokenResult OnFailure(Action<TokenResult> action)
        {
            if (!IsSuccess)
                action.Invoke(this);

            return this;
        }

        internal TokenResult OnSuccess(Func<TokenResult, TokenResult> action)
        {
            if (IsSuccess)
                return action.Invoke(this);

            return this;
        }

        internal TokenResult OnFailure(Func<TokenResult, TokenResult> action)
        {
            if (!IsSuccess)
                return action.Invoke(this);

            return this;
        }

        /// <summary>
        /// Get the value of this token as the specified type
        /// </summary>
        /// <typeparam name="T">The data type to get the value as</typeparam>
        /// <returns></returns>
        internal T Value<T>()
        {
            if (!IsSuccess) return default;
            else return (T)Convert.ChangeType(OriginalValue, typeof(T));
        }

        /// <summary>
        /// Gets the value as a string
        /// </summary>
        /// <returns></returns>
        internal string Value()
        {
            if (IsType(TokenType.VAR_NAME) && OriginalValue[0] == '$') return OriginalValue.Substring(1);
            else if (IsType(TokenType.QUOTE)) return Regex.Unescape(OriginalValue.Substring(1, OriginalValue.Length - 2));
            else return OriginalValue;
        }

        internal bool IsType(TokenType type, bool allowBackwardCasting = false)
        {
            if (type == TokenType || type == TokenType.LIST)
                return true;

            else if (TokenType == TokenType.INT && type == TokenType.DECIMAL)
                return true;

            else if (TokenType == TokenType.WORD && type == TokenType.VAR_NAME)
                return true;

            if (allowBackwardCasting && TokenType == TokenType.DECIMAL && type == TokenType.INT)
                return true;

            else if (allowBackwardCasting && TokenType == TokenType.VAR_NAME && type == TokenType.WORD)
                return true;

            else return false;
        }
    }

    public class ParsableString
    {
        internal int CursorPosition { get; private set; } = 0;
        private string _string;
        public int Length { get => _string == null ? 0 : _string.Length; }

        internal ParsableString(string stringVal)
        {
            _string = stringVal;
        }

        internal void EatWhiteSpace()
        {
            while (CursorPosition < Length && _string[CursorPosition].IsWhiteSpaceChar())
                CursorPosition++;
        }

        //Parsing functions
        internal TokenResult PeekNext(bool recurse = true)
        {
            int i = CursorPosition;

            while (i < Length && _string[i].IsWhiteSpaceChar())
                i++;

            int startingPosition = i;

            if (i >= Length)
                return TokenResult.Fail(this);

            string quoteType = null;
            bool escapeNext = false;

            for (; i < Length; i++)
            {
                if (escapeNext)
                    escapeNext = false;

                else if (quoteType != null && _string.ContainsStringAt(@"\", i))
                    escapeNext = true;

                else if (quoteType == null && _string.ContainsStringAt(new string[] { "'", "\"" }, i, out quoteType))
                    ;

                else if (quoteType != null && !escapeNext && _string.ContainsStringAt(quoteType, i))
                    quoteType = null;

                else if (_string[i] == ',' && i == startingPosition)
                {
                    i++;
                    break;
                }

                else if (quoteType == null && _string[i].IsWhiteSpaceChar() || _string[i] == ',')
                    break;
            }

            var result = TokenResult.Create(this, _string.Substring(startingPosition, i - startingPosition), startingPosition);
            if (!recurse) return result;

            //Parameter checking
            if (result.IsType(TokenType.WORD) && result.PeekNext(false).TokenType == TokenType.EQUALS)
            {
                var valueToken = result.PeekNext(false).PeekNext(false);

                if (!valueToken.IsType(TokenType.WORD) && !valueToken.IsType(TokenType.QUOTE))
                    throw new ArgumentException($"Token type {valueToken.TokenType} is an invalid type for the value of a parameter");

                result = TokenResult.CreateParam(this, _string.Substring(startingPosition, valueToken.Position + valueToken.Length - startingPosition), startingPosition, result, valueToken);
                //Type Checking?
                //result = TokenResult.Create(this, _string.Substring(startingPosition, valueToken.Position + valueToken.Length - startingPosition), startingPosition);
            }

            //List logic
            else
            {
                var list = new List<TokenResult>();
                while (result.PeekNext(false).TokenType == TokenType.COMMA)
                {
                    list.Add(result);
                    result = result.PeekNext(false).PeekNext(false);

                    //if (list.Count > )
                    //    throw new ArgumentException($"Error adding token {nextResult.OriginalValue} to a list of {result.TokenType} tokens. A list of items can not contain mixed types");
                }

                if (list.Count > 0)
                {
                    list.Add(result);
                    var lastToken = list.Last();
                    result = TokenResult.Create(this, _string.Substring(startingPosition, lastToken.Position + lastToken.Length - startingPosition), startingPosition, list);
                }
            }

            return result;
        }

        internal TokenResult Peek(int peekPosition, bool recurse = true)
        {
            if (peekPosition >= Length || peekPosition < 0)
                return TokenResult.Fail(this);

            int originalCursorPos = CursorPosition;
            CursorPosition = peekPosition;
            var result = PeekNext(recurse);
            CursorPosition = originalCursorPos;
            return result;
        }

        /// <summary>
        /// Removes the specified token result from this ParsableString
        /// </summary>
        /// <param name="tokenToTake"></param>
        /// <returns>The token that was passed in</returns>
        internal TokenResult Take(TokenResult tokenToTake)
        {
            EatWhiteSpace();

            if (tokenToTake.Source != this)
                throw new ArgumentException($"The token '{tokenToTake.OriginalValue}' is not from this ParsableString object, and therefore cannot be taken from it");

            if (!tokenToTake.IsSuccess)
                throw new ArgumentException($"A failed token result cannot be taken");

            if (tokenToTake.Position != CursorPosition)
                throw new ArgumentException($"The ParsableString is not at the starting position of the token '{tokenToTake.OriginalValue}' and therefore cannot be taken");

            CursorPosition += tokenToTake.Length;
            return tokenToTake;
        }

        /// <summary>
        /// Peeks at the next token. If it is a success, the token is taken
        /// </summary>
        /// <returns>The next token</returns>
        internal TokenResult GetNext()
        {
            var result = PeekNext();

            if (!result.IsSuccess)
                return TokenResult.Fail(this);

            result.Take();
            return result;
        }

        /// <summary>
        /// Gets the next token if the tokens value matches the specified value
        /// </summary>
        /// <param name="value">The value of the token</param>
        /// <returns></returns>
        internal TokenResult GetNext(string value)
        {
            var result = PeekNext(false);

            if (!result.IsSuccess || result.OriginalValue.ToLower() != value.ToLower())
                return TokenResult.Fail(this);

            result.Take();
            return result;
        }

        /// <summary>
        /// Gets the next token if the token is of the specified type
        /// </summary>
        /// <param name="tokenType">The type of token to get</param>
        /// <returns></returns>
        internal TokenResult GetNext(TokenType tokenType, bool recurse = true)
        {
            var result = PeekNext(recurse);

            if (!result.IsSuccess || tokenType != TokenType.LIST && !result.IsType(tokenType))
                return TokenResult.Fail(this);

            result.Take();
            return result;
        }

        internal TokenResult GetNextList(TokenType listType)
        {
            if (listType == TokenType.LIST || listType == TokenType.PARAMETER)
                throw new ArgumentException($"Invalid list type: {listType}");

            var list = PeekNext();

            try { list.AssertIsListType(listType); }
            catch { return TokenResult.Fail(this); }

            if (list.IsSuccess)
                list.Take();

            return list;
        }

        internal TokenResult WhileGetNext (TokenType tokenType, Action<TokenResult> whileTokenSuccess)
        {
            var result = GetNext(tokenType);

            while (result.IsSuccess)
            {
                whileTokenSuccess.Invoke(result);
                result = GetNext(tokenType);
            }

            return result;
        }

        internal TokenResult GetRemainder()
        {
            return TokenResult.Create(this, _string.Substring(CursorPosition), CursorPosition);
        }

        internal void VerifyAtEnd ()
        {
            EatWhiteSpace();

            if (CursorPosition != Length)
                throw new Exception($"Parsing '{_string}' failed!");
        }
    }
}
