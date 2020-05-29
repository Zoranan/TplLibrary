using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TplLib.Functions
{
    public class TplDelete : TplFunction
    {
        public List<string> SelectedFields { get; internal set; }

        internal TplDelete() { }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            foreach (var result in input)
                foreach (var field in SelectedFields)
                    if (!result.RemoveField(field))
                        result.SetFieldVisibility(field, false);

            return input;
        }
    }
}
