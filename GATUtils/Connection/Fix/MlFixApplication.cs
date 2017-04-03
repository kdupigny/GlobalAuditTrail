using System;
using System.Collections.Generic;
using GATUtils.Logger;
using QuickFix;
using System.Threading;
using GATUtils.Connection.DB;
using GATUtils.Fix;
using GATUtils.Connection.DB.Models;
using GATUtils.Utilities;

namespace GATUtils.Connection.Fix
{
    public class MlFixApplication : GatFixApplication
    {
          
        public MlFixApplication()
        {
            _InitFieldMap();
        }

        // MESSAGECRACKER BASE CLASS FUNCTIONS
        #region MESSAGECRACKER INHERITED FUNCTIONS

        //need new order single
        //Order cancel request
        //order cancel replace reject
        //session level reject 35=3

        // Upon receipt of an executed trade message
        public override void onMessage(QuickFix42.ExecutionReport message, SessionID sessionId)
        {
            //            MessageBox.Show("ExecutionReport: " + message.ToString());
            string theMessage = message.ToString();
            //Console.WriteLine("Execution Received: "+ theMessage);

            if (!message.isSetField(35) && !message.getHeader().isSetField(35))
            {
                message.setField(35, "8");
            }

            DbHandle.Instance.InsertMessageCommand("MlDropCopy", new GatFixMessage(message, "MlDropCopy", _executionFieldMap));
        }

        // Upon receipt of a rejected cancel message
        public override void onMessage(QuickFix42.OrderCancelReject message, SessionID sessionId)
        {
            //            MessageBox.Show("CancelReject: " + message.ToString());
            GatLogger.Instance.AddMessage("Reject Message Received: " + message, LogMode.LogAndScreen);
        }

        // Upon receipt of an Email Positions response
        public override void onMessage(QuickFix42.Email message, SessionID sessionId)
        {
            //            MessageBox.Show("Email Positions: " + message.ToString());
            GatLogger.Instance.AddMessage("Email Message Received: " + message, LogMode.LogAndScreen);
        }

        public override void onMessage(QuickFix42.OrderCancelReplaceRequest message, SessionID sessionId)
        {

            GatLogger.Instance.AddMessage("Replace Message Received: " + message, LogMode.LogAndScreen);
        }

        #endregion


        private List<KeyValuePair<FixTag, string>> _executionFieldMap;
        private void _InitFieldMap()
        {
            _executionFieldMap = new List<KeyValuePair<FixTag, string>>
            {
                new KeyValuePair<FixTag, string> (FixTag.TransactTime, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.DATE)),
                new KeyValuePair<FixTag, string> (FixTag.MessageType, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.MESSAGETYPE)),
                new KeyValuePair<FixTag, string> (FixTag.SenderCompId, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.SENDERCOMP)),
                new KeyValuePair<FixTag, string> (FixTag.TargetCompId, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.TARGETCOMP)),
                new KeyValuePair<FixTag, string> (FixTag.TargetSubId, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.TRADERINT)),
                new KeyValuePair<FixTag, string> (FixTag.OrderId, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.ORDERID)),
                new KeyValuePair<FixTag, string> (FixTag.ClOrdId, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.CLORDID)),
                new KeyValuePair<FixTag, string> (FixTag.LegRefId, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.CLORDID)),
                new KeyValuePair<FixTag, string> (FixTag.ExecId, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.EXECID)),
                new KeyValuePair<FixTag, string> (FixTag.Symbol, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.SYMBOL)),
                new KeyValuePair<FixTag, string> (FixTag.Side, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.SIDE)),
                new KeyValuePair<FixTag, string> (FixTag.LastPx, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.FILLPX)),
                new KeyValuePair<FixTag, string> (FixTag.Price, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.PRICE)),
                new KeyValuePair<FixTag, string> (FixTag.AvgFillPrice, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.AVGPX)),
                new KeyValuePair<FixTag, string> (FixTag.SecurityType, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.PRODUCTTYPE)),
                new KeyValuePair<FixTag, string> (FixTag.TransactTime, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.TIMESTAMP)),
                new KeyValuePair<FixTag, string> (FixTag.FillQty, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.QTY)),
                new KeyValuePair<FixTag, string> (FixTag.MktExecLastfill, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.ROUTE)),
                new KeyValuePair<FixTag, string> (FixTag.OrderQty, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.ORDERQTY)),
                new KeyValuePair<FixTag, string> (FixTag.MktExecLastfill, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.BROKER)),
                new KeyValuePair<FixTag, string> (FixTag.Account, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.ACC)),
                new KeyValuePair<FixTag, string> (FixTag.ClearingAcc, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.CLEARACC)),
                new KeyValuePair<FixTag, string> (FixTag.MaturityDate, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.MATURITYDATE)),
                new KeyValuePair<FixTag, string> (FixTag.Text, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.DESCRIPTION)),
                new KeyValuePair<FixTag, string> (FixTag.ExecType, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.EXECTYPE)),
                new KeyValuePair<FixTag, string> (FixTag.StrikePrice, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.STRIKEPX)),
                new KeyValuePair<FixTag, string> (FixTag.PutOrCall, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.PUTCALL)),
                new KeyValuePair<FixTag, string> (FixTag.MaturityMonthYear, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.MATURITYDATE)),
               
                new KeyValuePair<FixTag, string> (FixTag.None, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel.GatExecution),(int)DBFieldModel.GatExecution.SOURCE))
                ////new KeyValuePair<FixTag, string> (, EnumUtil.GetEnumStringFromInteger(typeof(DBFieldModel),(int)DBFieldModel.GatExecution)),
            };
        }
    }
}
