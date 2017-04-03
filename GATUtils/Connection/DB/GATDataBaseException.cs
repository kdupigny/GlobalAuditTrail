using System;
using GATUtils.Generic;

namespace GATUtils.Connection.DB
{
    public class GatDataBaseException : GatException
    {
        public GatDataBaseException(string msg, Exception e)
            : base(string.Format("{0}\nInner Exception: {1}\nStackTrace: {2}", msg, e.Message, e.StackTrace),e)
        { }

        public GatDataBaseException(string msg)
            : base(msg)
        {
        }
    }
}
