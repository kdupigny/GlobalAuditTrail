using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GATUtils.Connection.DB;
using GATUtils.Utilities;
using GATUtils.Connection.DB.Models;
using System.Data;

namespace UnitTests
{
    [TestClass]
    public class MySqlDBConnectionTest
    {

        [TestMethod]
        public void ConnectTest()
        {
            DbConnection dbConnection = new MySqlDbConnection(MockData.LHost, MockData.LPort, MockData.LDb, MockData.LUser, MockData.LPass);

            Assert.IsFalse(dbConnection.IsAlive);
            Assert.AreEqual(System.Data.ConnectionState.Closed, dbConnection.MyCurrentState);

            dbConnection.Connect();

            Assert.IsTrue(dbConnection.IsAlive);
            Assert.AreEqual(System.Data.ConnectionState.Open, dbConnection.MyCurrentState);
        }

        [TestMethod]
        public void DisconnectTest()
        {
            DbConnection dbConnection = new MySqlDbConnection(MockData.LHost, MockData.LPort, MockData.LDb, MockData.LUser, MockData.LPass);
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
            DbConnection dbConnection = new MySqlDbConnection(MockData.LHost, MockData.LPort, MockData.LDb, MockData.LUser, MockData.LPass);
            DataTable tableResult = dbConnection.RunQuery(MockData.QueryGetAllRecords);

            Assert.AreNotEqual(0, tableResult.Rows.Count);
        }

        [TestMethod]
        public void InsertQueryTest() 
        {
            int count = 0;
            List<string> dbFields = MockData.dbFields;

            DbConnection dbConnection = new MySqlDbConnection(MockData.LHost, MockData.LPort, MockData.LDb, MockData.LUser, MockData.LPass);

            Assert.IsFalse(dbConnection.IsAlive);

            int result = 0;
            while (result == 0 && count <= 10)
            {
                string[] testExecutionValues = MockData.testExecutionValues;

                string query = string.Format("INSERT IGNORE Into firmtrail.fills (`{0}`) VALUES ('{1}');", string.Join("`,`", dbFields), string.Format(string.Join("','", testExecutionValues), count));
            
                result = dbConnection.RunNonQuery(query);
                count++;
            }

            Assert.AreNotEqual(0, result);
        }

    
    }
}
