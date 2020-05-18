using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TplLib.Extensions;
using TplLib.Tpl_Parser;

namespace TplLib.Functions
{
    public class TplStats : TplFunction
    {
        public bool Count { get; internal set; } = false;
        public bool Sum { get; internal set; } = false;
        public bool Avg { get; internal set; } = false;

        public List<string> TargetFields { get; internal set; }
        public List<string> ByFields { get; internal set; }

        private TplSort _sorter;

        #region Constructor
        internal TplStats() { }
        public TplStats(bool count, bool sum = false, bool avg = false, List<string> sumFields = null, List<string> byFields = null)
        {
            Count = count;
            Sum = sum;
            Avg = avg;
            TargetFields = sumFields;
            ByFields = byFields;
            _sorter = new TplSort(ByFields.Select(f => new TplSortField(f)).ToList());
        }
        #endregion

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            var results = new List<TplResult>();

            if (_sorter == null)
                _sorter = new TplSort(ByFields.Select(f => new TplSortField(f)).ToList());

            if (input.Count > 0)
            {
                #region No BY field
                //NO 'BY' FIELD
                if (ByFields == null || ByFields.Count == 0)
                {
                    var sums = new double[TargetFields.Count];

                    if (Count)
                        input[0].AddOrUpdateField("Count", input.Count.ToString());

                    if (Sum || Avg)
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
                            var formatString = input[0].StringValueOf(TargetFields[j]);

                            if (Sum)
                            {
                                var value = sums[j].FormatLikeNumber(formatString);

                                input[0].AddOrUpdateField("Sum_of_" + fieldName, value);
                            }

                            if (Avg)
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
                                var formatString = currentCombo.StringValueOf(TargetFields[j]);

                                if (Sum)
                                {
                                    var value = sums[j].FormatLikeNumber(formatString);

                                    currentCombo.AddOrUpdateField("Sum_of_" + fieldName, value);
                                }

                                if (Avg)
                                {
                                    var value = (sums[j] / currentCount).FormatLikeNumber(formatString);

                                    currentCombo.AddOrUpdateField("Avg_of_" + TargetFields[j], value);
                                }

                                sums[j] = 0;
                            }

                            if (Count)
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

                    if (Count)
                        currentCombo.AddOrUpdateField("Count", currentCount.ToString());

                    for (int j = 0; j < sums.Length; j++)
                    {
                        var fieldName = TargetFields[j];
                        var formatString = currentCombo.StringValueOf(TargetFields[j]);

                        if (Sum)
                        {
                            var value = sums[j].FormatLikeNumber(formatString);

                            currentCombo.AddOrUpdateField("Sum_of_" + fieldName, value);
                        }

                        if (Avg)
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
                if (Sum)
                    selectFields.Add("Sum_of_" + f);
                if (Avg)
                    selectFields.Add("Avg_of_" + f);
            }
            if (Count)
                selectFields.Add("Count");

            var select = new TplSelect(selectFields);
            select.RemoveEntriesWithNullValues = false;
            select.Process(results);
            #endregion

            return results;
        }
    }
}
