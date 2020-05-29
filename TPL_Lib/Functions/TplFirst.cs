using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TplLib.Functions
{
    public class TplFirst : TplFunction
    {
        private int _first;
        public int First 
        {
            get => _first;
            internal set
            {
                if (value <= 0)
                    throw new InvalidOperationException("You must specify a value greater than 0 in 'First' function");

                _first = value;
            }
        }
        internal TplFirst() { }
        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            return input.Take(First).ToList();
        }
    }
}
