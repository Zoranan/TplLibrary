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
            if ((Sum || Avg) && !TargetFields.Any())
                throw new InvalidOperationException("Stats function requires at least one target field to be specified when calculating sum/average");

            if (!input.Any()) 
                return input;

            var results = new List<TplResult>();

            // If no 'By Fields' are specified, all results will end up in one big group
            var groupByResult = input
                .GroupBy(r => ByFields.Aggregate("", (t, b) => t + r.StringValueOf(b)))
                .Select(g => new 
                    { 
                        FinalResult = g.First().Copy(),
                        WholeGroup = g.ToList(),
                        Sums = TargetFields.ToDictionary(f => f, _ => 0.0)
                    })
                .ToList();

            var tplSelectFields = ByFields.ToList();

            // Target fields do not matter for Count
            if (Count)
                foreach (var group in groupByResult)
                {
                    var newField = "Count";
                    group.FinalResult.AddOrUpdateField(newField, group.WholeGroup.Count);
                    tplSelectFields.Add(newField);
                }

            // Target fields must be specified for Sum and Avg
            if (Sum || Avg)
            {
                //Calculate the Sum (Needed for average anyway)
                foreach (var group in groupByResult)
                {
                    foreach (var field in TargetFields)
                    {
                        var value = group.WholeGroup.Aggregate(0.0, (total, next) => total + next.NumericValueOf(field));
                        group.Sums[field] = value;
                        if (Sum)
                        {
                            var newField = field + "_Sum";
                            group.FinalResult.AddOrUpdateField(newField, value);
                            tplSelectFields.Add(newField);
                            
                        }
                    }
                }

                if (Avg)
                {
                    foreach (var group in groupByResult)
                    {
                        foreach (var field in TargetFields)
                        {
                            var newField = field + "_Avg";
                            group.FinalResult.AddOrUpdateField(newField, group.Sums[field] / group.WholeGroup.Count);
                            tplSelectFields.Add(newField);
                        }
                    }
                }
            }

            var select = new TplSelect(tplSelectFields);
            return select.Process(groupByResult.Select(g => g.FinalResult));
        }
    }
}
