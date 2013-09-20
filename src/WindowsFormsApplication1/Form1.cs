using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
	using System.Collections.Concurrent;
	using System.Threading;
	
	public partial class Form1 : Form
	{
		private ConcurrentQueue<Tuple<string, string>> queue = new ConcurrentQueue<Tuple<string, string>>(); 
		public Form1()
		{
			InitializeComponent();

			var t = new Thread(Poll);
			t.Start();

			var q = new Thread(Post);
			q.Start();

		}

		private void Post()
		{
			while (true)
			{
				Tuple<string, string> item;
				queue.TryPeek(out item);

				if (item != null)
				{
					EventStoreClient.PostMessage("Big bucket", textBox1.Text);
					Tuple<string, string> dequeud;
					queue.TryDequeue(out dequeud);
				}

			}
		}

		private void Poll()
		{


			Uri last = null;
			
			while (last == null)
			{
				last = EventStoreClient.GetLast(new Uri("http://ha.geteventstore.com:2113/streams/chat-foo"));
				if (last == null)
				{
					Thread.Sleep(1000);
				}
			}

			while (true)
			{
				var current = EventStoreClient.ReadPrevious(last, x => this.BeginInvoke(new Action(() => AddLine(x))));

				if (last == current)
				{
					Thread.Sleep(1000);
				}
				 last = current;
			}

		}

		void AddLine(string text)
		{
			richTextBox1.AppendText( text);
		}

		private void Form1_Load(object sender, EventArgs e)
		{

		}

		private void textBox2_TextChanged(object sender, EventArgs e)
		{

		}

		private void button1_Click(object sender, EventArgs e)
		{
			queue.Enqueue(new Tuple<string, string>("Big bucket", textBox1.Text));
		}
	}
}
