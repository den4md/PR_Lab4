using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Threading;



namespace tcpclient
{
    public partial class TcpClientForm : Form
    {
        public TcpClientForm()
        {
            InitializeComponent();
        }
        
        TcpClient _tcpСlient = new TcpClient();
        
        NetworkStream ns;
        
        bool _stopNetwork;

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            Connect();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseClient();
        }


        void Connect()
        {
            try
            {
                _tcpСlient.Connect(textBoxIP.Text, int.Parse(textBoxPort.Text));

                ns = _tcpСlient.GetStream();

                Thread th = new Thread(ReceiveRun);
                th.Start();

            }
            catch 
            {
                MessageBox.Show("Ошибка подключения", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void CloseClient()
        {
            if (ns != null) ns.Close();
            if (_tcpСlient != null) _tcpСlient.Close();

            _stopNetwork = true;
        }

        
        void SendMessage()
        {
            if (ns != null)
            {
                byte[] buffer = Encoding.Default.GetBytes(textBoxSend.Text);
                ns.Write(buffer, 0, buffer.Length);
            }
        }

        
        void ReceiveRun()
        {
            while (true)
            {
                try
                {
                    string s = null;
                    while (ns.DataAvailable == true)
                    {
                        byte[] buffer = new byte[_tcpСlient.Available];

                        ns.Read(buffer, 0, buffer.Length);
                        s += Encoding.Default.GetString(buffer);
                    }

                    if (s != null)
                    {
                        ShowReceiveMessage(s);
                        s = String.Empty;
                    }

                    Thread.Sleep(100);
                }
                catch
                {
                    MessageBox.Show("Ошибка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (_stopNetwork == true) break;

            }
        }


        delegate void UpdateReceiveDisplayDelegate(string message);
        void ShowReceiveMessage(string message)
        {
            if (listBox1.InvokeRequired == true)
            {
                UpdateReceiveDisplayDelegate rdd = new UpdateReceiveDisplayDelegate(ShowReceiveMessage);
                
                Invoke(rdd, new object[] { message });
            }
            else
            {
                listBox1.Items.Add(message);
            }
        }
    }
}
