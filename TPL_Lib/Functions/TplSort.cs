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
    public class TplSortField
    {
        public readonly string Name;
        public readonly bool Descending;

        public TplSortField(string name, bool desc = false)
        {
            Name = name;
            Descending = desc;
        }
    }

    /// <summary>
    /// Sorts a list of TplResults by one or more of their fields
    /// </summary>
    public class TplSort : TplFunction
    {
        #region Fields
        private List<TplSortField> _targetFields = new List<TplSortField>() { new TplSortField(TplResult.DEFAULT_FIELD) };
        #endregion

        #region Properties
        public List<TplSortField> TargetFields
        {
            get
            {
                return _targetFields;
            }
            internal set
            {
                if (value != null)
                {
                    _targetFields = value;
                }
            }
        }
        #endregion

        #region Constructors
        internal TplSort() { }

        public TplSort(List<TplSortField> targetFields=null)
        {
            TargetFields = targetFields;
        }
        #endregion

        #region Processing
        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            if (input.Count > 1)
            {
                int startPoint = 0;
                int endPoint = input.Count - 1;

                var sortFields = TargetFields.Any() ? TargetFields : input.GetAllFields().Select(f => new TplSortField(f)).ToList();

                QuickSort(input, startPoint, endPoint, sortFields);
            }

            return input;
        }

        private void QuickSort (List<TplResult> input, int startPoint, int endPoint, List<TplSortField> sortFields)
        {
            if (startPoint < endPoint)
            {
                //Bounds checks
                if (startPoint < 0)
                    startPoint = 0;

                if (endPoint > input.Count - 1)
                    endPoint = input.Count - 1;

                //Select the pivot
                int pivotPoint = endPoint;
                var pivotObject = input[pivotPoint];

                int i = startPoint;
                for (int j = startPoint; j < endPoint; j++)
                {
                    var comparison = input[j].CompareTo(pivotObject, sortFields);
                    if (comparison < 0)
                    {
                        input.Swap(i, j);
                        i++;
                    }
                }

                input.Swap(i, endPoint);
                QuickSort(input, startPoint, i - 1, sortFields);
                QuickSort(input, i + 1, endPoint, sortFields);
            }

        }
        #endregion
    }
}
