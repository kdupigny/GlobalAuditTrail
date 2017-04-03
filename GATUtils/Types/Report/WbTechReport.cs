using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GATUtils.Logger;
using GATUtils.Utilities;
using GATUtils.Utilities.FileUtils;

namespace GATUtils.Types.Report
{
    public class WbTechReport : IReport
    {
        public virtual string ReportName { get { return "Whitebay Tech Report"; } }
        public virtual int ViolationCount { get { throw new NotImplementedException("Violation Count undefined in report - " + ReportName); } }
        public virtual string ReportBasePath { get { return GatFile.Path(Dir.Data, "Reports", string.Empty); } }
        public virtual string ReportOutputPath { get { throw new NotImplementedException("Report output path not configured in report - " + ReportName); } }
        public virtual string ReportFilename { get { return string.Format("WbTechReport_{0}.csv", MyTime.TodaysDateStrYYYYMMDD); } }
       
        public virtual List<WbTechReportLevel> AcceptedRunLevels { get { throw new NotImplementedException("Accepted run levels not defined for report - " + ReportName); } }

        public WbTechReport(WbTechReportLevel runLevel, DateTime reportDate, object[] inputObjects)
        {
            _RunLevel = runLevel;
            _ReportDate = reportDate;
            _Init(_RunLevel, inputObjects);
        }

        public virtual bool RunReport() { throw new NotImplementedException("Report runner not defined in report - " + ReportName); }
        public virtual bool BuildReport() 
        { 
            _BuildReportOutputFile();
            return true;
        }
        public virtual bool DeliverReport() { throw new NotImplementedException("Report delivery not defined in report - " + ReportName); }

        protected WbTechReportLevel _RunLevel { get; set; }

        protected bool _ReportRan;

        protected StringBuilder _ReportText;
        protected DateTime _ReportDate;

        protected string _ReportFilePath { get { return GatFile.Path(ReportBasePath, string.Empty, ReportFilename); } }

        protected string _ReportHeader { get { return string.Format("Report {0}", ReportName); } }
        protected string _ReportTrailer { get { return string.Format("Trail, {0}", ViolationCount); } }

        protected virtual void _PrepareData(object[] inputData) { throw new NotImplementedException("Report data preparation undefined in method to prepare report data items - " + ReportName); }
        
        protected void _BuildReportOutputFile()
        {
            if (!_ReportRan)
            {
                RunReport();
                BuildReport();
                return;
            }

            using (var writer = new StreamWriter(_ReportFilePath))
            {
                writer.WriteLine(_ReportHeader);
                writer.WriteLine();

                writer.WriteLine(_ReportText.ToString());

                writer.WriteLine();
                writer.WriteLine(_ReportTrailer);
            }
        }

        private void _Init(WbTechReportLevel runLevel, object[] inputObjects)
        {
            if (!AcceptedRunLevels.Contains(runLevel))
            {
                List<string> acceptedLevelString = EnumUtil.GetStringListFromEnumList(AcceptedRunLevels);
                string exceptionText = string.Format("Report {0} does not support the intended runLevel {1}.  Accepted Levels [{2}].",
                        ReportName,
                        EnumUtil.GetEnumStringFromInteger(typeof(WbTechReportLevel), (int)runLevel),
                        string.Join(", ", acceptedLevelString.ToArray()));
                throw new ArgumentException(exceptionText);
            }

            GatLogger.Instance.AddMessage(string.Format("Starting {0}.", ReportName), LogMode.LogAndScreen);

            _ReportText = new StringBuilder();
            _PrepareData(inputObjects);
        }

    }
}
