using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace GAT.Utils.Data
{
    /// <summary>
    /// 
    /// </summary>
    public enum MatchPriority
    {
        NONE,       //Complete Mismatch
        LEVEL1,     //Only 1 parameter matches (any of the given fields except for order id)
        LEVEL2,     //More than one parameter matches could be all match except for order id
        LEVEL3,     //Order Id matches only
        LEVEL4,     //Order Id and at least one other parameter.
        LEVEL5      //Full Match
    }

    public class OrderMatcher
    {
        /// <summary>
        /// Gets the number of rows in the Audit Trail.
        /// </summary>
        public int TrailSize { get; private set; }
        /// <summary>
        /// Gets current row position in the trail.
        /// </summary>
        public int TrailPosition { get; private set; }

        public OrderMatcher(List<string> columnMatchSet)
            : this(null, null, columnMatchSet) { }

        public OrderMatcher(DataTable sourceTable, DataTable targetTable, List<string> columnMatchSet)
        {
            this.sourceTable = sourceTable;
            this.targetTagle = targetTable;
            columnsToMatch = columnMatchSet;
        }

        public MatchPriority MatchOrder(DataRow trailRecord, DataTable targetTable)
        {
            MatchPriority highestMatch = MatchPriority.NONE;
            MatchPriority matchLevel = highestMatch;
            foreach (DataRow targetRecord in targetTable.Rows)
            {
                string sourceValue = trailRecord["ORDERID"].ToString().Trim();
                string targetValue = targetRecord["ORDERID"].ToString().Trim();

                if (sourceValue.Equals(targetValue))
                {
                    matchLevel = MatchPriority.LEVEL3;
                }

                int matchCount = 0;
                foreach (string column in columnsToMatch)
                {
                    sourceValue = trailRecord[column].ToString().Trim();
                    targetValue = targetRecord[column].ToString().Trim();
                    
                    if (sourceValue.Equals(targetValue))
                        matchCount++;
                }
                if (matchCount == columnsToMatch.Count)
                    matchLevel = (matchLevel == MatchPriority.LEVEL3 ? MatchPriority.LEVEL5 : MatchPriority.LEVEL2);
                else if (matchCount == 1)
                    matchLevel = (matchLevel == MatchPriority.LEVEL3 ? MatchPriority.LEVEL4 : MatchPriority.LEVEL1);
                else if (matchCount > 1)
                    matchLevel = (matchLevel == MatchPriority.LEVEL3 ? MatchPriority.LEVEL4 : MatchPriority.LEVEL2);
                
                if ((int)matchLevel > (int)highestMatch)
                    highestMatch = matchLevel;

                if (highestMatch == MatchPriority.LEVEL5)
                    break;
            }

            return highestMatch;
        }

        #region Private Data

        private DataTable sourceTable;
        private DataTable targetTagle;
        private List<string> columnsToMatch;

        #endregion 
    }
}
