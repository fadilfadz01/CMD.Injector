// minimalistic telnet implementation
// by [m][e] 2021
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MinimalisticTelnet
{
    enum Verbs
    {
        WILL = 251,
        WONT = 252,
        DO = 253,
        DONT = 254,
        IAC = 255
    }

    enum Options
    {
        SGA = 3
    }

    class TelnetConnection
    {
        TcpClient tcpSocket;

        int TimeOutMs = 3000; //1000;

        //
        public TelnetConnection(string Hostname, int Port)
        {
            tcpSocket = new TcpClient(AddressFamily.InterNetwork); // (); TcpClient(Hostname, Port);

            Task.Delay(1000);

            tcpSocket.ConnectAsync(Hostname, Port);

            Task.Delay(1000);

        }

        // CloseConnection
        public int CloseConnection()
        {
            
            tcpSocket.Dispose();
            
            Task.Delay(1000);

            return 0;

        }


        /*
        public string Login(string Username,string Password,int LoginTimeOutMs)
        {
            int oldTimeOutMs = TimeOutMs;
            TimeOutMs = LoginTimeOutMs;
            
            
            string s = Read();

            Task.Delay(LoginTimeOutMs);
           
            / *
            if (!s.TrimEnd().EndsWith(":"))
            {
                Debug.WriteLine("Failed to connect : no login prompt");
                
            }
            else
                WriteLine(Username);

            s += Read();
            if (!s.TrimEnd().EndsWith(":"))
            {
                Debug.WriteLine("Failed to connect : no password prompt");
            }
            else
                WriteLine(Password);

            * /

            s += Read();
            TimeOutMs = oldTimeOutMs;

            return s;
            
        }
        */

        // WriteLine
        public void WriteLine(string cmd)
        {
            //Write(cmd + "\n");
            Write(cmd + "\r\n");

            //Task.Delay(TimeOutMs);
            //Task.Delay(1000);
        }

        // Write
        public void Write(string cmd)
        {
            if (!tcpSocket.Connected) return;
            byte[] buf = System.Text.ASCIIEncoding.ASCII.GetBytes(cmd.Replace("\0xFF","\0xFF\0xFF"));
            tcpSocket.GetStream().Write(buf, 0, buf.Length);

            //Task.Delay(TimeOutMs);
            //Task.Delay(1000);
        }

        // Read
        public string Read()
        {
            //Task.Delay(10000);

            if (!tcpSocket.Connected) return null;

            StringBuilder sb=new StringBuilder();
            do
            {
                ParseTelnet(sb);

                //System.Threading.Thread.Sleep(TimeOutMs);
                Task.Delay(TimeOutMs);
                

            } while (tcpSocket.Available > 0);

            //Task.Delay(1000);

            return sb.ToString();
        }

        // IsConnected
        public bool IsConnected
        {
            get { return tcpSocket.Connected; }
        }

        // ParseTelnet
        void ParseTelnet(StringBuilder sb)
        {
            while (tcpSocket.Available > 0)
            {
                int input = tcpSocket.GetStream().ReadByte();
                switch (input)
                {
                    case -1 :
                        break;
                    case (int)Verbs.IAC:
                        // interpret as command
                        int inputverb = tcpSocket.GetStream().ReadByte();
                        if (inputverb == -1) break;
                        switch (inputverb)
                        {
                            case (int)Verbs.IAC: 
                                //literal IAC = 255 escaped, so append char 255 to string
                                sb.Append(inputverb);
                                break;

                            case (int)Verbs.DO: 
                            case (int)Verbs.DONT:
                            case (int)Verbs.WILL:
                            case (int)Verbs.WONT:
                                // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                                int inputoption = tcpSocket.GetStream().ReadByte();

                                if (inputoption == -1) break;

                                tcpSocket.GetStream().WriteByte((byte)Verbs.IAC);

                                if (inputoption == (int)Options.SGA)
                                {
                                    tcpSocket.GetStream().WriteByte
                                    (
                                        inputverb == (int)Verbs.DO ? (byte)Verbs.WILL : (byte)Verbs.DO
                                    );
                                }
                                else
                                {
                                    tcpSocket.GetStream().WriteByte
                                    (
                                        inputverb == (int)Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT
                                    );
                                }

                                tcpSocket.GetStream().WriteByte((byte)inputoption);
                                break;

                            default:
                                break;
                        }
                        break;

                    default:
                        sb.Append( (char)input );
                        break;
                }
            }
        }
    }
}
