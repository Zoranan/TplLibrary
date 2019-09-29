using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TPL_Lib;
using TPL_Lib.Functions;

namespace TPL_Unit_Test
{
    [TestClass]
    public class TplSearchUnitTests
    {
        [TestMethod]
        public void TplSearch_Query_Constructor()
        {
            string query = "\"*\" | rex \"(?<Num>\\d+)\" | dedup Num";

            TplSearch tplSearch = new TplSearch(query);
            Assert.IsTrue(tplSearch.Query == "^.*$", "Query was not as expected: " + tplSearch.Query);

            var input = new List<TplResult>(new TplResult[] {
                new TplResult("Line 1"),
                new TplResult("Line 2"),
                new TplResult(" Line 1 "),
            });

            var res = tplSearch.Process(input);
            var ded = tplSearch.NextFunction.NextFunction as TplDedup;

            Assert.IsTrue(ded.TargetFields.Count == 1, "Target field count mismatch: " + ded.TargetFields.Count);
            Assert.IsTrue(res.Count == 2, "Expected results: 2, Actual: " + res.Count);
        }

        [TestMethod]
        public void Tpl_Explicit_Var_Test()
        {
            var exp = new TplSearch("kv rex='bit (?<key>.{3}) (?<value>[^,]+)' | where $003 == 'yes!'");
            var res = exp.Process(new List<TplResult> { new TplResult("bit 054 54 Data,bit 055 55 data, bit 003 yes!, bit _10 Normal") });
            
        }
    }
}
