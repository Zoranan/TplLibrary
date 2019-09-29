using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TPL_Lib.Extensions;
using System.Threading.Tasks;

namespace TPL_Lib
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

        private Dictionary<string, string> _fields = new Dictionary<string, string>();

        #region Properties
        public IReadOnlyDictionary<string, string> Fields
        {
            get
            {
                var d = new Dictionary<string, string>();

                foreach (var s in SelectedFields)
                {
                    if (_fields.ContainsKey(s))
                        d.Add(s, _fields[s]);
                }

                return d;
            }
        }
        public List<string> SelectedFields { get; private set; } = new List<string>();
        public int Count {get { return Fields.Count; } }
        #endregion

        #region Constructors
        public TplResult(string message, string src = null)
        {
            _fields.Add(DEFAULT_FIELD, message);
            SelectedFields.Add(DEFAULT_FIELD);
            _fields.Add("Length", message.Length.ToString());
            //SelectedFields.Add("Length");

            if (src != null)
            {
                _fields.Add("Source", src);
                //SelectedFields.Add("Source");
            }                
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
                isMatch &= ValueOf(k) == right.ValueOf(k);
            }

            return isMatch;
        }

        public int CompareTo(TplResult otherResult, List<string> targetFields=null)
        {
            if (targetFields == null)
            {
                targetFields = Fields.Keys.ToList();
            }

            for (int i = 0; i < targetFields.Count; i++)
            {
                var sortField = targetFields[i];
                int sortDirectionMod = 1;

                //Get sort direction
                if (sortField[0] == '+')
                {
                    sortField = sortField.Substring(1);
                }
                else if (sortField[0] == '-')
                {
                    sortDirectionMod = -1; //Negate the output of string comparison if we want descending order
                    sortField = sortField.Substring(1);
                }

                //Begin comparison with the current field
                var left = ValueOf(sortField);
                var right = otherResult.ValueOf(sortField);

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
            string sValue;
            try
            {
                sValue = Fields[key];
            }
            catch (KeyNotFoundException)
            {
                return 0;
            }

            return sValue.ToDouble();

        }

        public bool TryNumericValueOf (string key, out double result)
        {
            try
            {
                result = NumericValueOf(key);
                return true;
            }
            catch
            {
                result = 0;
                return false;
            }
        }

        public string ValueOf(string key)
        {
            try
            {
                return _fields[key];
            }
            catch (KeyNotFoundException)
            {
                return "";
            }
        }

        public bool AddField(string key, string value)
        {
            if (REQUIRED_FIELDS.Contains(key))
                throw new InvalidOperationException(key + " is reserved as readonly");

            try
            {
                _fields.Add(key, value);
                SelectedFields.Add(key);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public void AddOrUpdateField(string key, string value)
        {
            if (READONLY_FIELDS.Contains(key))
                throw new InvalidOperationException(key + " is reserved as readonly");

            else if (key == DEFAULT_FIELD)
                _fields["Length"] = value.Length.ToString();

            try
            {
                _fields.Add(key, value);
                SelectedFields.Add(key);
            }
            catch (ArgumentException)
            {
                _fields[key] = value;
            }
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
                catch (ArgumentNullException)
                {
                    success = false;
                }
            }

            success |= SelectedFields.Remove(key);
            return success;
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
                output.AppendLine(kv.Value);
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
                    output.AppendLine(Environment.NewLine + kv.Value);
                }
            }
            
            return output.ToString();
        }

        #endregion

    }
}
