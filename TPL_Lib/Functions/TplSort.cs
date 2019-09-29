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
    // PIPELINE SYNTAX
    //
    // sort (+-)Field1, (+-)Field2, (+-)Field3...
    //

    /// <summary>
    /// Sorts a list of TplResults by one or more of their fields
    /// </summary>
    public class TplSort : TplFunction
    {
        #region Fields
        private List<string> _targetFields = new List<string>() { TplResult.DEFAULT_FIELD };
        #endregion

        #region Properties
        public List<string> TargetFields
        {
            get
            {
                return _targetFields;
            }
            private set
            {
                if (value != null)
                {
                    _targetFields = value;
                }
            }
        }
        #endregion

        #region Constructors
        public TplSort (List<string> targetFields=null)
        {
            TargetFields = targetFields;
        }

        public TplSort (ParsableString query)
        {
            query.GetNextList(TokenType.VAR_NAME)
                .OnSuccess(list => TargetFields = list.ResultsList.Select(f => f.Value()).ToList())
                .Source.VerifyAtEnd();
        }
        #endregion

        #region Processing
        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            if (input.Count > 1)
            {
                int startPoint = 0;
                int endPoint = input.Count - 1;

                QuickSort(input, startPoint, endPoint);
            }

            return input;
        }

        private void QuickSort (List<TplResult> input, int startPoint, int endPoint)
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
                    var comparison = input[j].CompareTo(pivotObject, TargetFields);
                    if (comparison < 0)
                    {
                        input.Swap(i, j);
                        i++;
                    }
                }

                input.Swap(i, endPoint);
                QuickSort(input, startPoint, i - 1);
                QuickSort(input, i + 1, endPoint);
            }

        }
        #endregion
    }
}
