using System;

namespace CSharpUtils.Extensions
{
    public static class TypeConversionExtensions
    {
        #region General Type Conversion using 'Convert.ChangeType()'
        /// <summary>
        /// Attempts to convert the inputObject to the specified type. Returns the default value if the conversion fails.
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="inputObject">The object to convert</param>
        /// <param name="defaultValue">The value to return if conversion fails</param>
        /// <returns>The converted or default value</returns>
        public static T AsOrDefault<T>(this IConvertible inputObject, T defaultValue = default)
        {
            try
            {
                return inputObject.As<T>();
            }
            catch (Exception e) when 
            (e is InvalidCastException 
            || e is FormatException 
            || e is OverflowException 
            || e is ArgumentNullException)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Attempts to convert the inputObject to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="inputObject"></param>
        /// <exception cref="InvalidCastException"/>
        /// <exception cref="FormatException"/>
        /// <exception cref="OverflowException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <returns></returns>
        public static T As<T>(this IConvertible inputObject)
        {
            return (T)Convert.ChangeType(inputObject, typeof(T));
        }
        #endregion

        #region Enum Parsing
        /// <summary>
        /// Parse this string into the specified Enum type, or return the default value
        /// </summary>
        /// <typeparam name="TEnum">The Enum type to parse</typeparam>
        /// <param name="enumString">The string to parse</param>
        /// <param name="defaultValue">The value that will be returned if the conversion fails</param>
        /// <returns></returns>
        public static TEnum ParseEnumOrDefault<TEnum>(this string enumString, TEnum defaultValue = default)
        {
            return enumString.ParseEnumOrDefault(true, defaultValue);
        }

        /// <summary>
        /// Parse this string into the specified Enum type, or return the default value
        /// </summary>
        /// <typeparam name="TEnum">The Enum type to parse</typeparam>
        /// <param name="enumString">The string to parse</param>
        /// <param name="ignoreCase"></param>
        /// <param name="defaultValue">The value that will be returned if the conversion fails</param>
        /// <returns></returns>
        public static TEnum ParseEnumOrDefault<TEnum>(this string enumString, bool ignoreCase, TEnum defaultValue = default)
        {
            try
            {
                return enumString.ParseEnum<TEnum>(ignoreCase);
            }
            catch (Exception e) when
            (e is ArgumentException
            || e is OverflowException)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Parse this string into the specified Enum type
        /// </summary>
        /// <typeparam name="TEnum">The Enum type to parse</typeparam>
        /// <param name="enumString">The string to parse</param>
        /// <param name="ignoreCase"></param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="OverflowException"/>
        /// <returns></returns>
        public static TEnum ParseEnum<TEnum>(this string enumString, bool ignoreCase = true)
        {
            var t = typeof(TEnum);

            if (!t.IsEnum)
                throw new ArgumentException($"Please provide an Enum type. '{t}' is not an Enum type.", nameof(TEnum));

            if (enumString == null)
                throw new ArgumentNullException(nameof(enumString));

            enumString = enumString.Trim();

            if (enumString.Length == 0)
                throw new FormatException($"{nameof(enumString)} only contains whitespace. Please provide a valid string value to parse into an enum.");

            return (TEnum)Enum.Parse(t, enumString, ignoreCase);
        }
        #endregion
    }
}
