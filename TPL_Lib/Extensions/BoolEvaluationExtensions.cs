using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpUtils.Extensions
{
    public static class BoolEvaluationExtensions
    {
        /// <summary>
        /// Checks if the input object is equal to any of the elements in the list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputObject"></param>
        /// <param name="inList"></param>
        /// <returns></returns>
        public static bool IsIn<T> (this T inputObject, params T[] inList)
        {
            if (inputObject == null) 
                throw new ArgumentNullException(nameof(inputObject));

            return inList.Contains(inputObject);
        }

        /// <summary>
        /// Checks if the input object is equal to, or between the specified boundaries
        /// </summary>
        /// <param name="inputObject">The object to check bounds on</param>
        /// <param name="b1">The first boundary</param>
        /// <param name="b2">The second boundary</param>
        /// <returns></returns>
        public static bool IsBetween<T> (this T inputObject, T b1, T b2) where T : IComparable<T>
        {
            if (b1.CompareTo(b2) > 0)
            {
                var temp = b1;
                b1 = b2;
                b2 = temp;
            }

            return inputObject.CompareTo(b1) >= 0 
                && inputObject.CompareTo(b2) <= 0;
        }

        /// <summary>
        /// Checks if the input object between the specified boundaries, but not equal to either of them.
        /// </summary>
        /// <param name="inputObject">The object to check bounds on</param>
        /// <param name="b1">The first boundary</param>
        /// <param name="b2">The second boundary</param>
        /// <returns></returns>
        public static bool IsBetweenExclusive<T> (this T inputObject, T b1, T b2) where T : IComparable<T>
        {
            return inputObject.IsBetween(b1, b2) 
                || inputObject.CompareTo(b1) != 0 
                || inputObject.CompareTo(b2) != 0;
        }
    }
}
