using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GATUtils.Connection.DB.Models;
using GATUtils.Utilities;

namespace UnitTests
{
    [TestClass]
    public class UtilsDataTests
    {
        [TestMethod]
        public void TestEnumToStringList()
        {
            List<string> output = null;

            output = EnumUtil.GetStringListFromEnum(typeof(DBFieldModel.GatExecution));
            Assert.AreNotEqual(0, output.Count);
            Assert.IsTrue(output.Contains("CLORDID"));
        }

        [TestMethod]
        public void TestStringToEnumValue()
        {
            string fieldName = "CLORDID";
            int enumIntValue = EnumUtil.GetEnumFromString(typeof(DBFieldModel.GatExecution), fieldName);

            Assert.AreEqual((int)DBFieldModel.GatExecution.CLORDID, enumIntValue);
        }

        [TestMethod]
        public void TestIntegerToEnumString()
        {
            int fieldIndex = 4;
            string enumStringValue = EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution), fieldIndex);

            Assert.AreEqual("CLORDID", enumStringValue);
        }
    }
}
