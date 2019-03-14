using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Newtonsoft.Json;
using System.IO;

namespace Pluto_FactorySettings
{
    class FactorySettings
    {
        public const byte MT_RPC_CMD_POLL = 0x00;
        public const byte MT_RPC_CMD_SREQ = 0x20;
        public const byte MT_RPC_CMD_AREQ = 0x40;
        public const byte MT_RPC_CMD_SRSP = 0x60;

        public const byte MT_RPC_SYS_RES0 = 0x00;   /* Reserved. */
        public const byte MT_RPC_SYS_SYS = 0x01;
        public const byte MT_RPC_SYS_MAC = 0x02;
        public const byte MT_RPC_SYS_NWK = 0x03;
        public const byte MT_RPC_SYS_APS = 0x04;
        public const byte MT_RPC_SYS_PDO = 0x05;
        public const byte MT_RPC_SYS_SAP = 0x06;   /* Simple API. */
        public const byte MT_RPC_SYS_UTI = 0x07;
        public const byte MT_RPC_SYS_DBG = 0x08;
        public const byte MT_RPC_SYS_APP = 0x09;
        public const byte MT_RPC_SYS_OTA = 0x0A;
        public const byte MT_RPC_SYS_MAX = 0x0B;    /* Maximum value, must be last */

        public const byte MT_SYS_TEST =0x01;
        public const byte MT_SYS_RESET = 0x02;
        public const byte MT_SYS_READ_INFO = 0x03;
        public const byte MT_SYS_READ_SN = 0x04; 
        public const byte MT_SYS_WRITE_SN = 0x05;

        public static Form1 myFom1;
        MySerialPort mySerialPortObject;

        public FactorySettings(Form1 form, System.IO.Ports.SerialPort sp)
        {
            myFom1 = form;
            mySerialPortObject = new MySerialPort(RecieveCommandListener, sp);
            MySerialPort.SetRecieveMessageListener(RecieveMessageListener);
        }

        public static void ReadDeviceInfo() {
            MySerialPort.SendCommand(MT_RPC_CMD_SREQ|MT_RPC_SYS_SYS,MT_SYS_READ_INFO,null);
        }
        public static void ReadSerialNumber(String psw) {
            MySerialPort.SendCommand(MT_RPC_CMD_SREQ | MT_RPC_SYS_SYS, MT_SYS_READ_SN, null);
        }
        public static void WriteSerialNumber(ServerInfo info)
        {
            String output = JsonConvert.SerializeObject(info);
            byte[] buf = System.Text.Encoding.Default.GetBytes(output);
            MySerialPort.SendCommand(MT_RPC_CMD_SREQ | MT_RPC_SYS_SYS, MT_SYS_WRITE_SN, buf);
        }
        public static void WriteSerialNumber(String psw,String url, String svr_ipv4,int svr_udp_port,int svr_tcp_port,int local_port)
        {
            ServerInfo info = new ServerInfo { password = psw, 
                server_url = url, 
                server_ipv4 = svr_ipv4, 
                server_tcp_port = svr_tcp_port, 
                server_udp_port = svr_udp_port, 
                local_udp_port = local_port };
            String output = JsonConvert.SerializeObject(info);
            byte[] buf = System.Text.Encoding.Default.GetBytes(output);
            MySerialPort.SendCommand(MT_RPC_CMD_SREQ | MT_RPC_SYS_SYS, MT_SYS_WRITE_SN, buf);
        }
        public static ServerInfo ParseServiceInfo(String str) {
            ServerInfo sinfo = JsonConvert.DeserializeObject<ServerInfo>(str);
            return sinfo;
        }
        private static void RecieveCommandListener(byte cmd0, byte cmd1, byte[] data)
        {
            if ((cmd0 & MT_RPC_CMD_SRSP) == MT_RPC_CMD_SRSP)//only process response  command
            {
                if ((cmd0 & MT_RPC_SYS_SYS) == MT_RPC_SYS_SYS)//only process system request layer
                {
                    String str;
                    switch (cmd1)
                    {
                        case MT_SYS_READ_INFO://request read device info
                            str = System.Text.Encoding.ASCII.GetString(data);
                            myFom1.UpdateTextBox5(ConvertJsonString(str));
                            break;
                        case MT_SYS_READ_SN://request read device serial number info of remote service
                            str = System.Text.Encoding.ASCII.GetString(data);
                            myFom1.UpdateTextBox5(ConvertJsonString(str));
                            break;
                        case MT_SYS_WRITE_SN://request write device serial numer inof of remote service
                            break;
                    }
                }
            }
        }
        private static void RecieveMessageListener(byte[] tx, byte[] rx)
        {
            String tx_str,rx_str;
            if (myFom1 != null) { 
                if(tx!=null)
                {
                    tx_str = "\r\n[TRANSE]:" + System.Text.Encoding.ASCII.GetString(tx);
                    myFom1.UpdateTextBox16(tx_str);
                }
                if(rx!=null){
                    rx_str = "\r\n[RECIEVE]:" + System.Text.Encoding.ASCII.GetString(rx);
                    myFom1.UpdateTextBox16(rx_str);
                }          
            }
        }
        private static String ConvertJsonString(String str)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }
        public class ServerInfo{
            public int PID{get; set;}
            public int VID{ get; set; }
            public String product_date{ get; set; }
            public String password{ get; set; }
            public String sn{ get; set; }
            public String addr{ get; set; }
            public String server_url{ get; set; }
            public String server_ipv4{ get; set; }
            public int server_udp_port{ get; set; }
            public int server_tcp_port{ get; set; }
            public int local_udp_port{ get; set; }
        }
    }
}
