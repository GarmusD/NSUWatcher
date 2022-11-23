using System;

namespace serialTransport
{
    internal class ExceptionWithExitCode : Exception
    {
        public int ErrorCode { get; private set; }
        public ExceptionWithExitCode(int errorCode, string message = null) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
