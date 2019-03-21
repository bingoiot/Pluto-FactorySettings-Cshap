using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO.Ports;

namespace Pluto_FactorySettings
{
    public partial class Form1 : Form
    {
        private static int DebugTextLength = 0;
        private static FactorySettings myFactorySettings;
        delegate void UpdateTextBoxDelegate(String str);
        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(",") ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");
            dllName = dllName.Replace(".", "_");
            if (dllName.EndsWith("_resources")) return null;
            System.Resources.ResourceManager rm = new System.Resources.ResourceManager(GetType().Namespace + ".Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());
            byte[] bytes = (byte[])rm.GetObject(dllName);
            return System.Reflection.Assembly.Load(bytes);
        }
        public Form1()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            InitializeComponent();
            myFactorySettings = new FactorySettings(this, SerialPort1);
            UpdateSerialPortName();
        }
        public void UpdateTextBox16(String str) {
            UpdateTextBoxDelegate d = new UpdateTextBoxDelegate(UpdataTextBox16);
            this.BeginInvoke(d, str);
        }
        private void UpdataTextBox16(String str){
            DebugTextLength += str.Length;
            String cstr = textBox16.Text;
            if (DebugTextLength > (1024 * 1024)) {
                cstr.Remove(0, str.Length);
                DebugTextLength -= str.Length;
            }
            textBox16.Text = cstr + str;
            textBox16.SelectionStart = DebugTextLength;
            textBox16.ScrollToCaret();
        }
        public void UpdateTextBox5(String str) {
            UpdateTextBoxDelegate d = new UpdateTextBoxDelegate(UpdataTextBox5);
            this.BeginInvoke(d,str);
        }
        private void UpdataTextBox5(String str){
            textBox5.Text = str;
            textBox5.SelectionStart = DebugTextLength;
            textBox5.ScrollToCaret();
            FactorySettings.ServerInfo info = FactorySettings.ParseServiceInfo(str);
            if (info != null) {
                textBox6.Text = info.server_url;
                textBox7.Text = info.server_ipv4;
                textBox8.Text = info.server_udp_port.ToString();
                textBox9.Text = info.server_tcp_port.ToString();
                textBox10.Text = "reserve";
                textBox11.Text = info.local_udp_port.ToString();
                textBox12.Text = "reserve";
                textBox15.Text = info.password;
                textBox13.Text = info.addr;
                textBox14.Text = info.sn;
                textBox17.Text = info.VID.ToString();
                textBox18.Text = info.PID.ToString();
                textBox19.Text = info.product_date;
                if (info.server_ipv4==null)
                {
                    LoadDefaultValue();
                }
            }
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MySerialPort.Port.IsOpen == true)
            {
                MySerialPort.Port.Close();
                MySerialPort.SaveParameter(comboBox2.Text, comboBox3.Text, comboBox5.Text, comboBox4.Text,comboBox6.Text);
                try
                {
                    MySerialPort.Port.Open();
                }
                catch
                { Connect.Text = "连接"; MessageBox.Show("串口不存在或者被其他应用程序占用！", "提示"); }

            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.Text == "customize..")
            {
                comboBox3.SelectedText = "";
            }
        }
        private void UpdateSerialPortName()
        {
            if (MySerialPort.Port.IsOpen == true)
            {
                MessageBox.Show("请先关闭串口", "提示", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            else
            {
                comboBox2.Items.Clear();
                string[] ArryPort = SerialPort.GetPortNames();
                if (ArryPort != null)
                {
                    for (int i = 0; i < ArryPort.Length; i++)
                    {
                        comboBox2.Items.Add(ArryPort[i]);
                    }
                }
                if (comboBox2.Items.Count > 0)
                    comboBox2.SelectedIndex = 0;
                else
                    comboBox2.Text = "";
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Width = 680;
            this.Height = 680;
            LoadDefaultValue();
            timer1.Enabled = true;
        }
        private void LoadDefaultValue(){
            textBox6.Text = "www.glalaxy.com";// info.server_url;
            textBox7.Text = "119.23.8.181";// info.server_ipv4;
            textBox8.Text = "16729";// info.server_udp_port.ToString();
            textBox9.Text = "16729";// info.server_tcp_port.ToString();
            textBox10.Text = "reserve";
            textBox11.Text = "16729";// info.local_udp_port.ToString();
            textBox12.Text = "reserve";
            textBox15.Text = "123456";// info.password;
            textBox13.Text = "";//info.addr;
            textBox14.Text = "";//info.sn;
            textBox17.Text = "4294967295";//info.VID.ToString();
            textBox18.Text = "01";//info.PID.ToString();
            textBox19.Text = "2019-03-01";//info.product_date;
        }
        private void Connect_Click(object sender, EventArgs e)
        {
            try
            {
                if (MySerialPort.Port.IsOpen == false)
                {
                    MySerialPort.SaveParameter(comboBox2.Text, comboBox3.Text, comboBox5.Text, comboBox4.Text, comboBox6.Text);
                    MySerialPort.Port.Open();
                    Connect.Image = Properties.Resources.Close;
                }
                else
                {
                    MySerialPort.Port.Close();
                    Connect.Image = Properties.Resources.Open;
                }
            }
            catch
            { MessageBox.Show("串口不存在或者被其他应用程序占用！", "提示"); }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            FactorySettings.ReadDeviceInfo();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FactorySettings.ReadSerialNumber("123456");
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.Hide();
            this.Width = 1440;
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.Show();
            this.Width = 680;
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            textBox16.Text = "";
        }
        private void button4_Click(object sender, EventArgs e)
        {
            FactorySettings.ReadDeviceInfo();
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            FactorySettings.ReadSerialNumber("123456");
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            FactorySettings.ServerInfo info = new FactorySettings.ServerInfo();
            /*
                textBox6.Text = info.server_url;
                textBox7.Text = info.server_ipv4;
                textBox8.Text = info.server_udp_port.ToString();
                textBox9.Text = info.server_tcp_port.ToString();
                textBox10.Text = "reserve";
                textBox11.Text = info.local_udp_port.ToString();
                textBox12.Text = "reserve";
                textBox15.Text = info.password;
                textBox13.Text = info.addr;
                textBox14.Text = info.sn;
                textBox17.Text = info.VID.ToString();
                textBox18.Text = info.PID.ToString();
                textBox19.Text = info.product_date;
            */
            try
            {
                info.PID = int.Parse(textBox18.Text);
                info.VID =(int)long.Parse(textBox17.Text);
                info.product_date = textBox19.Text;
                info.sn = textBox14.Text;
                info.addr = textBox13.Text;
                info.password = textBox15.Text;
                info.local_udp_port = int.Parse(textBox11.Text);
                info.server_tcp_port = int.Parse(textBox9.Text);
                info.server_udp_port = int.Parse(textBox8.Text);
                info.server_ipv4 = textBox7.Text;
                info.server_url = textBox6.Text;
                if (info.PID == 0)
                    MessageBox.Show("please input Product ID");
                if (info.VID == 0)
                    MessageBox.Show("please input Vendor ID");
                if (info.product_date == null)
                    MessageBox.Show("please input Product Date");
                if (info.sn == null)
                    MessageBox.Show("please input SN");
                if (info.addr == null)
                    MessageBox.Show("please input Device Adress");
                if (info.password == null)
                    MessageBox.Show("please input Protect Password");
                if (info.server_ipv4 == null)
                    MessageBox.Show("please input Server IPv4 Adress");
                if (info.server_url == null)
                    MessageBox.Show("please input Server URL");
                FactorySettings.WriteSerialNumber(info);
            }
            catch {
                MessageBox.Show("you shoud input something words in textbox");
            }
        }

        private void textBox19_TextChanged(object sender, EventArgs e)
        {

        }
        private static byte[] RxBuffer = new byte[1024];
        private static byte[] SerialPort_ReadBuffer = new byte[4096];
        private static int SerialPort_RxCnt = 0;
        private Object Critical = new Object();
        private void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int len = SerialPort1.Read(RxBuffer, 0, 1024);
                if (len > 0)
                {
                    lock (Critical)
                    {
                        if ((SerialPort_RxCnt + len) >= 4096)
                        {
                            SerialPort_RxCnt = 0;
                        }
                        System.Array.Copy(RxBuffer, 0, SerialPort_ReadBuffer, SerialPort_RxCnt, len);
                        SerialPort_RxCnt += len;
                    }
                }
            }
            catch
            {
                // MessageBox.Show("串口接收错误");

            }

        }
        private void timer1_Tick(object sender, EventArgs e)
        {
        
            if(SerialPort_RxCnt>0)
            {
                lock (Critical)
                {
                    MySerialPort.InputMessage(SerialPort_ReadBuffer, SerialPort_RxCnt);
                    SerialPort_RxCnt = 0;
                }
            }
        }
    }
}
