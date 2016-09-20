using System;
using System.Collections.Generic;   
using StarMeter.Models;

namespace StarMeter.Controllers
{
    public interface IErrorDetector
    {
        ErrorTypes GetErrorType(Packet previousPacket, Packet currentPacket);
        bool IsDataError(Packet previousPacket, Packet currentPacket);
        bool IsSequenceError(Packet previousPacket, Packet currentPacket);
        bool IsTimeoutError(Packet previousPacket, Packet currentPacket);
        bool IsDisconnectError();
    }

    public class ErrorDetector : IErrorDetector
    {
        public ErrorTypes GetErrorType(Packet previousPacket, Packet currentPacket)
        {
            if (IsTimeoutError(previousPacket, currentPacket))
            {
                return ErrorTypes.Timeout;
            }
            if(IsDataError(previousPacket, currentPacket))
            {
                return ErrorTypes.DataError;
            }
            if (IsSequenceError(previousPacket, currentPacket))
            {
                return ErrorTypes.SequenceError;
            }
            return ErrorTypes.None;
        }

        public bool IsDataError(Packet previousPacket, Packet currentPacket)
        {
            var isCrcCorrect = CRC.CheckCrcForPacket(currentPacket.FullPacket);
            var isBabblingIdiot = CheckForBabblingIdiot(currentPacket, previousPacket);

            return !isCrcCorrect || isBabblingIdiot;
        }

        public bool IsSequenceError(Packet previousPacket, Packet currentPacket)
        {
            return currentPacket.SequenceNum < previousPacket.SequenceNum;
        }

        public bool IsTimeoutError(Packet previousPacket, Packet currentPacket)
        {
            return currentPacket.FullPacket.Length < previousPacket.FullPacket.Length;
        }

        public bool IsDisconnectError()
        {
            return false;
        }

        private static bool CheckForBabblingIdiot(Packet currentPacket, Packet previousPacket)
        {
            return previousPacket.FullPacket == currentPacket.FullPacket;
        }
    }
}
