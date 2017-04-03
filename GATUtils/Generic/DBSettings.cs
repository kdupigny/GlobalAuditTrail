using System.Xml;
using GATUtils.XML;

namespace GATUtils.Generic
{
    public class DBSettings
    {
        public string DisplayName { get { return _displayName; } }
        public bool IsActive { get { return _isActive; } }
        public string Host { get { return _host; } }
        public string Port { get { return _port; } }
        public string Db { get { return _db; } }
        public string User { get { return _user; } }
        public string Password { get { return _pass; } }

        public string FillTable { get { return _fillTable; } }
        public string OrderTable { get { return _orderTable; } }

        public DBSettings() { }
        public DBSettings(string displayName, bool isActive, string host, string port, string dbName, string username, string passoword, string fillTable, string orderTable)
        {
            _displayName = displayName;
            _isActive = isActive;
            _host = host;
            _port = port;
            _db = dbName;
            _user = username;
            _pass = passoword;

            _fillTable = fillTable;
            _orderTable = orderTable;
        }

        public DBSettings(XmlNode dbConfig)
        {
            foreach (XmlNode innerNode in dbConfig)
            {
                switch(innerNode.Name)
                {
                    case "DbAppName":  
                        _displayName = innerNode.InnerText.Trim(); 
                        break;
			        case "DbIsActive":
                        _isActive = XmlTools.ResolveBooleanValue(innerNode.InnerText.Trim());
                        break;
			        case "DbHost":  
                        _host  = innerNode.InnerText.Trim();
                        break;
			        case "DbPort":
                        _port  = innerNode.InnerText.Trim();  
                        break;
			        case "DbUser":  
                        _user = innerNode.InnerText.Trim();
                        break;
			        case "DbPass":  
                        _pass = innerNode.InnerText.Trim();
                        break;
			        case "DbName":  
                        _db = innerNode.InnerText.Trim();
                        break;
			        case "DbFillTable":  
                        _fillTable = innerNode.InnerText.Trim();
                        break;
			        case "DbOrderTable":  
                        _orderTable = innerNode.InnerText.Trim();
                        break;
                }
            }
        }

        private readonly string _displayName = "TestingDB";
        private readonly bool _isActive = true;
        private readonly string _host = "localhost";
        private readonly string _port = "3306";
        private readonly string _db = "firmtrail";
        private readonly string _user = "kevin";
        private readonly string _pass = "testing123";

        private readonly string _fillTable = "fills";
        private readonly string _orderTable = "ords";
    }
}
