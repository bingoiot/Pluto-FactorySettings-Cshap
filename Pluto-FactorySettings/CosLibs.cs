using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pluto_FactorySettings
{
    class CosLibs
    {
        private static byte Seq = 0;
        public static byte GetSeq()
        {
            byte n = Seq++;
            return n;
        }
        public static byte GetXor(byte[] data, int startIndex, int len)
        {
            byte sum = 0;
            for (int i = 0; i < len; i++)
                sum = (byte)(sum ^ data[startIndex + i]);
            return sum;
        }
        public static void Memcpy(byte[] s, int sIndex, byte[] d, int dIndex, int len)
        {
            byte[] buf = new byte[len];
            Array.Copy(s, sIndex, buf, 0, len);
            Array.Copy(buf, 0, d, dIndex, len);
        }
        public static UInt16 BtoU16(byte[] data, int startIndex)
        {
            UInt16 temp;
            temp = data[startIndex + 1];
            temp = (UInt16)(temp << 8);
            temp = (UInt16)(temp | data[startIndex]);
            return temp;
        }
        public static void U16toB(byte[] data, int startIndex, UInt16 val)
        {
            data[startIndex] = (byte)(val & 0x00FF);
            data[startIndex + 1] = (byte)((val >> 8) & 0x00FF);
        }
        public static String BtoHexStr(byte[] data, int startID, int len)
        {
            string str = null;
            for (int i = startID; i < len; i++)
            {
                str += data[i].ToString("X2");
                str += ' ';
            }
            return str;
        }
        public static byte[] HexStrtoB(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
        public static String U16toHexStr(UInt16[] data, int startID, int len)
        {
            String str = "";
            for (int i = startID; i < len; i++)
            {
                str += data[i].ToString("X4") + " ";
            }
            return str;
        }
        public static byte[] GetSubBytes(byte[] pdata, int startID, int len) {
            byte[] b = new byte[len];
            Array.Copy(pdata, startID, b, 0, len);
            return b;
        }
    }
}
