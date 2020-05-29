using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TplLib.Functions
{
    public class TplLast : TplFunction
    {
        private int _last;
        public int Last 
        {
            get => _last;
            internal set
            {
                if (value <= 0)
                    throw new InvalidOperationException("You must specify a value greater than 0 in 'Last' function");

                _last = value;
            }
        }
        internal TplLast() { }

        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            var skipAmnt = input.Count - Last;

            if (skipAmnt <= 0) return input;

            return input.Skip(skipAmnt).ToList();
        }
    }
}
