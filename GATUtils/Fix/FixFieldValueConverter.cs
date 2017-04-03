using System;
using System.Collections.Generic;
using GATUtils.Utilities;

namespace GATUtils.Fix
{
    public class FixFieldValueConverter
    {
        public FixFieldValueConverter()
        {
            _Init();
        }

        public string this[string tag, string fixValue]
        {
            get
            {
                string readableValue = fixValue;
                Dictionary<string, string> temp;

                FixTag fixTag = (FixTag)Enum.Parse(typeof(FixTag), tag);
                if (_fixFieldValueTranslator.TryGetValue(fixTag, out temp))
                {
                    temp.TryGetValue(fixValue, out readableValue);
                }
                
                return readableValue;
            }
        }

        public string this[FixTag tag, string fixValue, bool isDate]
        {
            get
            {
                string readableValue = fixValue;
                Dictionary<string, string> temp;

                if (_fixFieldValueTranslator.TryGetValue(tag, out temp))
                {
                    temp.TryGetValue(fixValue, out readableValue);
                }

                if (tag == FixTag.SendingTime || tag == FixTag.MaturityDate ||
                    tag == FixTag.SettleDate || tag == FixTag.TransactTime)
                    readableValue = isDate ? _ExtractMySqlDbDate(readableValue) : _FormatTimeStamp(readableValue.ToUpper());

                return readableValue;
            }
        }
 
        public static FixFieldValueConverter Instance
        {
            get { return s_instance ?? (s_instance = new FixFieldValueConverter()); }
        }

        public static string TryGetFixValue(FixTag tag, QuickFix.Message fixMsg)
        {
            string value = string.Empty;
            if (fixMsg.isSetField((int)tag))
                value = fixMsg.getField((int) tag);

            return value;
        }

        public static string FormatMaturityDate(string yearMonth, string day)
        {
            string output = string.Format("{0}-{1}-{2}", yearMonth.Substring(0,4), yearMonth.Substring(4), (string.IsNullOrEmpty(day) ? "01": day));
            return output;
        }

        public string GetProductSubType(string product)
        {
            switch (product.ToUpper())
            {
                case "CORP":
                case "CB":      //Convertible Bond
                case "CPP":     //Corporate Private Placement Bond
                case "DUAL":    //Dual Currency Bond
                case "EUCORP":  //Euro Corporate Bond
                case "XLINKD":  //Index Linded Bond
                case "STRUCT":  //Structured Notes Bond
                case "YANK":    //Yankee Corporate Bond
                case "CORPORATE":

                case "BRADY":   //Brady Bond
                case "EUSOV":   //Euro Sovereigns Bond
                case "TBOND":   //US Treasury Bond
                case "TINT":    //Interest strip from any bond or note
                case "TIPS":    //Treasury Inflation Protected Securities
                case "TCAL":    //Principal strip from a non-callable bond or note
                case "UST":     //US Treasury Note (Deprecated: uses TNOTE)
                case "USTB":    //US Treasury Bill (Deprecated: uses TBILL)
                case "TNOTE":   //US Treasury Note
                case "TBILL":   //US Treasury Bill
                case "GOVERNMENT":

                case "AN":     //Other Anticipation Notes BAN, GAN, etc.
                case "COFO":     //Certificate of Obligation
                case "COFP":     //Certificate of Participation
                case "GO":     //General Obligation Bonds
                case "MT":     //Mandatory Tender
                case "RAN":     //Revenue Anticipation Note
                case "REV":     //Revenue Bonds
                case "SPCLA":     //Special Assessment
                case "SPCLO":     //Special Obligation
                case "SPCLT":     //Special Tax
                case "TAN":     //Tax Anticipation Note
                case "TAXA":     //Tax Allocation
                case "TECP":     //Tax Exempt Commercial Paper
                case "TRAN":     //Tax & Revenue Anticipation Note
                case "VRDN":     //Variable Rate Demand Note
                case "WAR":     //Warrant
                case "MUNICIPAL":
                case "FAC":
                    return "BOND";

                case "CS":      //Common Stock
                case "PS":      //Preferred Stock
                case "EQUITY":
                    return "STOCK";
                
                case "FUT":
                    return "FUTURE";

                case "OPT": 
                    return "OPTION";

                default:
                    return product;
            }
        }

        private void _Init()
        {
            _BuildFieldValueTranslator();
        }

