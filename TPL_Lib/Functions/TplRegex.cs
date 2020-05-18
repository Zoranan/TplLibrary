using TplLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TplLib.Tpl_Parser;

namespace TplLib.Functions
{
    // PIPELINE SYNTAX
    //
    // rex fieldName "Regex"
    // rex "Regex" passthru=true (Keeps results even if they dont match)
    // rex fieldName "Regex"
    // rex "Regex" (implies using _raw)
    //

    /// <summary>
    /// Applys a specified regex to a list of TplResults.
    /// Named capture groups in the regex are stored as fields in the TplResults
    /// You can specify the field the regex will search using the 'field=name' option (or just entering the field name)
    /// The 'passthu=true' option keeps this function from removing results that dont match
    /// </summary>
    public class TplRegex : TplFunction
    {
        //private static readonly Regex _rexArgumentRegex = new Regex("^ *((?<field>[A-Za-z_]+[A-Za-z0-9_]*) +)?\"(?<reg>[^\"]*)\" *$", RegexOptions.Compiled);

        #region Properties
        public Regex Rex { get; internal set; }
        public string TargetField { get; internal set; } = TplResult.DEFAULT_FIELD;
        public bool PassThru { get; internal set; } = false;
        #endregion

        #region Constructors
        internal TplRegex() { }

        public TplRegex(Regex r, bool passthru = false, string targetField = TplResult.DEFAULT_FIELD)
        {
            Rex = r;
            TargetField = targetField;
            PassThru = passthru;
        }
        #endregion

        #region Processing
        protected override List<TplResult> InnerProcess(List<TplResult> input)
        {
            var output = new List<TplResult>();

            foreach (var r in input)
            {
                if (r.HasField(TargetField))
                {
                    var match = Rex.Match(r.StringValueOf(TargetField));

                    if (match.Success)
                    {
                        var groupNames = Rex.GetNamedCaptureGroupNames();
                        foreach (var key in groupNames)
                        {
                            r.AddOrUpdateField(key, match.Groups[key].Value);
                        }

                        if (!PassThru)
                            output.Add(r);
                    }
                }
            }

            if (PassThru)
                output = input;

            return output;
        }
        #endregion
    }
}
