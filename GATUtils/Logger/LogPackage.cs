namespace GATUtils.Logger
{
    internal class LogPackage
    {
        public string Message;
        public LogMode WriteMode;

        public LogPackage(string msg, LogMode writeMode)
        {
            Message = msg;
            WriteMode = writeMode;
        }
    }
}
