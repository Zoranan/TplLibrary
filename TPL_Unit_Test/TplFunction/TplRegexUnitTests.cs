using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TPL_Lib.Functions;
using TPL_Lib.Extensions;

namespace TPL_Unit_Test
{
    [TestClass]
    public class TplRegexUnitTests
    {
        [TestMethod]
        public void Basic_TplRegex_Test()
        {
            TplRegex tplReg = new TplRegex("field2 \"Regex Here\"");

            Assert.IsTrue(tplReg.TargetField == "field2", "First Field capture failed: " + tplReg.TargetField);
            Assert.IsTrue(tplReg.Rex.ToString() ==  "Regex Here", "First Regex capture failed: " + tplReg.Rex);

            tplReg = new TplRegex("  \"Regex Here\" ");

            Assert.IsTrue(tplReg.TargetField == "_raw", "Second Field capture failed: " + tplReg.TargetField);
            Assert.IsTrue(tplReg.Rex.ToString() == "Regex Here", "Second Regex capture failed: " + tplReg.Rex);
        }

    }
}