        private void _BuildFieldValueTranslator()
        {
            _fixFieldValueTranslator = new Dictionary<FixTag, Dictionary<string, string>>();

            //field 20 ExecTransType
            Dictionary<string, string> temp 
                = new Dictionary<string, string> {{"0", "New"}, {"1", "Cancel"}, {"2", "Correct"}, {"3", "Status"}};
            _fixFieldValueTranslator.Add(FixTag.ExecTransType, temp);

            //field 21 HandlInst
            temp = new Dictionary<string, string>
                       {
                           {"1", "Private Automated Order"},
                           {"2", "Public Automated order"},
                           {"3", "Manual Order"}
                       };
            _fixFieldValueTranslator.Add(FixTag.HandlInst, temp);

            //field 35 MessageType
            temp = new Dictionary<string, string>
                       {
                           {"0", "HeartBeat"},{"1", "TestRequest"},
                           {"2", "ResendRequest"},{"3", "Reject"},
                           {"4", "SequenceReset"},{"5", "Logout"},
                           {"8", "ExecutionReport"},{"A", "Logon"},
                           {"D", "Order"},{"F", "Cancel"},
                           {"G", "Replace"}
                       };
            _fixFieldValueTranslator.Add(FixTag.MessageType, temp);

            //field 40 OrdType
            temp = new Dictionary<string, string>
                       {
                           {"1", "Market"},{"2", "Limit"},{"3", "Stop"},
                           {"4", "StopLimit"},{"5", "MOC"},{"6", "WithOrWithout"},
                           {"7", "LimitOrBetter"},{"8", "LimitWithOrWithout"},{"9", "OnBasis"},
                           {"A", "OnClose"},{"B", "LimitOnClose"},{"C", "Forex-Market"},
                           {"D", "PreviouslyQuoted"},{"E", "PreviouslyIndicated"},{"F", "Forex-Limit"},
                           {"G", "Forex-Swap"},{"H", "Forex-PreviousQuote"},{"I", "Funari"},
                           {"J", "MIT"},{"K", "MarketLimitLeftOver"},{"L", "PreviousFundValidationPoint"},
                           {"P", "Pegged"}
                       };
            _fixFieldValueTranslator.Add(FixTag.OrdType, temp);

            //field 54 Side
            temp = new Dictionary<string, string>
                       {
                           {"1", "Buy"},{"2", "Sell"},
                           {"3", "BuyMinus"},{"4", "Sell Plus"},
                           {"5", "SellShort"},{"6", "ShortExempt"}
                       };
            _fixFieldValueTranslator.Add(FixTag.Side, temp);

            //field 77 PositionEffect
            temp = new Dictionary<string, string> {{"O", "Open"}, {"C", "Close"}, {"R", "Rolled"}, {"F", "FIFO"}};
            _fixFieldValueTranslator.Add(FixTag.PositionEffect, temp);

            //field 150 ExecType
            temp = new Dictionary<string, string>
                       {
                           {"0", "New"},{"1", "PartialFill"},{"2", "Fill"},
                           {"3", "DoneForDay"},{"4", "Canceled"},{"5", "Replace"},
                           {"6", "PendingCancel"},{"7", "Stopped"},{"8", "Rejected"},
                           {"9", "Suspended"},{"A", "Pending New"},{"B", "Calculated"},
                           {"C", "Expired"},{"D", "Restarted"},{"E", "PendingReplace"},
                           {"F", "Trade"},{"G", "TradeCorrect"},{"H", "TradeCancel"},
                           {"I", "OrderStatus"}
                       };
            _fixFieldValueTranslator.Add(FixTag.ExecType, temp);

            //field 201 putCall
            temp = new Dictionary<string, string> {{"0", "Put"}, {"1", "Call"}};
            _fixFieldValueTranslator.Add(FixTag.PutOrCall, temp);

            //field 803 PartySubIdType
            temp = new Dictionary<string, string>
                       {
                           {"1", "Firm"},{"2", "Person"},
                           {"3", "System"},{"4", "Application"},
                           {"5", "FullLegalName"},{"6", "PostalAddress"},
                           {"7", "PhoneNumber"},{"8", "Email"},
                           {"9", "ContactName"}
                       };
            _fixFieldValueTranslator.Add(FixTag.PartySubIdType, temp);

            //field 460 Product
            temp = new Dictionary<string, string>
                       {
                           {"1", "AGENCY"},{"2", "COMMODITY"},{"3", "CORPORATE"},
                           {"4", "CURRENCY"},{"5", "EQUITY"},{"6", "GOVERNMENT"},
                           {"7", "INDEX"},{"8", "LOAN"},{"9", "MONEYMARKET"},
                           {"10", "MORTGAGE"},{"11", "MUNICIPAL"},{"12", "OTHER"},
                           {"13", "FINANCING"}
                       };
            _fixFieldValueTranslator.Add(FixTag.Product, temp);

        }

        private static string _ExtractMySqlDbDate(string fixDateString)
        {
            string[] datePieces = fixDateString.Split(new[] { ' ', '-' });

            return string.Format("{0}-{1}-{2}", datePieces[0].Substring(0, 4), datePieces[0].Substring(4, 2), datePieces[0].Substring(6));
        }

        private static string _FormatTimeStamp(string fixDateString)
        {
            string[] datePieces = fixDateString.Split(new[] { ' ', '-' });

            TimeSpan easternTime = MyTime.GetEasternTime(datePieces[1], MyTime.TimeFormat.HHMMSSMMM_COLON);
            string outputTimeString = string.Format("{0}-{1}-{2} {3}", datePieces[0].Substring(0, 4),
                                                    datePieces[0].Substring(4, 2), datePieces[0].Substring(6),
                                                    string.Format("{0}:{1}:{2}.{3}", easternTime.Hours.ToString("00"),
                                                                  easternTime.Minutes.ToString("00"),
                                                                  easternTime.Seconds.ToString("00"),
                                                                  easternTime.Milliseconds.ToString("000")));

            return outputTimeString;
        }

        private static FixFieldValueConverter s_instance;
        Dictionary<FixTag, Dictionary<string, string>> _fixFieldValueTranslator;
    }
}
