using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPL_Lib.Tpl_Parser
{
    public static class StringExtensions
    {
        /// <summary>
        /// Checks if the search string is found in the input string, starting at the specified index.
        /// </summary>
        /// <param name="input">The string to search</param>
        /// <param name="search">The substring to look for</param>
        /// <param name="index">The starting index to look for the search string at</param>
        /// <returns>True if the search string is found at the specified index of the input string</returns>
        public static bool ContainsStringAt(this string input, string search, int index)
        {
            bool found = true;

            for (int i = 0; i < search.Length && i + index < input.Length && found; i++)
                found &= input[index + i] == search[i];

            return found;
        }

        /// <summary>
        /// Checks if any of the search strings are found in the input string, starting at the specified index.
        /// </summary>
        /// <param name="input">The string to search</param>
        /// <param name="searchTerms">The substrings to look for</param>
        /// <param name="index">The starting index to look for the search terms at</param>
        /// <param name="match">The matching search term (null if none matched)</param>
        /// <returns>True if any of the search terms is found at the specified index of the input string</returns>
        public static bool ContainsStringAt(this string input, string[] searchTerms, int index, out string match)
        {
            match = null;
            bool found = false;

            for (int i = 0; i < searchTerms.Length && !found; i++)
            {
                found = input.ContainsStringAt(searchTerms[i], index);

                if (found)
                    match = searchTerms[i];
            }

            return found;
        }
    }
}
