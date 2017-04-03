using System;
using System.Collections.Generic;
using System.Data;

namespace GATInterface.Forms.PositionData
{
    public class PositionObject
    {
        public string Account { get; private set; }
        public string Symbol { get; private set; }

        public decimal TotalExposure
        {
            get
            {
                if (!_positionsValid) UpdatePosition();
                return _totalExposure;
            }
        }
        public decimal NetExposure
        {
            get
            {
                if (!_positionsValid) UpdatePosition();
                return _netExposure;
            }
        }
        public decimal ShortExposure
        {
            get
            {
                if (!_positionsValid) UpdatePosition();
                return _totalShortExposure;
            }
        }
        public decimal LongExposure
        {
            get
            {
                if (!_positionsValid) UpdatePosition();
                return _totalLongExposure;
            }
        }

        public long TotalVolume
        {
            get
            {
                if (!_positionsValid) UpdatePosition();
                return _totalVolume;
            }
        }
        public long NetVolume
        {
            get
            {
                if (!_positionsValid) UpdatePosition();
                return _netVolume;
            }
        }
        public long ShortVolume
        {
            get
            {
                if (!_positionsValid) UpdatePosition();
                return _shortVolume;
            }
        }
        public long LongVolume
        {
            get
            {
                if (!_positionsValid) UpdatePosition();
                return _longVolume;
            }
        }

        public PositionObject(string account, string symbol)
        {
            Account = account;
            Symbol = symbol;

            _executions = new Dictionary<string, PositionAtom>();
            _positionsValid = true;
        }

        public PositionObject(string account, string symbol, int startOfDayPosition, decimal previousDayAvgPrice)
            :this(account, symbol)
        {
            _dayStartPosition = startOfDayPosition;
            _dayStartAvgPrice = previousDayAvgPrice;
        }

        public void AddExecution(DataRow execution)
        {
            string execId = execution["EXECID"].ToString();

            PositionAtom temp;
            if (!_executions.TryGetValue(execId, out temp))
            {
                temp = new PositionAtom(execution);
                _executions.Add(execId, temp);
                _positionsValid = false;
            }
        }

        public void UpdatePosition()
        {
            _ResetValues();
            foreach (PositionAtom atom in _executions.Values)
            {
                if (atom.Side.Equals("BUY"))
                {
                    _longVolume += atom.Quantity;
                    //_totalLongExposure += atom.NotionalValue;
                    _avgBuyPrice += atom.PricePerShare;
                    _buyCount++;
                }
                else
                {
                    _shortVolume += atom.Quantity;
                    //_totalShortExposure += atom.NotionalValue;
                    _avgSellPrice += atom.PricePerShare;
                    _sellCount++;
                }
            }

            _totalVolume = _longVolume + _shortVolume;
            _netVolume = (-1 * _shortVolume) + _longVolume;

            _avgBuyPrice /= (_buyCount !=0 ? _buyCount : 1);
            _avgSellPrice /= (_sellCount != 0 ? _sellCount : 1);

            if(_longVolume > _shortVolume)
            {
                _matchedShares = _shortVolume;
                _exposedShares = _longVolume - _matchedShares;
            }
            else if (_longVolume < _shortVolume)
            {
                _matchedShares = _longVolume;
                _exposedShares = _shortVolume - _matchedShares;
            }
            else
            {
                _matchedShares = _longVolume;
            }

            if (_netVolume > 0)
            {
                _totalLongExposure = _avgBuyPrice * _exposedShares;
            }
            else if (_netVolume < 0)
            {
                _totalShortExposure = _avgSellPrice*_exposedShares;
            }

            _pnl = (_matchedShares*_avgSellPrice) - (_matchedShares*_avgBuyPrice);

            _positionsValid = true;
        }

        public DataRow FillPositionRow(DataRow positionRow)
        {
            positionRow["ACCOUNT"] = Account;
            positionRow["SYMBOL"] = Symbol;
            positionRow["POSITION"] = NetVolume;
            positionRow["BUYVOLUME"] = LongVolume;
            positionRow["AVGBUYPX"] = _avgBuyPrice;
            positionRow["SELLVOLUME"] = ShortVolume;
            positionRow["AVGSELLPX"] = _avgSellPrice;
            positionRow["TOTALVOLUME"] = TotalVolume;
            positionRow["LONGEXPOSURE"] = LongExposure;
            positionRow["SHORTEXPOSURE"] = ShortExposure;
            positionRow["NETEXPOSURE"] = NetExposure;
            positionRow["PNL"] = _pnl;
            //positionRow[] =;
            return positionRow;
        }

        private void _ResetValues()
        {
            _netVolume = _totalVolume = _longVolume = _shortVolume = 0;
            _netExposure = _totalExposure = _totalLongExposure = _totalShortExposure = 0;
            _avgBuyPrice = _avgSellPrice = _buyCount = _sellCount = _pnl = 0;

            if(_dayStartPosition > 0)
            {
                _longVolume = _dayStartPosition;
                _totalLongExposure = _dayStartPosition*_dayStartAvgPrice;
            }
            else
            {
                _shortVolume = _dayStartPosition*-1;
                _totalShortExposure = _shortVolume*_dayStartAvgPrice;
            }
        }

        private long _netVolume;
        private long _totalVolume;
        private long _longVolume;
        private long _shortVolume;

        private decimal _netExposure;
        private decimal _totalExposure;
        private decimal _totalLongExposure;
        private decimal _totalShortExposure;

        private decimal _avgBuyPrice;
        private decimal _avgSellPrice;
        private decimal _buyCount;
        private decimal _sellCount;

        private decimal _pnl;

        private readonly long _dayStartPosition;
        private readonly decimal _dayStartAvgPrice;

        private long _matchedShares;
        private long _exposedShares;

        //dictionary organizes fills by execution id to avoid duplicates
        private readonly Dictionary<string, PositionAtom> _executions;
        private bool _positionsValid;

        private class PositionAtom
        {
            public DateTime EntryTime { get; private set; }
            public string Side { get; private set; }
            public int Quantity { get; private set; }
            public decimal PricePerShare { get; private set; }

            public decimal NotionalValue { get { return Quantity * PricePerShare; } }
            public decimal SideBasedNotional { get { return (Side.Equals("BUY") ? NotionalValue : -1 * NotionalValue); } }

            public PositionAtom(DataRow execution)
            {
                EntryTime = DateTime.Parse(execution["TIMESTAMP"].ToString());
                Side = execution["SIDE"].ToString().ToUpper();
                Quantity = int.Parse(execution["QTY"].ToString());
                PricePerShare = decimal.Parse(execution["FILLPX"].ToString());
            }
        }
    }

}
