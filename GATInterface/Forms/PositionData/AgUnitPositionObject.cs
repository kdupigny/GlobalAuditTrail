using System.Collections.Generic;
using System.Data;

namespace GATInterface.Forms.PositionData
{
    public class AgUnitPositionObject
    {
        public string Account { get; private set; }
        public Dictionary<string, PositionObject> Positions
        {
            get { return _position ?? (_position = new Dictionary<string, PositionObject>()); }
        }

        public AgUnitPositionObject(string account)
        {
            Account = account;

            //to-do load all overnight positions here
            _overnights = OvernightPositionManager.Instance.GetAccountPositions(account);
        }

        public void AddExecution(DataRow execution)
        {
            string symbol = execution["SYMBOL"].ToString();

            PositionObject posObject;
            if (!Positions.TryGetValue(symbol, out posObject))
            {
                OvernightPositionObject overnightPos;
                if (_overnights != null)
                    posObject = _overnights.TryGetValue(symbol, out overnightPos) ? 
                        new PositionObject(Account, symbol, overnightPos.Position, overnightPos.ClosePx) : 
                        new PositionObject(Account, symbol);
                else 
                    posObject = new PositionObject(Account, symbol);

                Positions.Add(symbol, posObject);
            }
            
            posObject.AddExecution(execution);
        }

        public List<DataRow> PopulateAgUnitRows(DataTable positionTable)
        {
            List<DataRow> positionRows = new List<DataRow>();

            positionTable.BeginLoadData();
            foreach (var positionObject in Positions)
            {
                DataRow positionRow = positionTable.NewRow();
                positionRow = positionObject.Value.FillPositionRow(positionRow);
                positionRows.Add(positionRow);
                positionTable.LoadDataRow(positionRow.ItemArray, LoadOption.OverwriteChanges);
            }

            return positionRows;
        }
        
        private Dictionary<string, PositionObject> _position;
        private readonly Dictionary<string, OvernightPositionObject> _overnights;
    }
}
