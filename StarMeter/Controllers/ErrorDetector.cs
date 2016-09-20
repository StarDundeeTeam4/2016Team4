using System;
using System.Collections.Generic;   
using StarMeter.Models;

namespace StarMeter.Controllers
{
    public interface IErrorDetector
    {
        ErrorTypes GetErrorType();
        bool IsDataError();
        bool IsSequenceError();
        bool IsTimeoutError();
        bool IsDisconnectError();
    }

    public class ErrorDetector : IErrorDetector
    {
        public ErrorTypes GetErrorType()
        {
            return ErrorTypes.None;
        }

        public bool IsDataError()
        {
            return false;
        }

        public bool IsSequenceError()
        {
            return false;
        }

        public bool IsTimeoutError()
        {
            return false;
        }

        public bool IsDisconnectError()
        {
            return false;
        }
    }
}
