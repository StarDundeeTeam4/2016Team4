using System.Linq;
using StarMeter.Models;

namespace StarMeter.Controllers
{
    public interface IErrorDetector
    {
        ErrorType GetErrorType(Packet previousPacket, Packet currentPacket);
        bool IsDataError(Packet previousPacket, Packet currentPacket);
        bool IsSequenceError(Packet previousPacket, Packet currentPacket);
        bool IsTimeoutError(Packet previousPacket, Packet currentPacket);
    }

    public class ErrorDetector : IErrorDetector
    {
        /// <summary>
        /// A method that gets the error type for an error packet 
        /// by comparing it with the previous packet
        /// </summary>
        /// <param name="previousPacket">The previous packet from the current packet</param>
        /// <param name="currentPacket">The current packet being checked for error</param>
        /// <returns>An error type relating to the error that occured</returns>
        public ErrorType GetErrorType(Packet previousPacket, Packet currentPacket)
        {
            if (IsTimeoutError(previousPacket, currentPacket))
            {
                return ErrorType.Timeout;
            }
            if(IsDataError(previousPacket, currentPacket))
            {
                return ErrorType.DataError;
            }
            return IsSequenceError(previousPacket, currentPacket) 
                ? ErrorType.SequenceError 
                : ErrorType.Disconnect;
        }

        /// <summary>
        /// Checks if the data is corrupt, causing a data error.
        /// The can take the form of a babbling idiot or a CRC error
        /// </summary>
        /// <param name="previousPacket">The previous packet from the current packet</param>
        /// <param name="currentPacket">The current packet being checked for error</param>
        /// <returns>A bool indicating whether this error occurred, true if it did and false if it didn't</returns>
        public bool IsDataError(Packet previousPacket, Packet currentPacket)
        {
            var isCrcCorrect = IsCrcError(currentPacket);
            var isBabblingIdiot = CheckForBabblingIdiot(currentPacket, previousPacket);

            return isBabblingIdiot || !isCrcCorrect;
        }

        /// <summary>
        /// Checks if the sequence numbers are out of order, causing a sequence error.
        /// </summary>
        /// <param name="previousPacket">The previous packet from the current packet</param>
        /// <param name="currentPacket">The current packet being checked for error</param>
        /// <returns>A bool indicating whether this error occurred, true if it did and false if it didn't</returns>
        public bool IsSequenceError(Packet previousPacket, Packet currentPacket)
        {
            return currentPacket.SequenceNum < previousPacket.SequenceNum;
        }

        /// <summary>
        /// Checks if the transmission timedout and stopped sending packets, causing a timeout error.
        /// </summary>
        /// <param name="previousPacket">The previous packet from the current packet</param>
        /// <param name="currentPacket">The current packet being checked for error</param>
        /// <returns>A bool indicating whether this error occurred, true if it did and false if it didn't</returns>
        public bool IsTimeoutError(Packet previousPacket, Packet currentPacket)
        {
            return currentPacket.FullPacket.Length < previousPacket.FullPacket.Length;
        }

        /// <summary>
        /// Checks if the transmission is repeatedly sending the same packet.
        /// </summary>
        /// <param name="previousPacket">The previous packet from the current packet</param>
        /// <param name="currentPacket">The current packet being checked for error</param>
        /// <returns>A bool indicating whether this error occurred, true if it did and false if it didn't</returns>
        private static bool CheckForBabblingIdiot(Packet previousPacket, Packet currentPacket)
        {
            return previousPacket.FullPacket.SequenceEqual(currentPacket.FullPacket);
        }

        /// <summary>
        /// Checks if the CRC value of the packet is the correct value
        /// </summary>
        /// <param name="currentPacket">The current packet being checked for error</param>
        /// <returns>A bool indicating whether the value is correct, true if it if and false if it isn't</returns>
        private static bool IsCrcError(Packet currentPacket)
        {
            bool crcValid;
            if (currentPacket.GetType() == typeof(RmapPacket))
            {
                crcValid = RmapPacketHandler.CheckRmapCrc((RmapPacket)currentPacket);
            }
            else
            {
                crcValid = true;
            }
            return crcValid;
        }
    }
}
