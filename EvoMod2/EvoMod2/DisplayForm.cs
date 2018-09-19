using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EvoMod2
{
	public partial class DisplayForm : Form
	{
		// Global values
		public static Random GLOBALRANDOM = new Random();
		public const int SCALE = 5000;   // Scale of domain for position
							
		// Private objects
		private List<Element> elements;
		private List<List<ResourceKernel>> resources;
		private BackgroundWorker worker = new BackgroundWorker();
		private Bitmap displayBmp;

		// Public objects


		/// <summary>
		/// Set up threading
		/// </summary>
		public DisplayForm()
		{
			InitializeComponent();
			worker.DoWork += worker_DoWork;
			worker.RunWorkerCompleted += worker_RunWorkerCompleted;
		}

		private void DisplayForm_Load(object sender, EventArgs e)
		{
			// Create the elements and resoruces
			typeof(Panel).InvokeMember("DoubleBuffered",
				BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
				null, panel1, new object[] { true });

			using (SettingsForm settings = new SettingsForm())
			{
				DialogResult result = settings.ShowDialog();
				if (result == DialogResult.OK)
				{
					Random random = new Random();
					displayBmp = new Bitmap(panel1.Width, panel1.Height);
					worker.RunWorkerAsync();
					timer1.Start();
				}
			}
		}

		private void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			Bitmap display = new Bitmap(panel1.Width, panel1.Height);
			Graphics g = Graphics.FromImage(display);
			g.Clear(Color.DarkGray);

			// Update elements
			List<List<ResourceKernel>> droppedResources = new List<List<ResourceKernel>>();
			for (int i = 0; i < resources.Count; i++)
			{
				droppedResources.Add(new List<ResourceKernel>());
			}
			foreach (Element element in elements)
			{
				List<float> resourceLevels = new List<float>();
				for (int i = 0; i < resources.Count; i++)
				{
					resourceLevels.Add(0.0f);
					foreach (ResourceKernel resourceNode in resources[i])
					{
						resourceLevels[i] += resourceNode.GetResourceLevelAt(element.Position);
					}
				}
				element.Update(resourceLevels); // change this foreach to instead launch threads and then add to droppedResources on Join
			}
			// Update and draw resources
			for (int i = 0; i < resources.Count; i++)
			{
				float dissipationVolume = 0.0f;
				foreach (ResourceKernel resourceNode in resources[i])
				{
					// Update resources
					dissipationVolume += resourceNode.Update(GLOBALRANDOM);

					// Draw resources
					//pathgradientbrush
				}
				dissipationVolume /= resources[i].Count;
				if (dissipationVolume > 0.0f)
				{
					foreach (ResourceKernel resourceNode in resources[i])
					{
						resourceNode.Volume += dissipationVolume;
					}
				}

				// Add new drops
				for (int j = 0; j < droppedResources[i].Count; j++)
				{
					resources[i].Add(droppedResources[i][j]);
				}
			}
			// Draw Elements
			foreach (Element element in elements)
			{
				Brush b = new SolidBrush(element.ElementColor);
				g.FillEllipse(b,
					(int)((element.Position.X + SCALE / 2.0f) * panel1.Size.Width / (float)SCALE),
					(int)((element.Position.Y + SCALE / 2.0f) * panel1.Size.Height / (float)SCALE),
					element.Size,
					element.Size);
			}

			e.Result = display;
		}

		private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			displayBmp = e.Result as Bitmap;
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			if (!worker.IsBusy)
			{
				panel1.BackgroundImage = displayBmp;
				panel1.Invalidate();
				worker.RunWorkerAsync();
			}
		}
	}
}
