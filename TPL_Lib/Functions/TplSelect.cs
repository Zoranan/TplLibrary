using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TplLib.Tpl_Parser;

namespace TplLib.Functions
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
        public List<string> SelectedFields { get; internal set; }

        public bool RemoveEntriesWithNullValues { get; internal set; } = true;

        #region Constructors
        internal TplSelect() { }

        public TplSelect (List<string> fields)
        {
            SelectedFields = fields;
        }

        //public TplSelect (ParsableString query)
        //{
        //    query.GetNextList(TokenType.VAR_NAME)
        //        .OnSuccess(fieldList => _selectedFields = fieldList.ResultsList.Select(f => f.Value()).ToList())
        //        .OnFailure(_ => throw new ArgumentException($"Select function requires one or more fields to be specified for selection"))
        //        .Source.VerifyAtEnd();
        //}
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
                    if (!SelectedFields.Contains(k))
                    {
                        if (r.RemoveField(k))
                            i--;
                    }
                }

                //Remove empty results
                bool containsAllSelectedFields = true;
                bool containsOneSelectedField = false;
                foreach (var f in SelectedFields)
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
