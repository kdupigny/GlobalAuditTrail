namespace GATUtils.Fix
{
    public enum FixTag
    {
        // General Tags
        None                    = 0,
        Account                 = 1,
        AvgFillPrice            = 6,
        BeginSeqNo              = 7,
        BeginString             = 8,
        BodyLength              = 9,
        CheckSum                = 10,
        ClOrdId                 = 11,
        TotalSharesFill         = 14,   //Known also ascumQty
        Currency                = 15,
        EndSeqNo                = 16,
        ExecId                  = 17,
        ExecInst                = 18,
        ExecRefId               = 19,   // Only present when ExecTransType=Cancel(1) or Correct(2). Refers to the ExecID of the message being cancelled or corrected
        ExecTransType           = 20,
        HandlInst               = 21,
        SecurityIdSource        = 22,
        MktExecLastfill         = 30,
        LastPx                  = 31,
        FillQty                 = 32,   //Known also as LastShares
        MsgSeqNum               = 34,
        MessageType             = 35,
        NewSeqNum               = 36,
        OrderId                 = 37,
        OrderQty                = 38,
        OrdStatus               = 39,
        OrdType                 = 40,
        OrigClOrdId             = 41,
        PossDupflag             = 43,
        Price                   = 44,
        SecurityId              = 45,
        Rule80A                 = 47,
        SenderCompId            = 49,
        SenderSubId             = 50,
        SendingTime             = 52,
        Side                    = 54,
        Symbol                  = 55,
        TargetCompId            = 56,
        TargetSubId             = 57,
        Text                    = 58,
        TimeInForce             = 59,
        TransactTime            = 60,
        SettleDate              = 64,
        SymbolSfx               = 65,
        TradeDate               = 75,
        ExecBroker              = 76,
        PositionEffect          = 77,   //OpenClose
        RawDataLength           = 95,
        RawData                 = 96,
        EncryptMethod           = 98,
        StopPx                  = 99,
        ExDestination           = 100,
        CxlRejectReason         = 102,
        OrdRejReason            = 103,
        SecurityDesc            = 107,
        HeartBeat               = 108,
        ClientId                = 109,
        MaxFloor                = 111,
        TestReqId               = 112,
        LocateReqd              = 114,
        OnBehalfOfCompId        = 115,
        OnBehalfOfSubId         = 116,  //BarCap session ID
        NetPrincipal            = 118,
        SettICurrency           = 120,
        OrigSendingTime         = 122,
        GapFillFlag             = 123,
        DeliverToCompId         = 128,
        DeliverToSubId          = 129,  //MSPool session ID (returns OnBehalfOfSubId optionally sent by user) 
        MiscFeeAmt              = 137,
        ResetSeqNumFlag         = 141,
        SenderLocationId        = 142,
        TargetLocationId        = 143,
        NoRelatedSym            = 146,
        ExecType                = 150,
        LeavesQty               = 151,
        AccruedInterestAmt      = 159,
        SecurityType            = 167,
        OrderQty2               = 192,
        FutSettDate2            = 193,
        LastSpotRate            = 194,
        LastForwardPoints       = 195,
        SecondaryOrderId        = 198,
        MaturityMonthYear       = 200,
        PutOrCall               = 201,
        StrikePrice             = 202,
        MaturityDay             = 205,
        OptAttribute            = 206,
        SecurityExchange        = 207,
        MaxShow                 = 210,
        CouponRate              = 223,
        ContractMultiplier      = 231,
        MdReqId                 = 262,
        SubscriptionRequestType = 263,
        MarketDepth             = 264,
        MdUpdateType            = 265,
        NoMdEntryTypes          = 267,
        NoMdEntries             = 268,
        MdEntryType             = 269,
        MdEntryPx               = 270,
        OpenCloseSettleFlag     = 286,
        UnderlyingSymbol        = 311,
        SecurityReqId           = 320,
        SecurityResponseId      = 322,
        LastSeqNumProcessed     = 369,
        ComplianceId            = 376, 
        GrossTradeAmt           = 381,
        MsgDirection            = 385,
        TotalNumSecurities      = 393,
        ClearingAcc             = 440,
        MultiLegReportType      = 442,  //indicates single(1), spread(3), or leg(2) of trade 
        Product                 = 460,
        CfiCode                 = 461,  //replacing 201
        ProductComplex          = 462,
        TestMessageIndicator    = 464,  //Ver 4.3 +
        MaturityDate            = 541,
        Username                = 553,
        Password                = 554,
        NoLegs                  = 555,
        SecurityListRequestType = 559,
        SecurityRequestResult   = 560,
        AccountType             = 581,
        LegSymbol               = 600,
        LegCfiCode              = 608,
        LegMaturityMonthYear    = 610,
        LegStrikePrice          = 612,
        LegSecurityExchange     = 616,
        LegRatioQuantity        = 623,
        LegSide                 = 624,
        LegRefId                = 654,
        PartySubIdType          = 803,
        LastLiquidityInd        = 851,
        EventType               = 865,
        EventDate               = 866,
	

        #region Custom Tags

        // CME
        TickSizeCme             = 969,
        EventTime               = 1145,
        InstrumentMultiplier    = 1147,
        ProductCode             = 1151,
        ApplicationFeedId       = 1180,
        ApplicationSeqNum       = 1181,
        ApplicationBeginSeqNum  = 1182,
        ApplicationEndSeqNum    = 1183,
        ApplicationRequestId    = 1346,
        ApplicationRequestType  = 1347,
        ApplicationLastSeqNum   = 1350,	
        NumberApplicationIds    = 1351,
        ReferenceApplicationId  = 1355,
        DisplayFactor           = 9787,

        // Pats
        TickSize                = 5023,
        TicksPerPoint           = 5071,

        // MSPool tags
        MsPoolLiquidityFlag     = 7433,

		//Credit Suisse
        DisplayInstruction      = 9140, // Order display instruction
        CsLiquidityIndicator    = 9200, // the tag name is LiquidityIndicator, but there it already a tag with the same name

        // Order Tags
        DbkLinkId               = 9483,
        RoutingInstruction      = 9487,
        DestinationMarketId     = 9570, //ExecAwayMktID on Nyse
        BillingIndicator        = 9578,
        LiquidityIndicator      = 9730,
        LiquidityFlag           = 9882, //TradeLiquidityIndicator on DirectAdge

        // Direct Edge tags
        SessionId               = 9600,

        //Bats tags
        OrigCompId              = 9688,
        OrigSubId               = 9689,

        #endregion
    };	
}
