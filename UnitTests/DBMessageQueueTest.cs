using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using GATUtils.Connection.DB;
using System.Data;
using System.Threading;
using MySql.Data.Types;


namespace UnitTests
{
    [TestClass]
    public class DBMessageQueueTest
    {
        private DbConnection dbConnection;

        public DBMessageQueueTest()
        {
            dbConnection = new MySqlDbConnection(MockData.LHost, MockData.LPort, MockData.LDb, MockData.LUser, MockData.LPass);
        }

        [TestMethod]
        public void InitializeQueue()
        {
            Assert.IsNotNull(DbHandle.Instance);

            DataTable dt = getDataTable();

            foreach (DataRow dr in dt.Rows)
            {
                dr["DATE"] = "2014-06-07";
                DbHandle.Instance.InsertMessageCommand("UnitTest", dr);
            }

            Assert.AreNotEqual(0, DbHandle.Instance.MessageQueueSize);
            while (DbHandle.Instance.MessageQueueSize > 0)
            {
                Thread.Sleep(3000);
            }
            Assert.AreEqual(0, DbHandle.Instance.MessageQueueSize);
        }

        private DataTable getDataTable()
        {
            DataTable dt = new DataTable("testTable");
            dt = dbConnection.RunQuery(MockData.QueryGetAllRecords);

            return dt;
        }

    }
}
