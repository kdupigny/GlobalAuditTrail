using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using GATInterface.FirmModel;
using GATUtils.Connection.DB;

namespace GATInterface.Forms.PositionData
{
    public class PositionTable
    {
        public BindingSource Binding { get { return _baseTable.Binding; } }
        public DataTable Table { get { return _baseTable.Table; } }

        public Dictionary<string, AgUnitPositionObject> AgUnitPositions
        {
            get { return _agUnitPositons ?? (_agUnitPositons = new Dictionary<string, AgUnitPositionObject>()); }
        }

        public PositionTable()
        {
            _Init();
        }

        public void Update(DateTime posDate)
        {
            SetPositionDate(posDate);
            Update();
        }

        public void Update()
        {
            _rawData = DbHandle.Instance.GetFills("PositionTable", _positionDate.ToString("yyyy-MM-dd"));
            _BuildPositionDataTable();
        }

        public void Update(DataTable currentData)
        {

            _rawData = currentData;
            _BuildPositionDataTable();
        }

        public void SetPositionDate(DateTime posDate)
        {
            _positionDate = posDate;
        }

        private void _BuildPositionDataTable()
        {
            foreach (DataRow dataRow in _rawData.Rows)
            {
                string acc = dataRow["ACC"].ToString();
                AgUnitPositionObject agUnit;

                if(!AgUnitPositions.TryGetValue(acc, out agUnit))
                {
                    agUnit = new AgUnitPositionObject(acc);
                    AgUnitPositions.Add(acc, agUnit);
                }

                agUnit.AddExecution(dataRow);
            }

            _UpdateTable();
        }

        private List<DataRow> _allPositoins;
        private void _UpdateTable()
        {
            _allPositoins = new List<DataRow>();

            Table.Clear();
            foreach (var agUnitPositionObject in AgUnitPositions)
            {
                _allPositoins.AddRange(agUnitPositionObject.Value.PopulateAgUnitRows(Table));
            }

            //_baseTable.CleanAndFillTable(_allPositoins);
        }

        private void _Init()
        {
        OvernightPositionManager.Instance.Initialize();
            _baseTable = new BindableDataTable(_BuildTableColumns());
        }

        private DataColumn[] _BuildTableColumns()
        {

            return _tableColumnNames.Select(column => new DataColumn(column.Key){DataType = column.Value }).ToArray();
        }

        private DateTime _positionDate;
        private BindableDataTable _baseTable;
        private readonly Dictionary<string, Type> _tableColumnNames = new Dictionary<string, Type> {
                                                                          {"ACCOUNT",       typeof(string) },  
                                                                          {"SYMBOL",        typeof(string)},
                                                                          {"POSITION",      typeof(int)},
                                                                          {"BUYVOLUME",     typeof(int)},
                                                                          {"SELLVOLUME",    typeof(int)},
                                                                          {"AVGBUYPX",      typeof(decimal)},
                                                                          {"AVGSELLPX",     typeof(decimal)},
                                                                          {"TOTALVOLUME",   typeof(int)},
                                                                          {"LONGEXPOSURE",  typeof(decimal)},
                                                                          {"SHORTEXPOSURE", typeof(decimal)},
                                                                          {"NETEXPOSURE",   typeof(decimal)},
                                                                          {"PNL",           typeof(decimal)},
                                                                      };

        private Dictionary<string, AgUnitPositionObject> _agUnitPositons;
        
        private DataTable _rawData;
    }
}
