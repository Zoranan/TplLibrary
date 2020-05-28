using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TplLib.Tpl_Parser;

namespace TplLib.Functions
{
    /// <summary>
    /// Removes all fields from a list of TplResults EXCEPT the ones specified
    /// Specify one or more fields to keep by typing the field names after the select command
    /// </summary>
    public class TplSelect : TplFunction
    {
        public List<string> SelectedFields { get; internal set; }

        public bool RemoveEntriesWithNullValues { get; internal set; } = true;

        #region Constructors
        internal TplSelect() { }

        public TplSelect (List<string> fields)
        {
            SelectedFields = fields;
        }
        #endregion

        #region Processing
        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            for (int j = 0; j < input.Count; j++)
            {
                var result = input[j];

                //Remove fields
                for (int i = 0; i < result.Count; i++)
                {
                    var key = result.Fields.Keys.ElementAt(i);
                    if (!SelectedFields.Contains(key))
                    {
                        if (result.RemoveField(key))
                            i--;

                        else
                            result.SetFieldVisibility(key, false);
                    }
                    else
                        result.SetFieldVisibility(key, true);
                }

                //Remove empty results
                bool containsAllSelectedFields = true;
                bool containsOneSelectedField = false;
                foreach (var f in SelectedFields)
                {
                    containsAllSelectedFields &= result.HasField(f);
                    containsOneSelectedField |= result.HasField(f);
                }

                if (!containsOneSelectedField || (!containsAllSelectedFields && RemoveEntriesWithNullValues))
                {
                    input.RemoveAt(j);
                    j--;
                }

            }

            return input;
        }

        #endregion
    }
}
