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
	using Newtonsoft.Json.Linq;

	public partial class Form1 : Form
	{
		private ConcurrentQueue<Tuple<string, string>> queue = new ConcurrentQueue<Tuple<string, string>>();
		private List<Appointment> serverAppointments;

		public Form1()
		{
			InitializeComponent();

//			var t = new Thread(PollChat);
//			t.Start();
//
//			var q = new Thread(PostChat);
//			q.Start();

			var tA = new Thread(PollAppointments);
			tA.Start();

		}

		private void PostChat()
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

		private void PollChat()
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

		private void PollAppointments()
		{
			Uri last = null;

			while (last == null)
			{
				last = EventStoreClient.GetLast(new Uri("http://ha.geteventstore.com:2113/streams/calendar-foo"));
				if (last == null)
				{
					Thread.Sleep(5000);
				}
			}

			while (true)
			{
				serverAppointments = new List<Appointment>();
				var current = EventStoreClient.ReadPrevious(last, x => this.BeginInvoke(new Action(() => serverAppointments.Add(GetAppt(x)) )));

				if (last == current)
				{

					this.BeginInvoke(new Action(() => serverAppTextBox.Clear()));
					foreach (var appointement in serverAppointments)
					{
						this.BeginInvoke(new Action(() => serverAppTextBox.AppendText(string.Format("Date: {0} Duration: {1} User: {2}", appointement.DateTime, appointement.Duration, appointement.User))));


					}

					Thread.Sleep(10000);
				}
				last = current;

			}



		}

		private Appointment GetAppt(string x)
		{
			JObject o = JObject.Parse(x);

			DateTime date =(DateTime)o["dateTime"];
			int dur = (int) o["duration"];
			string user = (string) o["user"];
			return  new Appointment{DateTime = date, Duration = dur, User = user};


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
