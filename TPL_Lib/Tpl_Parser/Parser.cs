using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPL_Lib.Tpl_Parser
{
    public static class Parser
    {
        /// <summary>
        /// Splits an input string into parts, as long as the split token isn't within double or single quotes. 
        /// </summary>
        /// <param name="fullQuery">The string to split</param>
        /// <param name="splitOn">The token to split on</param>
        /// <returns>A list of strings that have been split on the specified token</returns>
        public static List<string> SplitUnquoted(this string fullQuery, string splitOn="|")
        {
            var outputList = new List<string>();
            int lastSplitIndex = 0;
            string quoteType = null;
            bool escapeNext = false;

            for (int i=0; i<fullQuery.Length; i++)
            {
                if (escapeNext)
                {
                    escapeNext = false;
                }
                else if (quoteType != null && fullQuery.ContainsStringAt(@"\", i))
                {
                    escapeNext = true;
                }
                else if (quoteType == null && fullQuery.ContainsStringAt(new string[] { "'", "\"" }, i, out quoteType))
                {
                    //Do nothing
                }
                else if (quoteType != null && !escapeNext && fullQuery.ContainsStringAt(quoteType, i))
                {
                    quoteType = null;
                }
                else if (quoteType == null && fullQuery.ContainsStringAt(splitOn, i))
                {
                    outputList.Add(fullQuery.Substring(lastSplitIndex, i - lastSplitIndex).Trim());
                    lastSplitIndex = i + splitOn.Length;
                }
            }

            outputList.Add(fullQuery.Substring(lastSplitIndex).Trim());
            return outputList;
        }

        public static T ValueAs<T>(this string s)
        {
            return (T)Convert.ChangeType(s, typeof(T));
        }

        public static bool IsIntChar (this char c)
        {
            return c >= '0' && c <= '9';
        }

        public static bool IsDecimalChar(this char c)
        {
            return c == '.' || c.IsIntChar();
        }

        private static char[] _whiteSpaceChars = { ' ', '\r', '\n', '\t' };
        public static bool IsWhiteSpaceChar (this char c)
        {
            return _whiteSpaceChars.Contains(c);
        }
    }
}
