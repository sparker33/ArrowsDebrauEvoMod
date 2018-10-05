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
		public static Random GLOBALRANDOM;
		public const int SCALE = 5000;   // Scale of domain for position
		public static int ELEMENTCOUNT; // Number of elements
		public static float REPRODUCTIONCHANCE; // Likelihood of checking to see if an element can reproduce
		public static float MUTATIONCHANCE; // Likelihood of element features mutating
		public static float BASEDEATHCHANCE; // Scales likelihood of element death
		public static float INITHOLDINGS; // Scales element initial holdings
		public static float EXCHGRATE; // Scales element resource exchange rate
		public static float ELESPEED; // Scales element move speed

		// Private objects
		private List<Element> elements;
		private List<Resource> resources;
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
					GLOBALRANDOM = new Random();
					Kinematics.DAMPING = 0.01f;
					Kinematics.TIMESTEP = 0.01f;
					ResourceKernel.RESOURCESPEED = 1.0f;
					ResourceKernel.SPREADRATE = 0.01f;
					ELEMENTCOUNT = 5;
					REPRODUCTIONCHANCE = 0.01f;
					MUTATIONCHANCE = 0.01f;
					BASEDEATHCHANCE = 0.1f;
					INITHOLDINGS = 10.0f;
					EXCHGRATE = 3.0f;
					ELESPEED = 100000.0f;
					displayBmp = new Bitmap(panel1.Width, panel1.Height);
					elements = new List<Element>();
					resources = new List<Resource>();

					resources.Add(new Resource(Color.Blue, 1500.0f));
					resources.Add(new Resource(Color.Red, 5000.0f));
					resources.Add(new Resource(Color.Green, 500.0f));
					resources[0].Add(0.25f, new PointF((float)(GLOBALRANDOM.NextDouble() * SCALE), (float)(GLOBALRANDOM.NextDouble() * SCALE)));
					resources[1].Add(0.25f, new PointF((float)(GLOBALRANDOM.NextDouble() * SCALE), (float)(GLOBALRANDOM.NextDouble() * SCALE)));
					resources[2].Add(0.25f, new PointF((float)(GLOBALRANDOM.NextDouble() * SCALE), (float)(GLOBALRANDOM.NextDouble() * SCALE)));
					resources[0].Add(0.25f, new PointF((float)(GLOBALRANDOM.NextDouble() * SCALE), (float)(GLOBALRANDOM.NextDouble() * SCALE)));
					resources[1].Add(0.25f, new PointF((float)(GLOBALRANDOM.NextDouble() * SCALE), (float)(GLOBALRANDOM.NextDouble() * SCALE)));
					resources[2].Add(0.25f, new PointF((float)(GLOBALRANDOM.NextDouble() * SCALE), (float)(GLOBALRANDOM.NextDouble() * SCALE)));
					resources[0].Add(0.25f, new PointF((float)(GLOBALRANDOM.NextDouble() * SCALE), (float)(GLOBALRANDOM.NextDouble() * SCALE)));
					resources[1].Add(0.25f, new PointF((float)(GLOBALRANDOM.NextDouble() * SCALE), (float)(GLOBALRANDOM.NextDouble() * SCALE)));
					resources[2].Add(0.25f, new PointF((float)(GLOBALRANDOM.NextDouble() * SCALE), (float)(GLOBALRANDOM.NextDouble() * SCALE)));
					resources[0].Add(0.25f, new PointF((float)(GLOBALRANDOM.NextDouble() * SCALE), (float)(GLOBALRANDOM.NextDouble() * SCALE)));
					resources[1].Add(0.25f, new PointF((float)(GLOBALRANDOM.NextDouble() * SCALE), (float)(GLOBALRANDOM.NextDouble() * SCALE)));
					resources[2].Add(0.25f, new PointF((float)(GLOBALRANDOM.NextDouble() * SCALE), (float)(GLOBALRANDOM.NextDouble() * SCALE)));
				}
			}
			worker.RunWorkerAsync();
			timer1.Start();
		}

		private void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			Bitmap display = new Bitmap(panel1.Width, panel1.Height);
			Graphics g = Graphics.FromImage(display);
			g.Clear(Color.DarkGray);

			// Update elements
			while (elements.Count < ELEMENTCOUNT)
			{
				elements.Add(new Element(GLOBALRANDOM, resources.Count, INITHOLDINGS, EXCHGRATE, ELESPEED));
			}
			List<List<ResourceKernel>> droppedResources = new List<List<ResourceKernel>>();
			foreach (Element element in elements)
			{
				element.UpdateLocalResourceLevels(resources);
				droppedResources.Add(element.ExchangeResources());
				element.Move();
			}
			int ei = 0;
			while (ei < elements.Count)
			{
				if (GLOBALRANDOM.NextDouble() < REPRODUCTIONCHANCE)
				{
					if (elements[ei].CheckForReproduction())
					{
						elements.Add(elements[ei].Reproduce(GLOBALRANDOM, MUTATIONCHANCE));
					}
				}
				if (elements[ei].CheckForDeath(BASEDEATHCHANCE))
				{
					droppedResources.Add(elements[ei].Die());
					elements.RemoveAt(ei);
					continue;
				}
				ei++;
			}
			// Update and draw resources
			for (int i = 0; i < resources.Count; i++)
			{
				foreach (List<ResourceKernel> drops in droppedResources)
				{
					resources[i].Add(drops[i]);
				}
				resources[i].Consolidate();
				for (int j = 0; j < resources[i].Count; j++)
				{
					resources[i][j].Update(GLOBALRANDOM);
					Rectangle rect = resources[i][j].GetBoundingBox((float)panel1.ClientRectangle.Width / SCALE, (float)panel1.ClientRectangle.Height / SCALE);
					if (rect.Size.Height == 0 || rect.Size.Width == 0)
					{
						continue;
					}
					GraphicsPath path = new GraphicsPath();
					path.AddEllipse(rect);
					PathGradientBrush grdBrush = new PathGradientBrush(path);
					int o = resources[i].GetPeakOpacity(j);
					if (o > 0)
					{
						grdBrush.CenterColor = Color.FromArgb(resources[i].GetPeakOpacity(j), resources[i].Color);
					}
					else
					{
						grdBrush.CenterColor = Color.FromArgb(100, Color.DarkGray);
					}
					Color[] pathColors = { Color.FromArgb(0, resources[i].Color) };
					grdBrush.SurroundColors = pathColors;
					g.FillEllipse(grdBrush, rect);
					grdBrush.Dispose();
					path.Dispose();
				}
			}
			// Draw Elements
			foreach (Element element in elements)
			{
				using (Brush b = new SolidBrush(element.ElementColor))
				{
					g.FillEllipse(b,
						(int)(element.Position.X * panel1.ClientRectangle.Width / SCALE),
						(int)(element.Position.Y * panel1.ClientRectangle.Height / SCALE),
						element.Size,
						element.Size);
				}
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
