using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;


namespace tcpserver
{

    public partial class TcpServerForm : Form
    {
        public TcpServerForm()
        {
            InitializeComponent();

            listBox1.HorizontalScrollbar = true;
        }
        
        TcpListener _tcpserver;
        
        const int MAXNUMCLIENTS = 3;
        
        TcpClient[] clients = new TcpClient[MAXNUMCLIENTS];
        
        int _countClient = 0;
        
        bool _stopNetwork;


        private void buttonStart_Click(object sender, EventArgs e)
        {
            StartServer();
        }


        private void buttonSend_Click(object sender, EventArgs e)
        {
            string s = "[Server]" + ": " + textBoxSend.Text;
            SendToClients(s, -1);
        }


        private void buttonStop_Click(object sender, EventArgs e)
        {
            StopServer();
        }

        private void TcpServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopServer();
        }

        
        void StartServer()
        {
            if (_tcpserver == null)
            {
                try
                {
                    _stopNetwork = false;
                    _countClient = 0;
                    UpdateClientsDisplay();

                    int port = int.Parse(textBoxPort.Text);
                    _tcpserver = new TcpListener(IPAddress.Any, port);
                    _tcpserver.Start();

                    MessageBox.Show("Сервер успешно запущен!", "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    Thread acceptThread = new Thread(AcceptClients);
                    acceptThread.Start();


                }
                catch
                {
                    MessageBox.Show("Ошибка запуска сервера!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        
        void StopServer()
        {
            if (_tcpserver != null)
            {
                _tcpserver.Stop();
                _tcpserver = null;
                _stopNetwork = true;

                for (int i = 0; i < MAXNUMCLIENTS; i++)
                {
                    if (clients[i] != null) clients[i].Close();
                }
            }
        }
        
        void AcceptClients()
        {
            while (true)
            {
                try
                {
                    this.clients[_countClient] = _tcpserver.AcceptTcpClient();
                    Thread readThread = new Thread(ReceiveRun);
                    readThread.Start(_countClient);
                    _countClient++;

                    
                    Invoke(new UpdateClientsDisplayDelegate(UpdateClientsDisplay));
                }
                catch 
                {
                    //MessageBox.Show("Ошибка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }


                if (_countClient == MAXNUMCLIENTS || _stopNetwork == true)
                {
                    break;
                }

            }
        }

        
        void SendToClients(string text, int skipindex)
        {
            for (int i = 0; i < MAXNUMCLIENTS; i++)
            {
                if (clients[i] != null)
                {
                    if (i == skipindex) continue;

                    
                    NetworkStream ns = clients[i].GetStream();
                    byte[] myReadBuffer = Encoding.Default.GetBytes(text);
                    ns.BeginWrite(myReadBuffer, 0, myReadBuffer.Length,
                                                                 new AsyncCallback(AsyncSendCompleted), ns);
                }
            }
        }
        
        public void AsyncSendCompleted(IAsyncResult ar)
        {
            NetworkStream ns = (NetworkStream)ar.AsyncState;
            ns.EndWrite(ar);
        }

        
        void ReceiveRun(object num)
        {
            while (true)
            {
                try
                {
                    string s = null;
                    NetworkStream ns = clients[(int)num].GetStream();
                    
                    while (ns.DataAvailable == true)
                    {
                        byte[] buffer = new byte[clients[(int)num].Available];

                        ns.Read(buffer, 0, buffer.Length);
                        s += Encoding.Default.GetString(buffer);
                    }

                    if (s != null)
                    {
                        Invoke(new UpdateReceiveDisplayDelegate(UpdateReceiveDisplay), new object[] { (int)num, s });
                        
                        s = "User_" + ((int)num).ToString() + ": " + s;
                        SendToClients(s, (int)num);
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

        
        public void UpdateReceiveDisplay(int clientnum, string message)
        {
            listBox1.Items.Add("User_" + clientnum.ToString() + ": " + message);
        }
        protected delegate void UpdateReceiveDisplayDelegate(int clientcount, string message);


        public void UpdateClientsDisplay()
        {
            labelCountClient.Text = _countClient.ToString();
        }

        protected delegate void UpdateClientsDisplayDelegate();
    }
}
