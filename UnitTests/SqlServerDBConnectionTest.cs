using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using GATUtils.Connection.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class SqlServerDBConnectionTest
    {
        [TestMethod]
        public void ConnectTest()
        {
            DbConnection dbConnection = new SqlServerDbConnection(MockData.BullHost, MockData.BullPort, MockData.BullDb, MockData.BullUser, MockData.BullPass);

            Assert.IsFalse(dbConnection.IsAlive);
            Assert.AreEqual(System.Data.ConnectionState.Closed, dbConnection.MyCurrentState);

            dbConnection.Connect();

            Assert.IsTrue(dbConnection.IsAlive);
            Assert.AreEqual(System.Data.ConnectionState.Open, dbConnection.MyCurrentState);
        }

        [TestMethod]
        public void DisconnectTest()
        {
            DbConnection dbConnection = new SqlServerDbConnection(MockData.BullHost, MockData.BullPort, MockData.BullDb, MockData.BullUser, MockData.BullPass);
            dbConnection.Connect();

            Assert.IsTrue(dbConnection.IsAlive);
            Assert.AreEqual(System.Data.ConnectionState.Open, dbConnection.MyCurrentState);

            dbConnection.Disconnect();
            Assert.IsFalse(dbConnection.IsAlive);
            Assert.AreEqual(System.Data.ConnectionState.Closed, dbConnection.MyCurrentState);
        }

        [TestMethod]
        public void SelectQueryTest()
        {
            DbConnection dbConnection = new SqlServerDbConnection(MockData.BullHost, MockData.BullPort, MockData.BullDb, MockData.BullUser, MockData.BullPass);
            DataTable tableResult = dbConnection.RunQuery(MockData.QueryGetAtRecords);

            Assert.AreNotEqual(0, tableResult.Rows.Count);
        }

        [TestMethod]
        public void StoredProcedureTest()
        {
            
        }
    }
}
