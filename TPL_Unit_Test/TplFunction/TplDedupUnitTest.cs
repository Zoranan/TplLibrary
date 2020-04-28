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
