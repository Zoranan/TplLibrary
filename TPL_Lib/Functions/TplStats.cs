using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TPL_Lib.Extensions;
using TPL_Lib.Tpl_Parser;

namespace TPL_Lib.Functions
{
    public class TplStats : TplFunction
    {
        public bool _count;
        public bool _sum;
        public bool _avg;

        public List<string> TargetFields { get; private set; }
        public List<string> ByFields { get; private set; }

        private TplSort _sorter;

        #region Constructor
        public TplStats(bool count, bool sum = false, bool avg = false, List<string> sumFields = null, List<string> byFields = null)
        {
            _count = count;
            _sum = sum;
            _avg = avg;
            TargetFields = sumFields;
            ByFields = byFields;
            _sorter = new TplSort(ByFields);
        }

        public TplStats(ParsableString query)
        {
            query.GetNextList(TokenType.WORD)
                .OnSuccess(actions =>
                {
                    foreach (var a in actions.ResultsList)
                        switch (a.Value().ToLower())
                        {
                            case "count":
                                _count = true;
                                break;
                            case "sum":
                                _sum = true;
                                break;
                            case "avg":
                                _avg = true;
                                break;
                            default:
                                throw new ArgumentException($"Invalid stat type {a.Value()}. Expected 'count', 'sum',  and/or 'avg'");
                        }
                })
                .OnFailure(_ => throw new ArgumentException("Must specify at least one stat type for the stats function. 'count', 'sum', and/or 'avg'"))
                
                .GetNextList(TokenType.VAR_NAME)
                .OnSuccess(fields => TargetFields = fields.ResultsList.Select(f => f.Value()).ToList())
                .OnFailure(_ => TargetFields = new List<string>() { TplResult.DEFAULT_FIELD })
                
                .GetNext("by")
                .OnSuccess(by =>
                {
                    return by.GetNext(TokenType.LIST)
                    .AssertIsListType(TokenType.VAR_NAME)
                    .OnSuccess(fields => ByFields = fields.ResultsList.Select(f => f.Value()).ToList())
                    .OnFailure(_ => throw new ArgumentException($"Expected a list of fields to group stats by in stats function"));
                })
                .Source.VerifyAtEnd();

            _sorter = new TplSort(ByFields);
        }
        #endregion

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            var results = new List<TplResult>();

            if (input.Count > 0)
            {
                #region No BY field
                //NO 'BY' FIELD
                if (ByFields == null || ByFields.Count == 0)
                {
                    var sums = new double[TargetFields.Count];

                    if (_count)
                        input[0].AddOrUpdateField("Count", input.Count.ToString());

                    if (_sum || _avg)
                    {
                        //Populate index 0 of sum array
                        for (int j = 0; j < sums.Length; j++)
                        {
                            input[0].TryNumericValueOf(TargetFields[j], out double sum);
                            sums[j] = sum;
                        }

                        //Add the values of each target field from each result to its respective sum
                        int resultsToAvgBy = 0;
                        for (int i = 1; i < input.Count; i++)
                        {
                            for (int j = 0; j < sums.Length; j++)
                            {
                                if (input[i].TryNumericValueOf(TargetFields[j], out double result))
                                {
                                    resultsToAvgBy++;
                                    sums[j] += result;
                                }
                            }
                        }


                        for (int j = 0; j < sums.Length; j++)
                        {
                            var fieldName = TargetFields[j];
                            var formatString = input[0].ValueOf(TargetFields[j]);

                            if (_sum)
                            {
                                var value = sums[j].FormatLikeNumber(formatString);

                                input[0].AddOrUpdateField("Sum_of_" + fieldName, value);
                            }

                            if (_avg)
                            {
                                var value = (sums[j] / resultsToAvgBy).FormatLikeNumber(formatString);

                                input[0].AddOrUpdateField("Avg_of_" + TargetFields[j], value);
                            }
                        }
                    }
                    

                    results.Add(input[0]);
                }
                #endregion

                #region Have BY Field(s)
                //Only made it here if we have a 'BY'
                else
                {
                    _sorter.Process(input);

                    var currentCombo = input[0];
                    
                    //Set up temp variables
                    var sums = new double[TargetFields.Count];
                    var divBy = new int[TargetFields.Count];
                    int currentCount = 1;

                    for (int j = 0; j < sums.Length; j++)
                    {
                        currentCombo.TryNumericValueOf(TargetFields[j], out double sum);
                        sums[j] = sum;
                        divBy[j] = 0;
                    }


                    for (int i = 1; i < input.Count; i++)
                    {
                        //Store current values
                        if (ByFields != null && !currentCombo.Matches(input[i], ByFields))
                        {
                            for (int j = 0; j < sums.Length; j++)
                            {
                                var fieldName = TargetFields[j];
                                var formatString = currentCombo.ValueOf(TargetFields[j]);

                                if (_sum)
                                {
                                    var value = sums[j].FormatLikeNumber(formatString);

                                    currentCombo.AddOrUpdateField("Sum_of_" + fieldName, value);
                                }

                                if (_avg)
                                {
                                    var value = (sums[j] / currentCount).FormatLikeNumber(formatString);

                                    currentCombo.AddOrUpdateField("Avg_of_" + TargetFields[j], value);
                                }

                                sums[j] = 0;
                            }

                            if (_count)
                            {
                                currentCombo.AddOrUpdateField("Count", currentCount.ToString());
                            }

                            currentCount = 0;
                            results.Add(currentCombo);
                            currentCombo = input[i];
                        }

                        for (int j = 0; j < sums.Length; j++)
                        {
                            if (input[i].TryNumericValueOf(TargetFields[j], out double result))
                            {
                                sums[j] += result;
                                divBy[j]++;
                            }
                        }

                        currentCount++;
                    }

                    if (_count)
                        currentCombo.AddOrUpdateField("Count", currentCount.ToString());

                    for (int j = 0; j < sums.Length; j++)
                    {
                        var fieldName = TargetFields[j];
                        var formatString = currentCombo.ValueOf(TargetFields[j]);

                        if (_sum)
                        {
                            var value = sums[j].FormatLikeNumber(formatString);

                            currentCombo.AddOrUpdateField("Sum_of_" + fieldName, value);
                        }

                        if (_avg)
                        {
                            var value = (sums[j] / divBy[j]).FormatLikeNumber(formatString);

                            currentCombo.AddOrUpdateField("Avg_of_" + TargetFields[j], value);
                        }
                    }

                    results.Add(currentCombo);
                }
                #endregion
            }

            #region Filtering the results
            //Filter uneeded fields from the results
            var selectFields = new List<string>();

            if (ByFields != null)
                selectFields.AddRange(ByFields);

            foreach (var f in TargetFields)
            {
                if (_sum)
                    selectFields.Add("Sum_of_" + f);
                if (_avg)
                    selectFields.Add("Avg_of_" + f);
            }
            if (_count)
                selectFields.Add("Count");

            var select = new TplSelect(selectFields);
            select.RemoveEntriesWithNullValues = false;
            select.Process(results);
            #endregion

            return results;
        }
    }
}
