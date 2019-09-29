using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPL_Lib.Tpl_Parser;

namespace TPL_Lib.Functions
{
    // PIPELINE SYNTAX
    //
    // select fieldName1, fieldName2...
    //

    /// <summary>
    /// Removes all fields from a list of TplResults EXCEPT the ones specified
    /// Specify one or more fields to keep by typing the field names after the select command
    /// </summary>
    public class TplSelect : TplFunction
    {
        private List<string> _selectedFields;

        public bool RemoveEntriesWithNullValues { get; set; } = true;

        #region Constructors
        public TplSelect (List<string> fields)
        {
            _selectedFields = fields;
        }

        public TplSelect (ParsableString query)
        {
            query.GetNextList(TokenType.VAR_NAME)
                .OnSuccess(fieldList => _selectedFields = fieldList.ResultsList.Select(f => f.Value()).ToList())
                .OnFailure(_ => throw new ArgumentException($"Select function requires one or more fields to be specified for selection"))
                .Source.VerifyAtEnd();
        }
        #endregion

        #region Processing
        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            for (int j = 0; j < input.Count; j++)
            {
                var r = input[j];

                //Remove fields
                for (int i = 0; i < r.Count; i++)
                {
                    string k = r.Fields.Keys.ElementAt(i);
                    if (!_selectedFields.Contains(k))
                    {
                        if (r.RemoveField(k))
                            i--;
                    }
                }

                //Remove empty results
                bool containsAllSelectedFields = true;
                bool containsOneSelectedField = false;
                foreach (var f in _selectedFields)
                {
                    containsAllSelectedFields &= r.HasField(f);
                    containsOneSelectedField |= r.HasField(f);
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
