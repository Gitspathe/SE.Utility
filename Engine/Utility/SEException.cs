using System;

namespace SE.Utility
{
    public abstract class SEExceptionBase
    {
        public abstract string BasicInfo { get; }
        public abstract string VerboseInfo { get; }
    }

    public class SEException : SEExceptionBase
    {
        private Exception exception;
        private string basicInfo;
        private string verboseInfo;

        public Exception Exception => exception ?? new Exception(verboseInfo ?? basicInfo);
        public override string BasicInfo => basicInfo;
        public override string VerboseInfo => string.IsNullOrEmpty(verboseInfo) ? basicInfo : verboseInfo;

        public SEException(Exception exception)
        {
            this.exception = exception;
            basicInfo = exception.StackTrace;
            verboseInfo = GetExceptionInfo(exception);
        }

        public SEException(string message, string verboseMessage = null)
        {
            basicInfo = message;
            verboseInfo = verboseMessage;
        }

        private static string GetExceptionInfo(Exception e)
        {
            string msg = e.Message;
            int recursion = 0;
            while (e.InnerException != null && recursion <= 5) {
                msg += "\n  -->" + e.InnerException.Message;
                e = e.InnerException;
                recursion++;
            }
            return msg;
        }
    }
}
