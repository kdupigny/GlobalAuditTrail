using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using GATUtils.Utilities;

namespace ReportsFactory.Input
{
    public class GenericReportLoader
    {
        public string FilePath { get; private set; }
        public string Filename { get { return Path.GetFileName(FilePath); } }

        public GenericReportLoader(string filePath, bool hasHeader)
        {
            FilePath = filePath;
            this.hasHeader = hasHeader;
        }

        protected virtual void ParseFile()
        {
            if (!File.Exists(FilePath))
                return;

            using (TextReader tr = new StreamReader(FilePath))
            {
                string line = tr.ReadLine();
                if (hasHeader)
                    line = tr.ReadLine();

                FileData = DataTableUtils.GetEmptyDataTableFromHeaderFieldList(getHeaderList());
                while (line != null)
                {
                    string[] fields = line.Split(',');
                    DataRow dr = FileData.NewRow();
                    DataTableUtils.LoadDataRow(ref dr, FileColumnToDbFieldMap, fields);
                    FileData.Rows.Add(dr);
                    line = tr.ReadLine();
                }
            }
        }

        protected List<string> getHeaderList()
        {
            List<string> headers = new List<string>();

            foreach (KeyValuePair<int, string> columnIdxToDbField in FileColumnToDbFieldMap)
            {
                if(!headers.Contains(columnIdxToDbField.Value))
                {
                    headers.Add(columnIdxToDbField.Value);
                }
            }
            return headers;
        } 

        private bool hasHeader;
        protected virtual void initFieldMap()
        {
            throw new NotImplementedException();
        }
        protected List<KeyValuePair<int, string>> FileColumnToDbFieldMap;
        protected DataTable FileData;

        protected void init()
        {
            initFieldMap();
            ParseFile();
        } 
    }
}
