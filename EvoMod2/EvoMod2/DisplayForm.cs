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
							
		// Private objects
		private List<Element> elements;
		private List<List<ResourceKernel>> resources;
		private List<Color> resourceColorCode;
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
					displayBmp = new Bitmap(panel1.Width, panel1.Height);
					elements = new List<Element>();
					resources = new List<List<ResourceKernel>>();
					resourceColorCode = new List<Color>();

					resources.Add(new List<ResourceKernel>());
					resourceColorCode.Add(Color.Green);
					resources[0].Add(new ResourceKernel(300.0f, new PointF(200.0f, 250.0f)));
					resources[0].Add(new ResourceKernel(100.0f, new PointF(300.0f, 250.0f)));
					resources.Add(new List<ResourceKernel>());
					resourceColorCode.Add(Color.Blue);
					resources[1].Add(new ResourceKernel(300.0f, new PointF(250.0f, 150.0f)));
					resources[1].Add(new ResourceKernel(100.0f, new PointF(250.0f, 350.0f)));
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
			foreach (Element element in elements)
			{
				element.UpdateLocalResourceLevels(resources);
				droppedResources.Add(element.ExchangeResources());
				element.Move();
			}
			// Update and draw resources
			for (int i = 0; i < resources.Count; i++)
			{
				foreach (List<ResourceKernel> drops in droppedResources)
				{
					resources[i].Add(drops[i]);
				}
				int j = 0;
				bool deleteFlag = false;
				while (j < resources[i].Count)
				{
					for (int k = 1; k < j; k++)
					{
						if ((resources[i][j].PositionVector - resources[i][k].PositionVector).Magnitude
							< resources[i][j].Smoothing / resources[i][j].Volume + resources[i][k].Smoothing / resources[i][k].Volume)
						{
							resources[i][k].Volume += resources[i][j].Volume;
							PointF newPosition = new PointF();
							newPosition.X = (resources[i][j].Volume * resources[i][j].Position.X + resources[i][k].Volume * resources[i][k].Position.X)
								/ (resources[i][j].Volume + resources[i][k].Volume);
							newPosition.Y = (resources[i][j].Volume * resources[i][j].Position.Y + resources[i][k].Volume * resources[i][k].Position.Y)
								/ (resources[i][j].Volume + resources[i][k].Volume);
							resources[i][k].Position = newPosition;
							resources[i].RemoveAt(j);
							deleteFlag = true;
							break;
						}
					}
					if (!deleteFlag)
					{
						j++;
					}
					deleteFlag = false;
				}
				foreach (ResourceKernel kernel in resources[i])
				{
					kernel.Update(GLOBALRANDOM);
					int dia = (int)(Math.Sqrt(kernel.Smoothing) * kernel.Volume);
					Rectangle rect = new Rectangle((int)((kernel.Position.X - dia) * panel1.Size.Width / SCALE),
					(int)((kernel.Position.Y - dia) * panel1.Size.Height / SCALE),
					dia,
					dia);
					GraphicsPath path = new GraphicsPath();
					path.AddEllipse(rect);
					PathGradientBrush grdBrush = new PathGradientBrush(path);
					grdBrush.CenterColor = Color.FromArgb(200, resourceColorCode[i]);
					Color[] pathColors = { Color.FromArgb(1, resourceColorCode[i]) };
					grdBrush.SurroundColors = pathColors;
					g.FillEllipse(grdBrush, rect);
				}
			}
			// Draw Elements
			foreach (Element element in elements)
			{
				Brush b = new SolidBrush(element.ElementColor);
				g.FillEllipse(b,
					(int)(element.Position.X * panel1.Size.Width / SCALE),
					(int)(element.Position.Y * panel1.Size.Height / SCALE),
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
