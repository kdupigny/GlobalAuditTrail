using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace GAT.Utils.Data
{
    class AuditTrailDataTable : DataTable
    {
        /// <summary>
        /// Gets the name of the trail source.
        /// </summary>
        public string TrailSource { get; private set; }

       

        public AuditTrailDataTable(string sourceName)
            : base(sourceName)
        {
            TrailSource = sourceName;
            init();
        }

        #region Public Members



        #endregion

        #region Private Data

        private void init()
        {
            buildColumns();
        }

        private void buildColumns()
        {
            foreach (string col in DataColumns.AuditTrail)
            {
                this.Rows.Add(col);
            }
        }

        #endregion
    }
}
