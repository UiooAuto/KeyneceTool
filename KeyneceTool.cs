using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SocketConnect
{
    public class KeyneceTool
    {
        public Socket socket;
        public IPAddress ipAddress;
        public int serverPort;
        public IPEndPoint ipEndPoint;
        public Ping ping;
        int times = 3;//重连次数
        int wait = 1000;//每次重连前等待多久
        public int ConnectTimeOut = 1000;
        static object lock1 = new object();

        public KeyneceTool(string serverIp, int serverPort)
        {
            this.ipAddress = IPAddress.Parse(serverIp);
            this.serverPort = serverPort;
            this.ipEndPoint = new IPEndPoint(ipAddress, serverPort);
            this.ping = new Ping();
        }

        public bool IpAddressPing()
        {
            PingReply pingReply = ping.Send(this.ipAddress, this.ConnectTimeOut);
            if (pingReply.Status == IPStatus.Success)
            {
                return true;
            }
            return false;
        }

        public bool ReConnect()
        {
            while (times > 0)
            {
                CloseConnect();
                Thread.Sleep(wait);
                if (Connect())
                {
                    return true;
                }
                times--;
            }

            return false;
        }

        public bool Connect()
        {
            try
            {
                if (IpAddressPing())
                {
                    this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    this.socket.SendTimeout = ConnectTimeOut;
                    this.socket.ReceiveTimeout = ConnectTimeOut;
                    this.socket.Connect(this.ipEndPoint);
                }
                else
                {
                    //MessageBox.Show("连接超时");
                    return false;
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
                return false;
            }

            return true;
        }

        public void CloseConnect()
        {
            if (socket != null)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                }
                try
                {
                    socket.Close();
                }
                catch
                {
                }
            }
            try
            {
                ping.Dispose();
                ((IDisposable)this).Dispose();
            }
            catch
            {
            }
        }

        #region 读写底层
        public bool Sendto(byte[] cmd)
        {
            if (socket != null)
            {
                int i;
                if (cmd == null)
                {
                    return false;
                }
                try
                {
                    i = socket.Send(cmd);
                }
                catch
                {
                    return false;
                }
                if (i > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 从Socket对象中获取数据
        /// </summary>
        /// <param name="a">读取的数据长度</param>
        /// <returns>读取到的数据</returns>
        public byte[] Recivefrom(out int a)
        {
            if (socket != null)
            {
                byte[] recBytes = new byte[1024];
                try
                {
                    a = socket.Receive(recBytes);
                }
                catch
                {
                    a = 0;
                    return null;
                }
                if (a == 0)
                {
                    return null;
                }

                return recBytes;
            }
            else
            {
                a = 0;
                return null;
            }

        }

        /// <summary>
        /// 与服务器发起一次交互
        /// </summary>
        /// <param name="cmd">发送给服务器的命令</param>
        /// <returns>从服务器接收到的返回数据</returns>
        public byte[] SendAndRecivefrom(byte[] cmd, out int recNum)
        {
            lock (lock1)
            {
                byte[] recBytes;
                bool sendOK = Sendto(cmd);
                if (sendOK)
                {
                    recBytes = Recivefrom(out recNum);
                    if (recNum == 0)
                    {
                        return null;
                    }
                    else
                    {
                        return recBytes;
                    }
                }
                else
                {
                    recNum = 0;
                    return null;
                }
            }
        }

        #endregion


        #region 写入PLC

        public bool Write(string address, ushort cmd)
        {
            int readLength;
            string cmdStr = "WR " + address + ".U " + cmd + "\r";
            byte[] recBytes = SendAndRecivefrom(Encoding.ASCII.GetBytes(cmdStr), out readLength);

            if (recBytes != null)
            {
                string str = Encoding.ASCII.GetString(recBytes, 0, readLength);
                if ("OK\r\n".Equals(str))
                {
                    return true;
                }
                else
                {
                    //MessageBox.Show("错误信息" + str);
                    return false;
                }
            }
            return false;
        }

        public bool Write(string address, short cmd)
        {
            int readLength;
            string cmdStr = "WR " + address + ".S " + cmd + "\r";
            byte[] recBytes = SendAndRecivefrom(Encoding.ASCII.GetBytes(cmdStr), out readLength);

            if (recBytes != null)
            {
                string str = Encoding.ASCII.GetString(recBytes, 0, readLength);
                if ("OK\r\n".Equals(str))
                {
                    return true;
                }
                else
                {
                    //MessageBox.Show("错误信息" + str);
                    return false;
                }
            }
            return false;
        }

        public bool Write(string address, ushort[] cmd)
        {
            string cmdStr = "";
            int readLength;
            for (int i = 0; i < cmd.Length; i++)
            {
                cmdStr = cmdStr + " " + cmd[i];
            }
            cmdStr = "WRS " + address + ".U " + cmd.Length + cmdStr + "\r";
            byte[] recBytes = SendAndRecivefrom(Encoding.ASCII.GetBytes(cmdStr), out readLength);

            if (recBytes != null)
            {
                string recStr = Encoding.ASCII.GetString(recBytes, 0, readLength);
                if ("OK\r\n".Equals(recStr))
                {
                    return true;
                }
                else
                {
                    //MessageBox.Show(str);
                    return false;
                }
            }
            return false;
        }

        public bool Write(string address, short[] cmd)
        {
            string cmdStr = "";
            int readLength;
            for (int i = 0; i < cmd.Length; i++)
            {
                cmdStr = cmdStr + " " + cmd[i];
            }
            cmdStr = "WRS " + address + ".S " + cmd.Length + cmdStr + "\r";
            byte[] recBytes = SendAndRecivefrom(Encoding.ASCII.GetBytes(cmdStr), out readLength);
            if (recBytes != null)
            {
                string recStr = Encoding.ASCII.GetString(recBytes, 0, readLength);
                if ("OK\r\n".Equals(recStr))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public bool Write(string address, uint cmd)
        {
            int readLength;
            string cmdStr = "WR " + address + ".D " + cmd + "\r";
            byte[] recBytes = SendAndRecivefrom(Encoding.ASCII.GetBytes(cmdStr), out readLength);

            if (recBytes != null)
            {
                string recStr = Encoding.ASCII.GetString(recBytes, 0, readLength);
                if ("OK\r\n".Equals(recStr))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public bool Write(string address, int cmd)
        {
            int readLength;
            string cmdStr = "WR " + address + ".L " + cmd + "\r";
            byte[] recBytes = SendAndRecivefrom(Encoding.ASCII.GetBytes(cmdStr), out readLength);

            if (recBytes != null)
            {
                string recStr = Encoding.ASCII.GetString(recBytes, 0, readLength);
                if ("OK\r\n".Equals(recStr))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public bool Write(string address, uint[] cmd)
        {
            string cmdStr = "";
            int readLength;
            for (int i = 0; i < cmd.Length; i++)
            {
                cmdStr = cmdStr + " " + cmd[i];
            }
            cmdStr = "WRS " + address + ".D " + cmd.Length + cmdStr + "\r";
            byte[] recBytes = SendAndRecivefrom(Encoding.ASCII.GetBytes(cmdStr), out readLength);

            if (recBytes != null)
            {
                string recStr = Encoding.ASCII.GetString(recBytes, 0, readLength);
                if ("OK\r\n".Equals(recStr))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public bool Write(string address, int[] cmd)
        {
            string cmdStr = "";
            int readLength;
            for (int i = 0; i < cmd.Length; i++)
            {
                cmdStr = cmdStr + " " + cmd[i];
            }
            cmdStr = "WRS " + address + ".L " + cmd.Length + cmdStr + "\r";
            byte[] recBytes = SendAndRecivefrom(Encoding.ASCII.GetBytes(cmdStr), out readLength);

            if (recBytes != null)
            {
                string recStr = Encoding.ASCII.GetString(recBytes, 0, readLength);
                if ("OK\r\n".Equals(recStr))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public bool Write(string address, string cmd)
        {
            string str = "";
            ushort[] ushorts;
            byte[] strByteArr = Encoding.ASCII.GetBytes(cmd);
            if ((strByteArr.Length % 2) == 0)
            {
                ushorts = new ushort[strByteArr.Length / 2];
            }
            else
            {
                ushorts = new ushort[(strByteArr.Length / 2) + 1];
            }
            int readLength;

            for (int i = 0; i < ushorts.Length; i++)
            {
                ushorts[i] = 0x0000;
                ushorts[i] = (ushort)(ushorts[i] | strByteArr[i * 2]);
                ushorts[i] = (ushort)(ushorts[i] << 8);
                if (strByteArr.Length > (i * 2) + 1)
                {
                    ushorts[i] = (ushort)(ushorts[i] | strByteArr[(i * 2) + 1]);
                }

                str = str + " " + ushorts[i];
            }

            string cmdStr = "WRS " + address + ".U " + ushorts.Length + str + "\r";
            byte[] recBytes = SendAndRecivefrom(Encoding.ASCII.GetBytes(cmdStr), out readLength);

            if (recBytes != null)
            {
                string recStr = Encoding.ASCII.GetString(recBytes, 0, readLength);
                if ("OK\r\n".Equals(recStr))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        #endregion

        #region 读取PLC

        private bool TryRead(string cmd, out byte[] readResult, out int getLength)
        {
            byte[] recBytes = null;
            bool loop = true;
            int readLength = 0;
            while (loop)
            {
                recBytes = null;
                recBytes = SendAndRecivefrom(Encoding.ASCII.GetBytes(cmd), out readLength);

                if (recBytes != null)
                {
                    loop = false;
                }
                else
                {
                    if (!ReConnect())
                    {
                        getLength = 0;
                        readResult = null;
                        return false;
                    }
                }
            }
            getLength = readLength;
            readResult = recBytes;
            return true;
        }

        /// <summary>
        /// 读取PLC的内容
        /// </summary>
        /// <param name="address">读取的地址</param>
        /// <returns>读取的结果</returns>
        public ReadResult<ushort> ReadU16(string address)
        {
            int readLength;
            string recStr;
            byte[] bytes = null;
            ReadResult<ushort> result = new ReadResult<ushort>();
            result.isSuccess = false; //默认值为false，失败

            bool isSuccess = TryRead("RD " + address + ".U\r", out bytes, out readLength);

            if (isSuccess)
            {
                recStr = Encoding.ASCII.GetString(bytes, 0, readLength);
                ushort recShort = ushort.Parse(recStr);
                result.isSuccess = true;
                result.result = recShort;
            }
            return result;
        }

        /// <summary>
        /// 读取PLC的内容
        /// </summary>
        /// <param name="address">读取的地址</param>
        /// <returns>读取的结果</returns>
        public ReadResult<short> Read16(string address)
        {
            int readLength;
            string recStr;
            byte[] bytes = null;
            ReadResult<short> result = new ReadResult<short>();
            result.isSuccess = false; //默认值为false，失败

            bool isSuccess = TryRead("RD " + address + ".S\r", out bytes, out readLength);

            if (isSuccess)
            {
                recStr = Encoding.ASCII.GetString(bytes, 0, readLength);
                short recShort = short.Parse(recStr);
                result.isSuccess = true;
                result.result = recShort;
            }
            return result;
        }

        public ReadResult<ushort[]> ReadU16(string address, int length)
        {
            int readLength;
            string recStr;
            byte[] bytes = null;
            ReadResult<ushort[]> result = new ReadResult<ushort[]>();
            result.isSuccess = false; //默认值为false，失败

            bool isSuccess = TryRead("RDS " + address + ".U " + length + "\r", out bytes, out readLength);

            if (isSuccess)
            {
                recStr = Encoding.ASCII.GetString(bytes, 0, readLength);
                var indexOf = recStr.IndexOf('\r');
                if (indexOf != -1)
                {
                    recStr = recStr.Substring(0, indexOf);
                }
                char[] chars = { ' ' };
                string[] strings = recStr.Split(chars); //按照空格分隔
                ushort[] ushorts = new ushort[strings.Length];
                for (int i = 0; i < strings.Length; i++)
                {
                    ushorts[i] = ushort.Parse(strings[i]);
                }
                result.isSuccess = true;
                result.result = ushorts;
            }
            return result;
        }

        public ReadResult<short[]> Read16(string address, int length)
        {
            int readLength = 0;
            string recStr;
            bool b = false;
            byte[] bytes = null;
            ReadResult<short[]> result = new ReadResult<short[]>();
            result.isSuccess = false; //默认值为false，失败

            bool isSuccess = TryRead("RDS " + address + ".S " + length + "\r", out bytes, out readLength);

            if (isSuccess)
            {
                recStr = Encoding.ASCII.GetString(bytes, 0, readLength);
                var indexOf = recStr.IndexOf('\r');
                if (indexOf != -1)
                {
                    recStr = recStr.Substring(0, indexOf);
                }
                char[] chars = { ' ' };
                string[] strings = recStr.Split(chars); //按照空格分隔
                short[] shorts = new short[length];
                for (int i = 0; i < strings.Length; i++)
                {
                    shorts[i] = short.Parse(strings[i]);
                }
                result.isSuccess = true;
                result.result = shorts;
            }
            return result;
        }

        public ReadResult<uint> ReadU32(string address)
        {
            int readLength;
            string recStr;
            byte[] bytes = null;
            ReadResult<uint> result = new ReadResult<uint>();
            result.isSuccess = false; //默认值为false，失败

            bool isSuccess = TryRead("RD " + address + ".D\r", out bytes, out readLength);

            if (isSuccess)
            {
                recStr = Encoding.ASCII.GetString(bytes, 0, readLength);
                uint recShort = uint.Parse(recStr);
                result.isSuccess = true;
                result.result = recShort;
            }
            return result;
        }

        public ReadResult<int> Read32(string address)
        {
            int readLength;
            string recStr;
            byte[] bytes = null;
            ReadResult<int> result = new ReadResult<int>();
            result.isSuccess = false; //默认值为false，失败

            bool isSuccess = TryRead("RD " + address + ".L\r", out bytes, out readLength);

            if (isSuccess)
            {
                recStr = Encoding.ASCII.GetString(bytes, 0, readLength);
                int recInt = int.Parse(recStr);
                result.isSuccess = true;
                result.result = recInt;
            }
            return result;
        }

        public ReadResult<uint[]> ReadU32(string address, int length)
        {
            int readLength;
            string recStr;
            byte[] bytes = null;
            ReadResult<uint[]> result = new ReadResult<uint[]>();
            result.isSuccess = false; //默认值为false，失败

            bool isSuccess = TryRead("RD " + address + ".L\r", out bytes, out readLength);

            if (isSuccess)
            {
                recStr = Encoding.ASCII.GetString(bytes, 0, readLength);
                var indexOf = recStr.IndexOf('\r');
                if (indexOf != -1)
                {
                    recStr = recStr.Substring(0, indexOf);
                }
                char[] chars = { ' ' };
                string[] strings = recStr.Split(chars); //按照空格分隔
                uint[] uints = new uint[strings.Length];
                for (int i = 0; i < strings.Length; i++)
                {
                    uints[i] = uint.Parse(strings[i]);
                }
                result.isSuccess = true;
                result.result = uints;
            }
            return result;
        }

        public ReadResult<int[]> Read32(string address, int length)
        {
            int readLength;
            string recStr;
            byte[] bytes = null;
            ReadResult<int[]> result = new ReadResult<int[]>();
            result.isSuccess = false; //默认值为false，失败

            bool isSuccess = TryRead("RD " + address + ".L\r", out bytes, out readLength);

            if (isSuccess)
            {
                recStr = Encoding.ASCII.GetString(bytes, 0, readLength);
                var indexOf = recStr.IndexOf('\r');
                if (indexOf != -1)
                {
                    recStr = recStr.Substring(0, indexOf);
                }
                char[] chars = { ' ' };
                string[] strings = recStr.Split(chars); //按照空格分隔
                int[] ints = new int[strings.Length];
                for (int i = 0; i < strings.Length; i++)
                {
                    ints[i] = int.Parse(strings[i]);
                }
                result.isSuccess = true;
                result.result = ints;
            }
            return result;
        }

        public ReadResult<string> ReadString(string address, int length)
        {
            string recStr;
            byte[] bytes;
            ReadResult<string> result = new ReadResult<string>();
            result.isSuccess = false; //默认值为false，失败
            ReadResult<ushort[]> readResult = ReadU16(address, length);
            if (readResult.isSuccess)
            {
                try
                {
                    bytes = new byte[readResult.result.Length * 2];
                    for (int i = 0; i < readResult.result.Length; i++)
                    {
                        bytes[(i * 2) + 1] = (byte)(readResult.result[i] & 0x00ff);
                        bytes[i * 2] = (byte)(readResult.result[i] >> 8);
                    }
                    recStr = Encoding.ASCII.GetString(bytes);
                    result.result = recStr;
                    result.isSuccess = true;
                    return result;
                }
                catch
                {

                }

            }
            return result;
        }

        #endregion
    }

    public class ReadResult<T>
    {
        public bool isSuccess;
        public T result;
    }
}