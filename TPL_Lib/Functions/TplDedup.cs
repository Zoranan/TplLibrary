using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TplLib;
using TplLib.Extensions;
using TplLib.Functions;
using TplLib.Tpl_Parser;

namespace TplLib.Functions
{
    // PIPELINE SYNTAX
    //
    // dedup Field1, Field2, Field3...
    // dedup Field1, Field2 consecutive=true sort=(last|first)...
    //
    public class TplDedup : TplFunction
    {
        #region Enums
        public enum DedupMode { All, Consecutive }
        public enum DedupSort { First, Last }
        #endregion

        #region Properties
        internal List<string> TargetFields { get; set; }
        /// <summary>
        /// Determines how duplicates are detected; consecutively or through the whole result set
        /// </summary>
        internal DedupMode Mode { get; set; } = DedupMode.All;
        /// <summary>
        /// Determines which duplicate item is kept, the first or last instance
        /// </summary>
        internal DedupSort SortMode { get; set; } = DedupSort.Last;
        #endregion

        #region Constructors

        internal TplDedup() { }

        public TplDedup(List<string> targetFields, DedupMode mode = DedupMode.All, DedupSort sortMode = DedupSort.Last)
        {
            TargetFields = targetFields;
            Mode = mode;
            SortMode = sortMode;
        }
        #endregion

        #region Processing

        protected override List<TplResult> InnerProcess(List<TplResult> inputs)
        {
            var results = new List<TplResult>();

            if (inputs.Count > 0)
            {
                results.Add(inputs[0]);

                var targetFields = TargetFields == null || !TargetFields.Any() ? inputs.GetAllFields() : TargetFields;

                if (Mode == DedupMode.All)
                {
                    for (int j = 1; j < inputs.Count; j++)
                    {
                        var input = inputs[j];
                        bool alreadyAdded = false;

                        for (int i = 0; i < results.Count; i++)
                        {
                            var result = results[i];

                            alreadyAdded |= result.Matches(input, targetFields);

                            if (alreadyAdded)
                            {
                                if (SortMode == DedupSort.Last)
                                {
                                    results[i] = input;
                                }
                                break;
                            }
                        }

                        if (!alreadyAdded)
                        {
                            results.Add(input);
                        }

                    }
                }

                else if (Mode == DedupMode.Consecutive)
                {
                    foreach (var result in inputs)
                    {
                        if (!result.Matches(results.Last(), targetFields))
                        {
                            results.Add(result);
                        }
                    }
                }
            }

            return results;
        }
        #endregion
    }
}
