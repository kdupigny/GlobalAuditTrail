namespace GATUtils.Types.Report
{
    public interface IReport
    {
        string ReportName { get; }
        int ViolationCount { get; }

        string ReportFilename { get; }
        string ReportBasePath { get; }
        string ReportOutputPath { get; }
        
        bool RunReport();
        bool BuildReport();
        bool DeliverReport();
    }
} 
