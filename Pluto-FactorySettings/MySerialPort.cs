using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;

namespace Pluto_FactorySettings
{
    
    class MySerialPort
    {
        public static       System.IO.Ports.SerialPort Port;
        private static      onRecieveCommandListener myRecieveCommandListener;
        private static      onRecieveMessageListener myRecieveMessageListener;

        private const int   RX_BUF_SIZE = 1024;
        static byte[]       rx_buf = new byte[RX_BUF_SIZE];
        static int          rx_counter = 0;
        public delegate void onRecieveCommandListener(byte cmd0, byte cmd1, byte[] data);
        public delegate void onRecieveMessageListener(byte[] tx, byte[] rx);

        public MySerialPort(onRecieveCommandListener listener,System.IO.Ports.SerialPort sp)
        {
            Port = sp;
            myRecieveCommandListener = listener;
        }
        public static void SetRecieveMessageListener(onRecieveMessageListener listener){
            myRecieveMessageListener = listener;
        }
        public static void InputMessage(byte[] data, int len) {
            int remain, temp;
            if (myRecieveMessageListener != null)
            {
                myRecieveMessageListener(null,data);
            }
            temp = rx_counter + len;
            if (temp > RX_BUF_SIZE)
            {
                remain = temp - RX_BUF_SIZE;
                rx_counter -= remain;
                CosLibs.Memcpy(rx_buf, remain, rx_buf, 0, rx_counter);
            }
            Array.Copy(data, 0, rx_buf, rx_counter, len);
            rx_counter += len;
            byte[] pkg = ReadPackage();
            if (pkg != null)
            {
                int dlen = BtoU16(pkg, 1);//get data length
                byte[] pdata = CosLibs.GetSubBytes(pkg, 5, dlen);
                if (myRecieveCommandListener != null)
                    myRecieveCommandListener(pkg[3],pkg[4],pdata);
            }
        }
        public static void SaveParameter(String portName, String baudrate,String dataBit,String parity, String stopbit)
        {
            Port.Parity = (Parity)Enum.Parse(typeof(Parity), parity);
            string StopB = stopbit.Substring(0, 1);//将停止位中的汉字截掉，以便和枚举变量匹配
            Port.StopBits = (StopBits)Enum.Parse(typeof(StopBits), StopB);
            Port.PortName = portName; //确定选中的串口
            Port.BaudRate = int.Parse(baudrate);
            string DataB = dataBit.Substring(0, 1);//将数据位中的汉字截掉，以便和整型变量匹配
            Port.DataBits = int.Parse(DataB);     
        }    
        public static int SendCommand(byte cmd0, byte cmd1, byte[] data)
        {
            int dlen = 0;
            if (data != null){
                dlen = data.Length;  
            }
            int slen = dlen + 6;
            byte[] buf = new byte[slen];
            buf[0] = 0xFE;
            buf[1] = (byte)(dlen>>8);
            buf[2] = (byte)(dlen&0x00FF);
            buf[3] = cmd0;
            buf[4] = cmd1;
            if (data != null)
            {
                Array.Copy(data, 0, buf, 5, dlen);
            }
            buf[slen - 1] = CosLibs.GetXor(buf, 1, (slen - 1));
            if (Port.IsOpen)
            {
                Port.Write(buf, 0, buf.Length);
                if (myRecieveMessageListener != null)
                {
                    myRecieveMessageListener(buf, null);
                }
            }
            return 0;
        }

        public static byte[] ReadPackage()
        {
            int remain;
            int index = 0;
            int slen;
            while (index < rx_counter)
            {
                remain = (rx_counter - index);
                index = find_sof(rx_buf, index, remain);
                if (index >= 0)
                {
                    if (check_package(rx_buf, index, remain) == 0)
                    {
                        slen = BtoU16(rx_buf, (index + 1)) + 6; 
                        byte[] buf = new byte[slen];
                        Array.Copy(rx_buf, index, buf, 0, slen);
                        rx_counter -= slen;
                        int clen = rx_counter - index;
                        if ((clen >= RX_BUF_SIZE) || (clen < 0))
                            rx_counter = 0;
                        else
                            CosLibs.Memcpy(rx_buf, (index + slen), rx_buf, index, clen);
                        return buf;
                    }
                    else
                    {
                        index++;//move to next byte
                    }
                }
                else
                {
                    break;
                }
            }
            return null;
        }
        private static int find_sof(byte[] pdata, int startIndex, int len)
        {
            int i;
            for (i = 0; i < len; i++)
            {
                if (pdata[i + startIndex] == 0xFE)
                    return (i + startIndex);
            }
            return -1;
        }
        private static int check_package(byte[] pdata, int startID, int len)
        {
            int slen;
            byte fcs, sum;
            if (len < 6)
                return 1;
            slen = BtoU16(pdata, (startID + 1)) + 6;
            if (slen > len)//package length error!
            {
                return 1;
            }
            fcs = pdata[startID + (slen - 1)];
            sum = CosLibs.GetXor(pdata, startID + 1, (slen - 2));
            if (fcs == sum)
                return 0;
            else
                return 1;
        }
        private static int BtoU16(byte[] data, int startID)
        {
            int temp = 0;
            temp = data[startID];
            temp = temp << 8;
            temp |= data[startID + 1];
            return temp;
        }
    }
}
