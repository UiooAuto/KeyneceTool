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

namespace KeyneceTool
{
	public partial class Form1 : Form
	{
		SocketUtils plc1;
		SocketUtils plc2;
		SocketUtils plc3;
		Thread t;
		bool readOn, writeOn;
		public Form1()
		{
			InitializeComponent();
			Control.CheckForIllegalCrossThreadCalls = false;  //程序加载时取消跨线程检查
		}

		public void ClickConnectBtn(ref SocketUtils plc, TextBox ip, TextBox port, Button btn)
        {
			if ("断开".Equals(btn.Text))
			{
				readOn = false;
				plc.CloseConnect();
				btn.Text = "连接";
			}
			else if ("连接".Equals(btn.Text))
			{
				plc = new SocketUtils(ip.Text, int.Parse(port.Text));
				bool v = plc.Connect();
				if (!v)
				{
					MessageBox.Show("连接失败");
				}
				else
				{
					btn.Text = "断开";
				}
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			ClickConnectBtn(ref plc1, textBox1, textBox2, button1);
		}

		private void button2_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(textBox6.Text))
			{
				Stopwatch s = new Stopwatch();
				s.Start();
				ReadResult<short> readResult = plc1.Read16(textBox3.Text);
				s.Stop();
				if (readResult.isSuccess)
				{
					MessageBox.Show(readResult.result.ToString());
				}
			}
			else
			{
				int v = int.Parse(textBox6.Text);
				Stopwatch s = new Stopwatch();
				s.Start();
				ReadResult<short[]> readResult = plc1.Read16(textBox3.Text, v);
				s.Stop();
				string str = "";
				if (readResult.isSuccess)
				{
					foreach (short v1 in readResult.result)
					{
						str = str + v1.ToString() + ",";
					}
					MessageBox.Show(str);
				}
				MessageBox.Show(s.Elapsed.Milliseconds.ToString()+"ms");
			}
		}

		private void button3_Click(object sender, EventArgs e)
		{
			short cmd;
			bool v;
			button3.Enabled = false;
			if (short.TryParse(textBox5.Text, out cmd))
			{
				v = plc1.Write(textBox4.Text, cmd);
			}
			else
			{
				v = plc1.Write(textBox4.Text, textBox5.Text);
			}

			//v = plc.Write(textBox4.Text, new uint[] {0,0,0,0});
			//v = plc.Write(textBox4.Text, 0x00ffff00);

			if (v)
			{
				MessageBox.Show("写入成功");
			}
			else
			{
				MessageBox.Show("写入失败");
			}
			button3.Enabled = true;
		}

		private void button4_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(textBox7.Text))
			{
				ReadResult<int> readResult = plc1.Read32(textBox8.Text);
				if (readResult.isSuccess)
				{
					MessageBox.Show(readResult.result.ToString());
				}
				else
				{
					MessageBox.Show("读取失败");
				}
			}
			else
			{
				int v = int.Parse(textBox7.Text);
				Stopwatch s = new Stopwatch();
				s.Start();
				ReadResult<int[]> readResult = plc1.Read32(textBox8.Text, v);
				s.Stop();
				string str = "";
				if (readResult.isSuccess)
				{
					foreach (int v1 in readResult.result)
					{
						//string str1 = Convert.ToString(v1, 16);
						str = str + v1 + ",";
					}
					MessageBox.Show(str);
				}
				MessageBox.Show(s.Elapsed.Milliseconds.ToString() + "ms");
			}
			
		}

		private void button5_Click(object sender, EventArgs e)
		{
			int v;
			if (string.IsNullOrWhiteSpace(textBox6.Text))
			{

				v = 1;
			}
			else
			{
				v = int.Parse(textBox6.Text);
			}
			Stopwatch s = new Stopwatch();
			s.Start();
			ReadResult<string> readResult = plc1.ReadString(textBox3.Text, v);
			s.Stop();
			if (readResult.isSuccess)
			{
				MessageBox.Show(readResult.result);
			}
			MessageBox.Show(s.Elapsed.Milliseconds.ToString() + "ms");
		}

		private void button6_Click(object sender, EventArgs e)
		{
			int cmd;
			bool v;

            if (int.TryParse(textBox5.Text, out cmd))
            {
                v = plc1.Write(textBox4.Text, cmd);
            }
            else
            {
                v = plc1.Write(textBox4.Text, textBox5.Text);
            }

            /*v = plc.Write(textBox4.Text, new int[] {0,0, 0, 0});*/
			//v = plc.Write(textBox4.Text, 0x00ffff00);

			if (v)
			{
				MessageBox.Show("写入成功");
			}
			else
			{
				MessageBox.Show("写入失败");
			}
		}

		public void read()
		{
			int i = 0;
			while (readOn)
			{
				if (i<10)
				{
					i++;
				}
				else
				{
					i = 0;
				}
				string str = "";
				ReadResult<short[]> readResult = plc1.Read16("D1500", 2);
				if (readResult.isSuccess)
				{
					foreach (int v1 in readResult.result)
					{
						//string str1 = Convert.ToString(v1, 16);
						str = str + v1 + ",";
					}
					label1.Text = str;
					Console.WriteLine(str+ " " + i);
				}
			}
			
		}

		public void write()
		{

			while (writeOn)
			{
				string str = "";
				bool v = plc1.Write("DM2027", -10000);
				if (v)
				{
					Thread.Sleep(10);
				}
			}

		}

        private void button7_Click(object sender, EventArgs e)
        {
			ClickConnectBtn(ref plc2, textBox10, textBox9, button7);
		}

        private void button8_Click(object sender, EventArgs e)
		{
			ClickConnectBtn(ref plc3, textBox12, textBox11, button8);
		}

        private void button9_Click(object sender, EventArgs e)
        {
			int a = 0;
            if (a < 1)
            {
                MessageBox.Show("1");
            }
            else if (a < 2)
            {
                MessageBox.Show("2");
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
		{
            if (t != null)
            {
				t.Abort();
            }
		}
	}
}
