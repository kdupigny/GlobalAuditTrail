using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GATUtils.Connection.DB.Models;
using GATUtils.Utilities;

namespace ReportsFactory.Input
{
    class MlDropCopyAuditTrailFileLoader : GenericReportLoader
    {
        public MlDropCopyAuditTrailFileLoader(string filePath, bool hasHeader) 
            : base(filePath, hasHeader)
        {
            init();
        }

        protected override void initFieldMap()
        {
            FileColumnToDbFieldMap = new List<KeyValuePair<int, string>>
                {
                    new KeyValuePair<int, string> (0, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.MESSAGETYPE)),
                    new KeyValuePair<int, string> (2, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.SENDERCOMP)),
                    new KeyValuePair<int, string> (3, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.TARGETCOMP)),
                    new KeyValuePair<int, string> (7, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.ORDERID)),
                    new KeyValuePair<int, string> (9, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.CLORDID)),
                    new KeyValuePair<int, string> (10, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.EXECID)),
                    new KeyValuePair<int, string> (11, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.SYMBOL)),
                    new KeyValuePair<int, string> (12, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.SIDE)),
                    new KeyValuePair<int, string> (13, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.FILLPX)),
                    new KeyValuePair<int, string> (14, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.PRICE)),
                    new KeyValuePair<int, string> (15, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.AVGPX)),
                    new KeyValuePair<int, string> (17, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.PRODUCTTYPE)),
                    new KeyValuePair<int, string> (18, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.TIMESTAMP)),
                    new KeyValuePair<int, string> (19, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.QTY)),
                    new KeyValuePair<int, string> (22, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.ROUTE)),
                    new KeyValuePair<int, string> (23, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.ORDERQTY)),
                    new KeyValuePair<int, string> (25, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.BROKER)),
                    new KeyValuePair<int, string> (26, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.ACC)),
                    new KeyValuePair<int, string> (29, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.MATURITYDATE)),
                    new KeyValuePair<int, string> (33, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.DESCRIPTION)),
                    new KeyValuePair<int, string> (35, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.EXECTYPE)),
                    new KeyValuePair<int, string> (-1, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.SOURCE))
                    //new KeyValuePair<int, string> (, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel),(int)DBFieldModel.GatExecution)),
                };
        }
    }
}
