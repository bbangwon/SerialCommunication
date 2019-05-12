using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SerialCommunication
{
    public partial class Form1 : Form
    {
        enum LogType
        {
            System,
            Send,
            Receive
        }

        private SerialPort _Port;

        private SerialPort Port
        {
            get
            {
                if(_Port == null)
                {
                    _Port = new SerialPort();
                    _Port.PortName = "COM1";
                    _Port.BaudRate = 9600;
                    _Port.DataBits = 8;
                    _Port.Parity = Parity.None;
                    _Port.Handshake = Handshake.None;
                    _Port.StopBits = StopBits.One;
                    _Port.DataReceived += _Port_DataReceived;
                }
                return _Port;
            }
        }

        private Boolean IsOpen
        {
            get => Port.IsOpen;
            set
            {
                if(value)
                {
                    Log(LogType.System, "연결 됨");
                    btnConnect.Text = "연결 끊기";
                    btnSend.Enabled = true;
                    gbSettings.Enabled = false;                    
                }
                else
                {
                    Log(LogType.System, "연결 해제됨");
                    btnConnect.Text = "연결";
                    btnSend.Enabled = false;
                    gbSettings.Enabled = true;
                }
            }
        }

        private StringBuilder _String;
        private String Strings
        {
            set
            {
                if (_String == null)
                    _String = new StringBuilder(1024);

                if (_String.Length >= (1024 - value.Length))
                    _String.Clear();

                _String.AppendLine(value);
                tbMessage.Text = _String.ToString();
                tbMessage.Select(tbMessage.Text.Length, 0);
                tbMessage.ScrollToCaret();
            }
        }

        private bool _timer = false;
        

        void Log(LogType logType, string formatString, params object[] objs)
        {
            string sLogType = logType.ToString();
            string sLogTime = DateTime.Now.ToString("H:mm:ss.fff");

            string sLog = string.Format(formatString, objs);

            Strings = string.Format("[{0}] ({1}) {2}", sLogType, sLogTime, sLog);
        }

        private void _Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            String msg = Port.ReadExisting();
            this.Invoke(new EventHandler(delegate
            {
                Log(LogType.Receive, msg);
            }));
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cbPort.DataSource = SerialPort.GetPortNames();

            cbBaudrate.SelectedIndex = 0;
            cbData.SelectedIndex = 0;
            cbParity.SelectedIndex = 0;
            cbHandshake.SelectedIndex = 0;

            sendTimer.Tick += SendTimer_Tick;
        }

        private void SendTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                Port.Write(new byte[] { 0xFF }, 0, 1);
                Log(LogType.Send, "0xFF");
            }
            catch(Exception ex)
            {
                Log(LogType.System, ex.Message);
            }           
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if(!Port.IsOpen)
            {
                Port.PortName = cbPort.SelectedItem.ToString();
                Port.BaudRate = Convert.ToInt32(cbBaudrate.SelectedItem);
                Port.DataBits = Convert.ToInt32(cbData.SelectedItem);
                Port.Parity = (Parity)cbParity.SelectedIndex;
                Port.Handshake = (Handshake)cbHandshake.SelectedIndex;

                try
                {
                    Port.Open();
                }
                catch (Exception ex) { Log(LogType.System, ex.Message); }                              
            }
            else
            {
                TimerStop();
                Port.Close();
            }

            IsOpen = Port.IsOpen;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if(!_timer)
            {
                _timer = true;
                btnSend.Text = "보내기 중지";
                Log(LogType.System, "보내기 시작");
                sendTimer.Start();
                return;
            }

            TimerStop();
        }

        void TimerStop()
        {
            if(_timer)
            {
                _timer = false;
                btnSend.Text = "반복 0xFF 보내기";
                Log(LogType.System, "보내기 중지");
                sendTimer.Stop();
            }
        }
    }
}
