using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TPL_Lib.Extensions;
using TPL_Lib.Functions.String_Functions;

namespace TPL_Unit_Test
{
    [TestClass]
    public class GeneralTests
    {
        [TestMethod]
        public void RegexGroupReplaceTest()
        {
            var r = new Regex("hello (?<cap>\\w+)");
            var o = r.ReplaceGroup("hello world!!!", "globe", "cap");

            Assert.IsTrue(o == "hello globe!!!", o);

            r = new Regex("e(?<cap>[l]+)");
            o = r.ReplaceGroup(o, "L", "cap");

            Assert.IsTrue(o == "heLo globe!!!", o);
        }

        [TestMethod]
        public void ListSwapTest()
        {
            List<string> list = new string[] { "second", "first", "last" }.ToList();
            list.Swap(0, 1);

            Assert.IsTrue(list[0] == "first");
            Assert.IsTrue(list[1] == "second");
        }

        [TestMethod]
        public void ListModifyTest()
        {
            List<string> list = new string[] { "second", "first", "last" }.ToList();
            list.Swap(0, 1);

            Assert.IsTrue(list[0] == "first");
            Assert.IsTrue(list[1] == "second");
        }

        [TestMethod]
        public void Format_Like_Number_Test()
        {
            var money = "12345".FormatLikeNumber("$11,111");
            Assert.IsTrue(money == "$12,345", "Actual value: " + money);

            money = "12345".FormatLikeNumber("$11,111.00");
            Assert.IsTrue(money == "$12,345.00", "Actual value: " + money);

        }

        [TestMethod]
        public void Concat_Test()
        {
            var con = new TplStringConcat("\"h\" f1 \" 3\" \"  :  \" f2 f3 \" something \" f1 as Concated");

        }
    }
}
