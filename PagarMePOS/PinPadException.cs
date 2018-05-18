using System;

namespace PagarMePOS
{
    public class PinPadException : Exception
    {
        public int ErrorCode { get; set; }

        public PinPadException(int errorCode, string message) : base(message)
        {
            this.ErrorCode = errorCode;
        }
    }
}
