using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RICADO.Omron
{
    public static class BCDConverter
    {
        #region Public Methods

        public static byte ToByte(byte bcdByte)
        {
            return convertToBinaryBytes(new byte[] { bcdByte })[0];
        }
        
        public static short ToInt16(short bcdWord)
        {
            return ToInt16(BitConverter.GetBytes(bcdWord));
        }

        public static short ToInt16(byte[] bcdBytes)
        {
            if(bcdBytes.Length != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(bcdBytes), "The BCD Bytes Array Length must be '2' for conversion to Int16");
            }
            
            return BitConverter.ToInt16(convertToBinaryBytes(bcdBytes));
        }

        public static ushort ToUInt16(short bcdWord)
        {
            return ToUInt16(BitConverter.GetBytes(bcdWord));
        }

        public static ushort ToUInt16(byte[] bcdBytes)
        {
            if (bcdBytes.Length != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(bcdBytes), "The BCD Bytes Array Length must be '2' for conversion to UInt16");
            }

            return BitConverter.ToUInt16(convertToBinaryBytes(bcdBytes));
        }

        public static int ToInt32(short bcdWord1, short bcdWord2)
        {
            List<byte> integerBytes = new List<byte>(4);

            integerBytes.AddRange(BitConverter.GetBytes(bcdWord1));
            integerBytes.AddRange(BitConverter.GetBytes(bcdWord2));

            return ToInt32(integerBytes.ToArray());
        }

        public static int ToInt32(byte[] bcdBytes)
        {
            if (bcdBytes.Length != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(bcdBytes), "The BCD Bytes Array Length must be '4' for conversion to Int32");
            }

            return BitConverter.ToInt32(convertToBinaryBytes(bcdBytes));
        }

        public static uint ToUInt32(short bcdWord1, short bcdWord2)
        {
            List<byte> integerBytes = new List<byte>(4);

            integerBytes.AddRange(BitConverter.GetBytes(bcdWord1));
            integerBytes.AddRange(BitConverter.GetBytes(bcdWord2));

            return ToUInt32(integerBytes.ToArray());
        }

        public static uint ToUInt32(byte[] bcdBytes)
        {
            if (bcdBytes.Length != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(bcdBytes), "The BCD Bytes Array Length must be '4' for conversion to UInt32");
            }

            return BitConverter.ToUInt32(convertToBinaryBytes(bcdBytes));
        }

        public static byte GetBCDByte(byte binaryValue)
        {
            return convertToBCDBytes(binaryValue, 1)[0];
        }

        public static short GetBCDWord(short binaryValue)
        {
            return BitConverter.ToInt16(convertToBCDBytes(binaryValue, 2));
        }

        public static short GetBCDWord(ushort binaryValue)
        {
            return BitConverter.ToInt16(convertToBCDBytes(binaryValue, 2));
        }

        public static short[] GetBCDWords(int binaryValue)
        {
            ReadOnlyMemory<byte> bcdBytes = convertToBCDBytes(binaryValue, 4);

            return new short[] { BitConverter.ToInt16(bcdBytes.Slice(0, 2).ToArray()), BitConverter.ToInt16(bcdBytes.Slice(2, 2).ToArray()) };
        }

        public static short[] GetBCDWords(uint binaryValue)
        {
            ReadOnlyMemory<byte> bcdBytes = convertToBCDBytes(binaryValue, 4);

            return new short[] { BitConverter.ToInt16(bcdBytes.Slice(0, 2).ToArray()), BitConverter.ToInt16(bcdBytes.Slice(2, 2).ToArray()) };
        }

        public static byte[] GetBCDBytes(short binaryValue)
        {
            return convertToBCDBytes(binaryValue, 2);
        }

        public static byte[] GetBCDBytes(ushort binaryValue)
        {
            return convertToBCDBytes(binaryValue, 2);
        }

        public static byte[] GetBCDBytes(int binaryValue)
        {
            return convertToBCDBytes(binaryValue, 4);
        }

        public static byte[] GetBCDBytes(uint binaryValue)
        {
            return convertToBCDBytes(binaryValue, 4);
        }

        #endregion


        #region Private Methods

        private static byte[] convertToBinaryBytes(byte[] bcdBytes)
        {
            if (bcdBytes.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bcdBytes), "The BCD Bytes Length cannot be Zero");
            }

            if (bcdBytes.Length > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(bcdBytes), "The BCD Bytes Length cannot be greater than 4");
            }

            long binaryValue = 0;

            foreach(byte bcdByte in bcdBytes.Reverse())
            {
                binaryValue *= 100;
                binaryValue += (long)(10 * (bcdByte >> 4));
                binaryValue += (long)(bcdByte & 0xF);
            }

            ReadOnlyMemory<byte> binaryBytes = BitConverter.GetBytes(binaryValue);

            return binaryBytes.Slice(0, bcdBytes.Length).ToArray();
        }
        
        private static byte[] convertToBCDBytes(long binaryValue, int byteLength)
        {
            byte[] bcdBytes = new byte[byteLength];

            for(int i = 0; i < bcdBytes.Length; i++)
            {
                long lowDigit = binaryValue % 10;
                long highDigit = (binaryValue % 100) - lowDigit;

                if(highDigit != 0)
                {
                    highDigit /= 10;
                }

                lowDigit = lowDigit < 0 ? -lowDigit : lowDigit;
                highDigit = highDigit < 0 ? -highDigit : highDigit;

                bcdBytes[i] = (byte)((highDigit << 4) | lowDigit);

                if(binaryValue == 0)
                {
                    break;
                }
                else
                {
                    binaryValue /= 100;
                }
            }

            return bcdBytes;
        }

        #endregion
    }
}
