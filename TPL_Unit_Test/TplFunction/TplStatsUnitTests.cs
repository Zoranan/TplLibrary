using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TPL_Lib.Functions;

namespace TPL_Unit_Test.TplFunction
{
    [TestClass]
    public class TplStatsUnitTests
    {
        [TestMethod]
        public void Constructor_Testing()
        {
            //Count
            var tplStat = new TplStats("count");
            Assert.IsFalse(tplStat._sum, "Sum was true");
            Assert.IsTrue(tplStat._count, "Count was false");

            //Count sum
            tplStat = new TplStats("count sum price");
            Assert.IsTrue(tplStat._sum, "Sum was false");
            Assert.IsTrue(tplStat._count, "Count was false");
            Assert.IsTrue(tplStat.TargetFields.Count == 1);
            Assert.IsTrue(tplStat.TargetFields.Contains("price"));
            Assert.IsTrue(tplStat.ByFields == null);

            tplStat = new TplStats("count sum price, tax");
            Assert.IsTrue(tplStat._sum, "Sum was false");
            Assert.IsTrue(tplStat._count, "Count was false");
            Assert.IsTrue(tplStat.TargetFields.Count == 2);
            Assert.IsTrue(tplStat.TargetFields.Contains("price"));
            Assert.IsTrue(tplStat.TargetFields.Contains("tax"));
            Assert.IsTrue(tplStat.ByFields == null);

            //Sum
            tplStat = new TplStats("sum field1");
            Assert.IsTrue(tplStat._sum, "Sum was false");
            Assert.IsFalse(tplStat._count, "Count was true");
            Assert.IsTrue(tplStat.TargetFields.Count == 1);
            Assert.IsTrue(tplStat.TargetFields.Contains("field1"));

            //Sum Count
            tplStat = new TplStats("sum count price");
            Assert.IsTrue(tplStat._sum, "Sum was false");
            Assert.IsTrue(tplStat._count, "Count was false");
            Assert.IsTrue(tplStat.TargetFields.Count == 1);
            Assert.IsTrue(tplStat.TargetFields.Contains("price"));
            Assert.IsTrue(tplStat.ByFields == null);

            tplStat = new TplStats("sum count price, tax");
            Assert.IsTrue(tplStat._sum, "Sum was false");
            Assert.IsTrue(tplStat._count, "Count was false");
            Assert.IsTrue(tplStat.TargetFields.Count == 2);
            Assert.IsTrue(tplStat.TargetFields.Contains("price"));
            Assert.IsTrue(tplStat.TargetFields.Contains("tax"));
            Assert.IsTrue(tplStat.ByFields == null);

            //Sum not field fail check
            try
            {
                tplStat = new TplStats("sum");
                Assert.IsTrue(false, "Constructor allowed sum without field(s)");
            }
            catch { Assert.IsTrue(true); }
            
            //All of the above with by clauses
            //Count
            tplStat = new TplStats("count by field1");
            Assert.IsFalse(tplStat._sum, "Sum was true");
            Assert.IsTrue(tplStat._count, "Count was false");
            Assert.IsTrue(tplStat.ByFields.Contains("field1"), "by clause field name did not match expected");
            
            //Count sum
            tplStat = new TplStats("count sum price by field1 field2");
            Assert.IsTrue(tplStat._sum, "Sum was false");
            Assert.IsTrue(tplStat._count, "Count was false");
            Assert.IsTrue(tplStat.TargetFields.Count == 1);
            Assert.IsTrue(tplStat.TargetFields.Contains("price"));
            Assert.IsTrue(tplStat.ByFields.Contains("field1"), "by clause field name did not match expected");
            Assert.IsTrue(tplStat.ByFields.Contains("field2"), "by clause field name did not match expected");
            
            tplStat = new TplStats("count sum price, tax by field1, field2");
            Assert.IsTrue(tplStat._sum, "Sum was false");
            Assert.IsTrue(tplStat._count, "Count was false");
            Assert.IsTrue(tplStat.TargetFields.Count == 2);
            Assert.IsTrue(tplStat.TargetFields.Contains("price"));
            Assert.IsTrue(tplStat.TargetFields.Contains("tax"));
            Assert.IsTrue(tplStat.ByFields.Contains("field1"), "by clause field name did not match expected");
            Assert.IsTrue(tplStat.ByFields.Contains("field2"), "by clause field name did not match expected");

            //Sum
            tplStat = new TplStats("sum field1 by day");
            Assert.IsTrue(tplStat._sum, "Sum was false");
            Assert.IsFalse(tplStat._count, "Count was true");
            Assert.IsTrue(tplStat.TargetFields.Count == 1);
            Assert.IsTrue(tplStat.TargetFields.Contains("field1"));
            Assert.IsTrue(tplStat.ByFields.Contains("day"), "by clause field name did not match expected");

            //Sum Count
            tplStat = new TplStats("sum count price by store, day");
            Assert.IsTrue(tplStat._sum, "Sum was false");
            Assert.IsTrue(tplStat._count, "Count was false");
            Assert.IsTrue(tplStat.TargetFields.Count == 1);
            Assert.IsTrue(tplStat.TargetFields.Contains("price"));
            Assert.IsTrue(tplStat.ByFields.Contains("store"), "by clause field name did not match expected");
            Assert.IsTrue(tplStat.ByFields.Contains("day"), "by clause field name did not match expected");


            tplStat = new TplStats("sum count price, tax by store day");
            Assert.IsTrue(tplStat._sum, "Sum was false");
            Assert.IsTrue(tplStat._count, "Count was false");
            Assert.IsTrue(tplStat.TargetFields.Count == 2);
            Assert.IsTrue(tplStat.TargetFields.Contains("price"));
            Assert.IsTrue(tplStat.TargetFields.Contains("tax"));
            Assert.IsTrue(tplStat.ByFields.Contains("store"), "by clause field name did not match expected");
            Assert.IsTrue(tplStat.ByFields.Contains("day"), "by clause field name did not match expected");

            //Sum not field fail check
            try
            {
                tplStat = new TplStats("sum by day");
                Assert.IsTrue(false, "Constructor allowed sum without field(s)");
            }
            catch { Assert.IsTrue(true); }

        }
        /*
        [TestMethod]
        public void TestMethod1()
        {
        }*/
    }
}
