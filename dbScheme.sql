/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Creating Database: `firmtrail`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `firmtrail` /*!40100 DEFAULT CHARACTER SET latin1 */;

USE `firmtrail`;

--
-- Table structure for table `fills`
--

DROP TABLE IF EXISTS `fills`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `fills` (
  `RECORDINSERTSTAMP` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `DATE` date NOT NULL,
  `SENDERCOMP` varchar(20) COMMENT 'Sender session that sent the fill',
  `TARGETCOMP` varchar(20)  COMMENT 'Target session that recieved the fill',
  `ORDERID` varchar(50) NOT NULL,
  `CLORDID` varchar(50) NOT NULL,
  `EXECID` varchar(50) NOT NULL,
  `MESSAGETYPE` varchar(5) NOT NULL COMMENT 'Execution Message type',
  `EXECTYPE` varchar(20) NOT NULL COMMENT 'Execution transaction type',
  `SYMBOL` varchar(20) NOT NULL,
  `SIDE` varchar(4) NOT NULL,
  `ORDERQTY` int(11) DEFAULT NULL,
  `QTY` int(11) NOT NULL,
  `PRICE` double NOT NULL DEFAULT '0',
  `FILLPX` double NOT NULL,
  `AVGPX` double NOT NULL DEFAULT '0',
  `TIMESTAMP` TIMESTAMP NOT NULL COMMENT 'Transaction time stamp',
  `TIF` varchar(5) DEFAULT NULL COMMENT 'Time In Force',
  `CLIENTID` varchar(30) DEFAULT NULL COMMENT 'Trader Identifier at exchange recieving order',
  `TRADERINT` varchar(30) DEFAULT NULL COMMENT 'Additional trader identification',
  `ACC` varchar(30) DEFAULT NULL COMMENT 'Target account',
  `CLEARACC` varchar(30) DEFAULT NULL COMMENT 'Clearing firm account',
  `LIQUIDITYIND` varchar(5) DEFAULT NULL COMMENT 'Indicator of liquidity {1 2 6 A R X}',
  `PRODUCTTYPE` varchar(15) DEFAULT NULL,
  `SECURITYID` varchar(20) DEFAULT NULL,
  `SECURITYIDSOURCE` varchar(10) DEFAULT NULL,
  `ACCRUEDINTAMT` varchar(15) DEFAULT NULL COMMENT 'Accrued Interest Amount',
  `ROUTE` varchar(60) DEFAULT NULL COMMENT 'Intermediate route to contra',
  `EXCHANGE` varchar(10) DEFAULT NULL COMMENT 'Exchange receiving the order',
  `BROKER` varchar(20) NOT NULL DEFAULT 'UNKN' COMMENT 'Contra Broker (Last Market)',
  `EXPIRATIONDATE` date DEFAULT NULL,
  `MATURITYDATE` date DEFAULT NULL,
  `SETTLEDATE` date DEFAULT NULL,
  `PUTCALL` varchar(4) DEFAULT NULL,
  `STRIKEPX` float NOT NULL DEFAULT '0',
  `COUPONRATE` varchar(10) NOT NULL DEFAULT '0',
  `GROSSTRADEAMT` varchar(15) NOT NULL DEFAULT '0',
  `NETPRINCIPAL` int(11) NOT NULL DEFAULT '0',
  `COMMISSION` double NOT NULL DEFAULT '0',
  `EXCHANGEFEE` double NOT NULL DEFAULT '0' COMMENT 'Access Fee',
  `REGFEE` double NOT NULL DEFAULT '0',
  `OCCFEE` double NOT NULL DEFAULT '0',
  `OTHERFEE` double NOT NULL DEFAULT '0',
  `DESCRIPTION` text NOT NULL,
  `SOURCE` varchar(60) NOT NULL COMMENT 'Partner fix session or file where fills were obtained',
  PRIMARY KEY (`DATE`,`ORDERID`,`CLORDID`,`EXECID`),
  KEY `idxFillDate` (`DATE`),
  KEY `idxFillSource` (`DATE`,`SOURCE`),
  KEY `idx_sym` (`SYMBOL`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COMMENT='Order Executions';
/*!40101 SET character_set_client = @saved_cs_client */;


--
-- Table structure for table `ords`
--

DROP TABLE IF EXISTS `ords`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ords` (
  `RECORDINSERTSTAMP` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `DATE` date NOT NULL,
  `CLIENTID` varchar(30) NOT NULL,
  `ORDERID` varchar(50) NOT NULL,
  `CLORDID` varchar(50) NOT NULL,
  `ORIGCLORDID` varchar(50) NOT NULL,
  `MESSAGETYPE` varchar(10) NOT NULL,
  `SYMBOL` varchar(15) NOT NULL,
  `SIDE` varchar(10) NOT NULL,
  `ORDQTY` int(11) NOT NULL,
  `TIF` varchar(30) NOT NULL,
  `ACC` varchar(60) NOT NULL DEFAULT ' ',
  `TRADERINT` varchar(30) NOT NULL,
  `ORDTYPE` varchar(30) NOT NULL DEFAULT '',
  `LIMITPX` double NOT NULL DEFAULT '0',
  `STOPPX` double NOT NULL DEFAULT '0',
  `ORDERVALUE` double DEFAULT NULL,
  `MAXFLOOR` varchar(20) NOT NULL,
  `TEXT` text NOT NULL,
  `ACCTYPE` varchar(30) NOT NULL,
  `STRATEGY` varchar(30) NOT NULL,
  `STARTTIME` varchar(30) NOT NULL,
  `ENDTIME` varchar(30) NOT NULL,
  `IDSOURCE` varchar(20) NOT NULL,
  `SECURITYID` varchar(60) NOT NULL,
  `SECURITYEXCH` varchar(30) NOT NULL,
  `PEGTYPE` varchar(30) NOT NULL,
  `SENDINGTIME` varchar(30) NOT NULL,
  `TRIGGERTIME` varchar(30) NOT NULL,
  `RECEIVETIME` varchar(30) NOT NULL,
  `SENDERCOMP` varchar(20) NOT NULL,
  `TARGETCOMP` varchar(20) NOT NULL,
  `ONBEHALF` varchar(20) NOT NULL,
  `RULE80A` varchar(30) NOT NULL,
  `SOURCE` varchar(50) NOT NULL,
  PRIMARY KEY (`DATE`,`CLORDID`,`ORIGCLORDID`,`MESSAGETYPE`),
  KEY `idxOrderDate` (`DATE`),
  KEY `idxOrderSource` (`DATE`,`SOURCE`),
  KEY `idx_sym` (`SYMBOL`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 COMMENT='Order Details';
/*!40101 SET character_set_client = @saved_cs_client */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

