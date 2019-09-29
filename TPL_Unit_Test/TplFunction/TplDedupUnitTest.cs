using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TPL_Lib;
using TPL_Lib.Functions;

namespace TPL_Unit_Test
{
    [TestClass]
    public class TplDedupUnitTest
    {
        [TestMethod]
        public void TplDedup_Query_String_Constructor_Test()
        {
            //fields first, then parameters
            TplDedup tplDed = new TplDedup("field1, field_2  Field__3, sort=last, consecutive=true");

            Assert.IsTrue(tplDed.SortMode == TplDedup.DedupSort.Last, "Sort mode was " + tplDed.SortMode);
            Assert.IsTrue(tplDed.Mode == TplDedup.DedupMode.Consecutive, "Dedup mode was " + tplDed.Mode);

            //parameters first, then fields
            tplDed = new TplDedup("sort=first, consecutive=true field1, field_2  Field__3");

            Assert.IsTrue(tplDed.SortMode == TplDedup.DedupSort.First, "Sort mode was " + tplDed.SortMode);
            Assert.IsTrue(tplDed.Mode == TplDedup.DedupMode.Consecutive, "Dedup mode was " + tplDed.Mode);
            Assert.IsTrue(tplDed.TargetFields.Count == 3, "Only " + tplDed.TargetFields.Count + "/3 fields were extracted");

            //parameter, then field(s), then parameter (non default values)
            tplDed = new TplDedup(" consecutive=true field1, field_2  Field__3, sort=first");

            Assert.IsTrue(tplDed.SortMode == TplDedup.DedupSort.First, "Sort mode was " + tplDed.SortMode);
            Assert.IsTrue(tplDed.Mode == TplDedup.DedupMode.Consecutive, "Dedup mode was " + tplDed.Mode);
            Assert.IsTrue(tplDed.TargetFields.Count == 3, "Only " + tplDed.TargetFields.Count + "/3 fields were extracted");

            //fields only
            tplDed = new TplDedup("field1, field_2");

            Assert.IsTrue(tplDed.TargetFields.Count == 2, "Only " + tplDed.TargetFields.Count + "/2 fields were extracted");
            Assert.IsTrue(tplDed.SortMode == TplDedup.DedupSort.Last, "Sort mode was " + tplDed.SortMode);
            Assert.IsTrue(tplDed.Mode == TplDedup.DedupMode.All, "Dedup mode was " + tplDed.Mode);

            //param(s) only
            tplDed = new TplDedup("  sort =  first");

            Assert.IsTrue(tplDed.TargetFields.Count == 1, tplDed.TargetFields.Count + " fields were extracted");
            Assert.IsTrue(tplDed.SortMode == TplDedup.DedupSort.First, "Sort mode was " + tplDed.SortMode);
            Assert.IsTrue(tplDed.Mode == TplDedup.DedupMode.All, "Dedup mode was " + tplDed.Mode);

            //blank query should have default values
            tplDed = new TplDedup();

            Assert.IsTrue(tplDed.TargetFields.Count == 1, tplDed.TargetFields.Count + " fields were extracted");
            Assert.IsTrue(tplDed.SortMode == TplDedup.DedupSort.Last, "Sort mode was " + tplDed.SortMode);
            Assert.IsTrue(tplDed.Mode == TplDedup.DedupMode.All, "Dedup mode was " + tplDed.Mode);
        }

        [TestMethod]
        public void TplDedup_Smoke_Test()
        {
            var input = new List<TplResult>(new TplResult[] {
                new TplResult("Line 1"),
                new TplResult("Line 2"),
                new TplResult(" Line 1 "),
            });


        }

    }
}
