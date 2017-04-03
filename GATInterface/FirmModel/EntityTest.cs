using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using DevExpress.Data.Filtering.Helpers;
using DevExpress.LookAndFeel;
using GATInterface.XML;
using GATUtils.Logger;

namespace GATInterface.FirmModel
{
    public class EntityTest
    {
        public static EntityTest Instance { get { return _instance ?? (_instance = new EntityTest()); } }

        public EntityTest()
        {
        }

        public void GetDataTest()
        {
            

        }

        public void RefreshContext()
        {
            if (_executions == null) _executions = new List<FirmExecution>();

            int startingCount = _executions.Count;
            int endCount = TodaysFills.Count;
            if (startingCount != endCount)
                GatLogger.Instance.AddMessage(string.Format("{0} New executions found", endCount - startingCount));
        }

        public List<FirmExecution> TodaysFills  
        { 
            get
            {
                if (_firmTrailContext == null) _firmTrailContext = new firmtrailEntities1();
                    
                if (_executions == null) _executions = new List<FirmExecution>();

                if (!_lastQueriedDate.Equals(ApplicationState.Instance.DisplayDate))
                    _executions.Clear();

                var result = from record in _firmTrailContext.FirmExecutions
                    where record.DATE.Equals(ApplicationState.Instance.DisplayDate)
                    select record;

                List<FirmExecution> temp = result.ToList();

                if (_executions.Count != temp.Count)  //new fills found
                {
                    int startIdx = _executions.Count;
                    int count = temp.Count - startIdx;
                    _executions.AddRange(result.ToList().GetRange(startIdx, count));
                }

                _lastQueriedDate = ApplicationState.Instance.DisplayDate;

                return _executions;
            }
        }

        public List<FirmExecution> DirectExecutions
        {
            get
            {
                if (_executions == null)
                    RefreshContext();

                return _executions;
            }
        }

        public BindingSource BindingHandle
        {
            get
            {
                if (_bindHandle == null)
                {
                    _bindHandle = new BindingSource {DataSource = DirectExecutions};
                }
                return _bindHandle;
            }
        }

        private BindingSource _bindHandle;
        private List<FirmExecution> _executions; 
        private IQueryable<FirmExecution> _GetFills(DateTime date, string symbol = "*", string orderid = "*")
        {
            return from record in _firmTrailContext.FirmExecutions
                                 where record.DATE.Equals(ApplicationState.Instance.DisplayDate)
                                 select record;
        }


        private DateTime _lastQueriedDate;

        private static firmtrailEntities1 _firmTrailContext;
        private static EntityTest _instance;
        

    }
}
