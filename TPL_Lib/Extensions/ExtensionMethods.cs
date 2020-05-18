using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TplLib.Extensions
{

    public static class ExtensionMethods
    {
        public static readonly Regex _moneyReplacementRegex = new Regex(@"[$,]", RegexOptions.Compiled);
        public static readonly Regex _moneyStringRegex = new Regex(@"(^\$?(\d{1,3})(,?\d{3})*)?(\.\d+)?$", RegexOptions.Compiled);
        //public static readonly Regex _filePathPartsRegex = new Regex(@"[$,]", RegexOptions.Compiled);


        #region Regex Operations
        //Get a list of only the NAMED capture groups from a regex
        public static List<string> GetNamedCaptureGroupNames (this Regex regex)
        {
            var names = new List<string>();

            foreach (var name in regex.GetGroupNames())
            {
                if (regex.GroupNumberFromName(name).ToString() != name)
                {
                    names.Add(name);
                }
            }

            return names;
        }

        //Get a list of only the UN-NAMED capture groups from a regex
        public static List<string> GetUnNamedCaptureGroupNumbers(this Regex regex)
        {
            var names = new List<string>();

            foreach (var name in regex.GetGroupNames())
            {
                if (regex.GroupNumberFromName(name).ToString() == name)
                {
                    names.Add(name);
                }
            }

            return names;
        }

        //Replacing the values of only certain groups on a regex
        public static string ReplaceGroup(this Regex regex, string input, string replace, string group)
        {
            if (!regex.GetNamedCaptureGroupNames().Contains(group))
                throw new ArgumentException("The specified regex does not contain a group named " + group);

            List<string> pieces = new List<string>();
            StringBuilder sb = new StringBuilder();
            var matches = regex.Matches(input);


            int currentIndex = sb.Length;
            foreach (Match m in matches)
            {
                var last = input.Substring(currentIndex, m.Index - currentIndex);
                pieces.Add(last);
                currentIndex += last.Length;

                if (m.Groups[group].Success)
                {
                    last = input.Substring(currentIndex, m.Groups[group].Index - currentIndex);
                    pieces.Add(last);
                    currentIndex += last.Length;

                    pieces.Add(null);
                    currentIndex += m.Groups[group].Length;
                }
            }

            pieces.Add(input.Substring(currentIndex));

            foreach (var p in pieces)
            {
                if (p != null)
                    sb.Append(p);

                else
                    sb.Append(replace);
            }

            return sb.ToString();
        }

        //Comparing matches
        //public static bool Contains(this Match left, Match right)
        //{
        //    return right.Index >= left.Index && (right.Index + right.Length) <= (left.Index + left.Length);
        //}

        //public static bool Intersects(this Match left, Match right)
        //{
        //    return (right.Index + right.Length) >= left.Index && right.Index <= (left.Index + left.Length);
        //}

        //public static bool StartsBefore(this Match left, Match right)
        //{
        //    return left.Index <= right.Index;
        //}

        //public static bool StartsAfter(this Match left, Match right)
        //{
        //    return right.Index >= (left.Index + left.Length);
        //}

        //public static bool IsBefore(this Match left, Match right)
        //{
        //    return (left.Index + left.Length) <= right.Index;
        //}

        //public static bool IsAfter(this Match left, Match right)
        //{
        //    return left.StartsAfter(right);
        //}
        #endregion

        #region Array Operations

        public static void Swap<T>(this IList<T> list, int index1, int index2)
        {
            var temp = list[index2];
            list[index2] = list[index1];
            list[index1] = temp;
        }

        #endregion

        #region String Operations
        //public static int ContainsCount (this string s, string sub)
        //{
        //    var searchReg = new Regex(sub);
        //    return searchReg.Matches(s).Count;
        //}

        public static double ToDouble(this string s)
        {
            if (!_moneyStringRegex.IsMatch(s))
            {
                throw new ArgumentException(s + " does not contain numeric values");
            }
            else
            {
                if (double.TryParse(_moneyReplacementRegex.Replace(s, ""), out double result))
                {
                    return result;
                }
                else
                {
                    throw new ArgumentException("Could not parse " + s + " as a numeric value");
                }
            }
        }

        public static bool TryParseDouble (this string s, out double result)
        {
            try
            {
                result = s.ToDouble();
                return true;
            }
            catch
            {
                result = 0;
                return false;
            }
        }

        #endregion

        #region Number to string conversion

        //public static string FormatLikeNumber(this string orig, string formatString)
        //{
        //    if (double.TryParse(orig, out double result))
        //        return result.FormatLikeNumber(formatString);

        //    else
        //        return orig;
        //}

        public static string FormatLikeNumber(this double dbl, string formatString)
        {
            if (formatString.Length > 0)
            {
                bool hasDollar = formatString[0] == '$';
                var decIndex = formatString.IndexOf('.');
                int decPlaces = formatString.Length - decIndex - 1;

                if (hasDollar)
                    return dbl.ToString("C2");

                else if (decIndex > -1)
                    return dbl.ToString("N" + decPlaces);

                else
                    return dbl.ToString("N");
            }
            else
                return dbl.ToString();
        }

        #endregion

        public static List<string> GetAllFields(this IEnumerable<TplResult> resultSet)
        {
            return resultSet
                .SelectMany(r => r.Fields.Keys)
                .GroupBy(f => f)
                .Select(f => f.Key)
                .ToList();
        }
    }
}
