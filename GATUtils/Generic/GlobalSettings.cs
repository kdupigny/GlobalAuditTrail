using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GATUtils
{
    public static class GlobalSettings
    {
        public static DBSettings DB 
        { 
            get 
            {
                if (db == null)
                    db = new DBSettings();

                return db; 
            } 
        }
        private static DBSettings db;
    }

    public class DBSettings
    {
        public string host { get { return "localhost"; } }
        public string port { get { return "3306"; } }
        public string db { get { return "firmtrail"; } }
        public string user { get { return "kevin"; } }
        public string pass { get { return "testing123"; } }
                            //*/{ get { return "us3rpa55"; } }
                           
        public string fillTable { get { return "fills"; } }
        public string orderTable { get { return "ords"; } }
    }
}
