using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TPL_Lib;
using TPL_Lib.Functions;
using TPL_Lib.Tpl_Parser;

namespace TPL_Lib.Functions
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
        private List<string> TargetFields { get; set; }
        /// <summary>
        /// Determines how duplicates are detected; consecutively or through the whole result set
        /// </summary>
        private DedupMode Mode { get; set; } = DedupMode.All;
        /// <summary>
        /// Determines which duplicate item is kept, the first or last instance
        /// </summary>
        private DedupSort SortMode { get; set; } = DedupSort.Last;
        #endregion

        #region Constructors

        public TplDedup(List<string> targetFields, DedupMode mode = DedupMode.All, DedupSort sortMode = DedupSort.Last)
        {
            TargetFields = targetFields;
            Mode = mode;
            SortMode = sortMode;
        }


        public TplDedup (ParsableString query)
        {
            query.GetNextList(TokenType.VAR_NAME)
                .OnSuccess(fields => TargetFields = fields.ResultsList.Select(r => r.Value()).ToList())
                .OnFailure(_ => TargetFields = new List<string>() { TplResult.DEFAULT_FIELD })

                .WhileGetNext(TokenType.PARAMETER, param =>
                {
                    switch (param.ParamName.Value().ToLower())
                    {
                        case "consecutive":
                            Mode = param.ParamValue.Value<bool>() ? DedupMode.Consecutive : DedupMode.All;
                            break;

                        case "sort":

                            if (Enum.TryParse(param.ParamValue.Value(), out DedupSort dedupSort))
                                SortMode = dedupSort;

                            else
                                throw new ArgumentException($"Invalid value for parameter 'sort' in dedup function. Expected 'first', or 'last'");
                            break;

                        default:
                            throw new ArgumentException($"Invalid parameter '{param.ParamName}' in dedup function");
                    }
                })
                .Source.VerifyAtEnd();
        }

        #endregion

        #region Processing

        protected override List<TplResult> InnerProcess(List<TplResult> inputs)
        {
            var results = new List<TplResult>();

            if (inputs.Count > 0)
            {
                results.Add(inputs[0]);

                if (TargetFields == null)
                {
                    TargetFields = inputs[0].Fields.Keys.ToList();
                }

                if (Mode == DedupMode.All)
                {
                    for (int j = 1; j < inputs.Count; j++)
                    {
                        var input = inputs[j];
                        bool alreadyAdded = false;

                        for (int i = 0; i < results.Count; i++)
                        {
                            var result = results[i];

                            alreadyAdded |= result.Matches(input, TargetFields);

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
                        if (!result.Matches(results.Last(), TargetFields))
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
