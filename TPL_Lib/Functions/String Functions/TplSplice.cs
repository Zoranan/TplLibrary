using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextFormatterLanguage;
using TplLib.Tpl_Parser;

namespace TplLib.Functions.String_Functions
{
    public class TplSplice : TplFunction
    {
        private readonly SpliceFormatter _splicer;
        public string TargetField { get; internal set; } = TplResult.DEFAULT_FIELD;
        public string AsField { get; internal set; }

        internal TplSplice(string splice) { _splicer = new SpliceFormatter(splice); }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            var results = input.ToList();

            Parallel.ForEach(results, result =>
            {
                if (result.HasField(TargetField))
                {
                    result.AddOrUpdateField(AsField, _splicer.Format(result.StringValueOf(TargetField)));
                }
            });

            return results;
        }
    }
}
