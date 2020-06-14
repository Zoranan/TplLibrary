using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TplLib.Extensions;
using System.Threading.Tasks;
using TplLib.Functions;
using CSharpUtils.Extensions;
using Microsoft.VisualBasic.CompilerServices;

namespace TplLib
{
    /// <summary>
    /// Represents one processed result from a TplQuery
    /// </summary>
    public class TplResult
    {
        #region Static Readonly/Const Fields
        public const string DEFAULT_FIELD = "_raw";
        public static readonly string[] REQUIRED_FIELDS = { DEFAULT_FIELD, "Source", "Length" };
        public static readonly string[] READONLY_FIELDS = { "Source", "Length" };
        #endregion

        private readonly Dictionary<string, TplVariable> _fields = new Dictionary<string, TplVariable>();

        #region Properties
        public IReadOnlyDictionary<string, TplVariable> Fields { get => _fields; }
        public int Count {get => Fields.Count; }
        #endregion

        #region Constructors
        private TplResult() { }
        public TplResult(string message, string src = null)
        {
            _fields.Add(DEFAULT_FIELD, new TplVariable(message));

            if (src != null)
            {
                _fields.Add("Source", new TplVariable(src));
            }                
        }

        public TplResult Copy()
        {
            var r = new TplResult();
            foreach (var field in _fields)
                r._fields.Add(field.Key, new TplVariable(field.Value.Value));

            return r;
        }
        #endregion

        #region Comparison

        public bool Matches (TplResult right, List<string> keys=null)
        {
            if (keys == null)
            {
                keys = Fields.Keys.ToList();
            }

            bool isMatch = true;

            foreach (var k in keys)
            {
                isMatch &= StringValueOf(k) == right.StringValueOf(k);
            }

            return isMatch;
        }

        public int CompareTo(TplResult otherResult, List<TplSortField> targetFields=null)
        {
            if (targetFields == null)
            {
                targetFields = Fields.Keys.Select(f => new TplSortField(f)).ToList();
            }

            for (int i = 0; i < targetFields.Count; i++)
            {
                var sortField = targetFields[i];
                int sortDirectionMod = 1;

                //Get sort direction
                if (sortField.Descending)
                {
                    sortDirectionMod = -1; //Negate the output of string comparison if we want descending order
                }

                //Begin comparison with the current field
                var left = StringValueOf(sortField.Name);
                var right = otherResult.StringValueOf(sortField.Name);

                //Check for number values
                if (left != right && (left.Length > 0 || right.Length > 0))
                {
                    if (left.TryParseDouble(out double leftDbl)
                        && right.TryParseDouble(out double rightDbl))
                    {
                        return leftDbl.CompareTo(rightDbl) * sortDirectionMod;
                    }
                    else
                    {
                        return left.CompareTo(right) * sortDirectionMod;
                    }
                }
            }

            return 0;
        }

        #endregion

        #region Field Manipulation and Information
        public double NumericValueOf (string key)
        {
            return Fields.ContainsKey(key) ? Fields[key].NumberValue() : 0;
        }

        public bool TryNumericValueOf (string key, out double result)
        {
            var value = Fields?[key]?.NumberValue();
            result = value ?? 0;
            return value.HasValue;
        }

        public string StringValueOf (string key)
        {
            return Fields.ContainsKey(key) ? Fields[key].StringValue() : "";
        }

        public object ValueOf(string key)
        {
            return Fields.ContainsKey(key) ? Fields[key].Value : null;
        }

        public bool AddField(string key, IComparable value)
        {
            if (REQUIRED_FIELDS.Contains(key))
                throw new InvalidOperationException(key + " is reserved as readonly");

            try
            {
                _fields.Add(key, new TplVariable(value));
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public void AddOrUpdateField(string key, object value)
        {
            if (READONLY_FIELDS.Contains(key))
                throw new InvalidOperationException(key + " is reserved as readonly");

            if (_fields.ContainsKey(key))
                _fields[key].Value = value;

            else
                _fields.Add(key, new TplVariable(value));
        }

        public bool RemoveField(string key)
        {
            bool success = false;

            if (!REQUIRED_FIELDS.Contains(key))
            {
                try
                {
                    _fields.Remove(key);
                    success = true;
                }
                catch (Exception e) when (e is ArgumentNullException || e is KeyNotFoundException)
                {
                    success = false;
                }
            }

            return success;
        }

        public bool SetFieldVisibility(string key, bool visible)
        {
            try
            {
                Fields[key].Visible = visible;
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        public bool HasField(string field)
        {
            return _fields.ContainsKey(field);
        }

        #endregion

        #region String Output

        public override string ToString()
        {
            var output = new StringBuilder();
            foreach (var kv in Fields)
            {
                output.Append(kv.Key);
                output.Append(" = ");
                output.AppendLine(kv.Value.StringValue());
            }
            return output.ToString();
        }

        public string PrintValuesOnly()
        {
            var fields = Fields;
            var output = new StringBuilder();

            if (fields.Count > 0)
            {
                output.Append(fields.First().Value);

                foreach (var kv in fields.Skip(1))
                {
                    output.Append(Environment.NewLine + kv.Value);
                }
            }
            
            return output.ToString();
        }

        #endregion


        public class TplVariable
        {
            public bool Visible { get; internal set; } = true;

            private object _value;
            public object Value {
                get => _value;
                
                internal set
                {
                    _value = value;
                    CastAutoType();
                }
            }
            public TplVariable (object value) { Value = value; }

            #region Getting Value As
            internal string StringValue()
            {
                if (Value is string str) return str;
                else return Value.ToString();
            }

            internal double NumberValue()
            {
                if (Value is double dbl) return dbl;
                else if (Value is bool b) return b ? 1 : 0;
                else
                {
                    try { return Convert.ToDouble(Value); }
                    catch (Exception e) when (e is FormatException || e is InvalidCastException)
                    { return 0; }
                }
            }

            internal bool BoolValue()
            {
                if (Value is bool b) return b;
                else if (Value is double dbl) return dbl != 0;
                else return string.IsNullOrEmpty(Value as string);
            }
            #endregion

            #region Convert Stored Value Type
            internal void CastString()
            {
                _value = StringValue();
            }

            internal void CastNumber()
            {
                _value = NumberValue();
            }

            internal void CastBool()
            {
                _value = BoolValue();
            }

            internal void CastAutoType()
            {
                if (_value is string)
                {
                    try { _value = Convert.ToDouble(Value); return; }
                    catch (Exception e) when (e is FormatException || e is InvalidCastException)
                    { /*Convert Failed. Try a boolean*/ }

                    try { _value = Convert.ToBoolean(Value); return; }
                    catch (Exception e) when (e is FormatException || e is InvalidCastException)
                    { /*Convert Failed. Set to string*/ }
                }
                
                if (!_value.GetType().IsIn(typeof(double), typeof(bool), typeof(string)))
                    CastString();
            }
            #endregion

            public override string ToString()
            {
                return Visible ? Value.ToString() : null;
            }
        }
    }
}
