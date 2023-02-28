using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace SocketConnect
{
	public partial class Form1 : Form
	{
		KeyneceTool plc1;
		KeyneceTool plc2;

        Thread readThread1;
        Thread readThread2;

        Thread t;
		bool readOn, writeOn;
		public Form1()
		{
			InitializeComponent();
            this.Text = "天津信息缘科技有限公司-基恩士上位链路通信协议 v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Control.CheckForIllegalCrossThreadCalls = false;  //程序加载时取消跨线程检查
		}



		
        public void Show1(string str)
        {
            listBox1.Items.Add(DateTime.Now.ToString("HH:mm:ss.fff") + "- " + str);
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }

        private void connect_Click(object sender, EventArgs e)
        {
            if (connect.Text == "连接")
            {
                plc1 = new KeyneceTool(tb_ip.Text, int.Parse(tb_port.Text));
                //plc2 = new KeyneceTool(tb_ip.Text, int.Parse(tb_port.Text));
                bool v1 = plc1.Connect();
                //bool v2 = plc2.ConnectServer();
                if (!(v1 /*& v2*/))
                {
                    Show1("连接失败");
                    return;
                }
                connect.Text = "断开";
                Show1("连接");
                ButtonNo();
            }
            else
            {
                plc1.CloseConnect();
                plc2.CloseConnect();
                connect.Text = "连接";
                Show1("断开");
                ButtonOff();
            }
        }

        public void ButtonOff()
        {
            btn_ReadBool.Enabled = false;
            btn_WriteBool.Enabled = false;
            btn_ReadWord.Enabled = false;
            btn_WriteWord.Enabled = false;
            btn_ReadDWord.Enabled = false;
            btn_WriteDWord.Enabled = false;
            btn_ReadString.Enabled = false;
            btn_WriteString.Enabled = false;
        }

        public void ButtonNo()
        {
            btn_ReadBool.Enabled = false;
            btn_WriteBool.Enabled = false;
            btn_ReadWord.Enabled = true;
            btn_WriteWord.Enabled = true;
            btn_ReadDWord.Enabled = true;
            btn_WriteDWord.Enabled = true;
            btn_ReadString.Enabled = true;
            btn_WriteString.Enabled = true;
        }

        private void btn_ReadBool_Click(object sender, EventArgs e)
        {

        }

        private void btn_WriteBool_Click(object sender, EventArgs e)
        {

        }

        private void btn_ReadWord_Click(object sender, EventArgs e)
        {
            int num;
            if (int.TryParse(tb_ReadWordLength.Text, out num))
            {
                if (cb_ThreadReadOpen.Checked)
                {
                    readThread1 = new Thread(ReadThread1);
                    readThread1.IsBackground = true;
                    readThread1.Start();

                    readThread2 = new Thread(ReadThread11);
                    readThread2.IsBackground = true;
                    readThread2.Start();
                    return;
                }
                if (cb_DoubleThreadTest.Checked)
                {

                }

                if (cb_IsWord.Checked)
                {
                    ReadResult<Int16[]> readResult = plc1.Read16(tb_ReadWordAddress.Text, (UInt16)num);

                    if (readResult.isSuccess)
                    {
                        Show1(JsonConvert.SerializeObject(readResult.result));
                    }
                    else
                    {
                        Show1("读取失败");
                    }
                }
                else
                {
                    ReadResult<UInt16[]> readResult = plc1.ReadU16(tb_ReadWordAddress.Text, (UInt16)num);

                    if (readResult.isSuccess)
                    {
                        Show1(JsonConvert.SerializeObject(readResult.result));
                    }
                    else
                    {
                        Show1("读取失败");
                    }
                }
            }
            else
            {
                if (cb_IsWord.Checked)
                {
                    ReadResult<Int16> readResult = plc1.Read16(tb_ReadWordAddress.Text);
                    if (readResult.isSuccess)
                    {
                        Show1(readResult.result.ToString());
                    }
                    else
                    {
                        Show1("读取失败");
                    }
                }
                else
                {
                    ReadResult<UInt16> readResult = plc1.ReadU16(tb_ReadWordAddress.Text);
                    if (readResult.isSuccess)
                    {
                        Show1(readResult.result.ToString());
                    }
                    else
                    {
                        Show1("读取失败");
                    }
                }
            }
        }

        private void btn_WriteWord_Click(object sender, EventArgs e)
        {
            bool v;

            if (tb_WriteBoolValue.Text.Contains('，'))
            {
                MessageBox.Show("请使用英文逗号分隔");
                return;
            }
            if (tb_WriteWordValue.Text.Contains(','))
            {
                string[] strings = tb_WriteWordValue.Text.Split(',');

                if (cb_IsWord.Checked)
                {
                    Int16[] int16s = new Int16[strings.Length];
                    for (int i = 0; i < strings.Length; i++)
                    {
                        int16s[i] = Int16.Parse(strings[i]);
                    }
                    Show1("开始写入");
                    v = plc1.Write(tb_WriteWordAddress.Text, int16s);
                }
                else
                {
                    UInt16[] uint16s = new UInt16[strings.Length];
                    for (int i = 0; i < strings.Length; i++)
                    {
                        uint16s[i] = UInt16.Parse(strings[i]);
                    }
                    Show1("开始写入");
                    v = plc1.Write(tb_WriteWordAddress.Text, uint16s);
                }

            }
            else
            {
                if (cb_IsWord.Checked)
                {
                    Show1("开始写入");
                    v = plc1.Write(tb_WriteWordAddress.Text, Int16.Parse(tb_WriteWordValue.Text));

                }
                else
                {
                    Show1("开始写入");
                    v = plc1.Write(tb_WriteWordAddress.Text, UInt16.Parse(tb_WriteWordValue.Text));
                }
            }
            if (v)
            {
                Show1("写入成功");
            }
            else
            {
                Show1("写入失败");
            }
        }

        private void btn_ReadDWord_Click(object sender, EventArgs e)
        {
            int num;
            if (int.TryParse(tb_ReadDWordLength.Text, out num))
            {
                if (cb_ThreadReadOpen.Checked)
                {
                    readThread1 = new Thread(ReadThread2);
                    readThread1.IsBackground = true;
                    readThread1.Start();
                    return;
                }
                if (cb_IsDWord.Checked)
                {
                    ReadResult<int[]> readResult = plc1.Read32(tb_ReadDWordAddress.Text, (UInt16)num);
                    if (readResult.isSuccess)
                    {
                        Show1(JsonConvert.SerializeObject(readResult.result));
                    }
                    else
                    {
                        Show1("读取失败");
                    }
                }
                else
                {
                    ReadResult<uint[]> readResult = plc1.ReadU32(tb_ReadDWordAddress.Text, (UInt16)num);
                    if (readResult.isSuccess)
                    {
                        Show1(JsonConvert.SerializeObject(readResult.result));
                    }
                    else
                    {
                        Show1("读取失败");
                    }
                }
            }
            else
            {
                ReadResult<int> readResult = plc1.Read32(tb_ReadDWordAddress.Text);
                if (readResult.isSuccess)
                {
                    Show1(readResult.result.ToString());
                }
                else
                {
                    Show1("读取失败");
                }
            }
        }

        private void btn_WriteDWord_Click(object sender, EventArgs e)
        {
            bool v;

            if (tb_WriteBoolValue.Text.Contains('，'))
            {
                MessageBox.Show("请使用英文逗号分隔");
                return;
            }

            if (tb_WriteDWordValue.Text.Contains(','))
            {
                string[] strings = tb_WriteDWordValue.Text.Split(',');

                if (cb_IsDWord.Checked)
                {
                    int[] ints = new int[strings.Length];
                    for (int i = 0; i < strings.Length; i++)
                    {
                        ints[i] = int.Parse(strings[i]);
                    }
                    Show1("开始写入");
                    v = plc1.Write(tb_WriteDWordAddress.Text, ints);

                    if (v)
                    {
                        Show1("写入成功");
                    }
                    else
                    {
                        Show1("写入失败");
                    }
                }
                else
                {
                    uint[] uints = new uint[strings.Length];
                    for (int i = 0; i < strings.Length; i++)
                    {
                        uints[i] = (uint)int.Parse(strings[i]);
                    }
                    Show1("开始写入");
                    v = plc1.Write(tb_WriteDWordAddress.Text, uints);

                    if (v)
                    {
                        Show1("写入成功");
                    }
                    else
                    {
                        Show1("写入失败");
                    }
                }
            }
            else
            {
                if (cb_IsDWord.Checked)
                {
                    Show1("开始写入");
                    v = plc1.Write(tb_WriteDWordAddress.Text, int.Parse(tb_WriteDWordValue.Text));

                    if (v)
                    {
                        Show1("写入成功");
                    }
                    else
                    {
                        Show1("写入失败");
                    }
                }
                else
                {
                    Show1("开始写入");
                    v = plc1.Write(tb_WriteDWordAddress.Text, uint.Parse(tb_WriteDWordValue.Text));

                    if (v)
                    {
                        Show1("写入成功");
                    }
                    else
                    {
                        Show1("写入失败");
                    }
                }
            }
        }

        private void btn_ReadString_Click(object sender, EventArgs e)
        {
            ReadResult<string> readResult = plc1.ReadString(tb_ReadStringAddress.Text, int.Parse(tb_ReadStringLength.Text));
            if (readResult.isSuccess)
            {
                Show1(readResult.result);
            }
            else
            {
                Show1("读取失败");
            }
        }

        private void btn_WriteString_Click(object sender, EventArgs e)
        {
            bool v = plc1.Write(tb_WriteStringAddress.Text, tb_WriteStringValue.Text);
            if (v)
            {
                Show1("写入成功");
            }
            else
            {
                Show1("写入失败");
            }
        }

        private void btn_StopThreadRead_Click(object sender, EventArgs e)
        {
            if (readThread1 != null)
            {
                readThread1.Abort();
            }

            if (readThread2 != null)
            {
                readThread2.Abort();
            }
        }

        private void ReadThread1()
        {
            while (true)
            {
                ReadResult<Int16[]> readResult = plc1.Read16(tb_ReadWordAddress.Text, (UInt16)int.Parse(tb_ReadWordLength.Text));
                if (readResult.isSuccess)
                {
                    /*if (null == readResult.result || 0 == readResult.result.Length)
                    {
                        ;
                    }*/
                    string v = JsonConvert.SerializeObject(readResult.result);
                    if (v == "[]")
                    {
                        Thread.Sleep(1);
                    }
                    Show1("t1-" + v);
                }
                else
                {
                    Show1("读取失败");
                }
            }
        }

        private void ReadThread11()
        {
            while (true)
            {
                ReadResult<Int16[]> readResult = plc1.Read16("D200", 4);
                if (readResult.isSuccess)
                {
                    /*if (null == readResult.result || 0 == readResult.result.Length)
                    {
                        Thread.Sleep(1);
                    }*/
                    string v = JsonConvert.SerializeObject(readResult.result);
                    if (v == "[]")
                    {
                        Thread.Sleep(1);
                    }
                    Show2("t2-" + v);
                }
                else
                {
                    Show2("读取失败");
                }
            }
        }
        private void ReadThread2()
        {
            while (true)
            {
                ReadResult<int[]> readResult = plc1.Read32(tb_ReadDWordAddress.Text, (UInt16)int.Parse(tb_ReadDWordLength.Text));
                if (readResult.isSuccess)
                {
                    /*if (null == readResult.result || 0 == readResult.result.Length)
                    {
                        Thread.Sleep(1);
                    }*/
                    string v = JsonConvert.SerializeObject(readResult.result);
                    if (v == "[]")
                    {
                        Thread.Sleep(1);
                    }
                    Show1(v);
                }
                else
                {
                    Show1("读取失败");
                }
            }
        }

        public void Show2(string str)
        {
            listBox2.Items.Add(DateTime.Now.ToString("HH:mm:ss.fff") + "- " + str);
            listBox2.SelectedIndex = listBox1.Items.Count - 1;
        }
    }
}
