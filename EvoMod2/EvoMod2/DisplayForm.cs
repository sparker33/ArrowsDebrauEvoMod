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
using MatrixMath;

namespace EvoMod2
{
	public partial class DisplayForm : Form
	{
		// Global values
		public const int SCALE = 5000;   // Scale of domain for positions
		public static Random GLOBALRANDOM; // Global random number generator
		public static int ELEMENTCOUNT; // Number of elements
		public static float DEATHCHANCE; // Scales the health treashold for death

		// Private objects
		private static List<Element> elements;
		private static List<Resource> resources;
		private BackgroundWorker worker = new BackgroundWorker();
		private Bitmap displayBmp;

		// Public objects
		public static int NaturalResourceTypesCount { get => resources.Count; }
		public static float DomainMaxDistance;

		/// <summary>
		/// Set up threading
		/// </summary>
		public DisplayForm()
		{
			InitializeComponent();
			DomainMaxDistance = (float)Math.Sqrt(2.0) * SCALE;
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
					ELEMENTCOUNT = 300;
					DEATHCHANCE = 0.05f;
					Kinematics.DEFAULTDAMPING = 0.08f;
					Kinematics.TIMESTEP = 0.05f;
					ResourceKernel.RESOURCESPEED = 1.0f;
					ResourceKernel.SPREADRATE = 0.0f;
					Element.COLORMUTATIONRATE = 0.15f;
					Element.TRAITSPREAD = 3.5f;
					Element.INTERACTCOUNT = ELEMENTCOUNT / 4.0f;
					Element.INTERACTRANGE = SCALE / 100;
					Element.ELESPEED = 7500.0f;
					Element.RELATIONSHIPSCALE = 10.0f;
					Element.FOODREQUIREMENT = 0.5f;
					Element.STARTRESOURCES = 75.0f;
					Element.MAXRELATIONSHIPS = 10;
					Element.MAXLOCATIONSCOUNT = 10;
					Element.MAXRESOURCECOUNT = 25;
					Element.MAXACTIONSCOUNT = 10;
					Element.DISCOVERYRATE = 0.003f;
					Element.MIDDLEAGE = 50;
					Element.TRADEROUNDOFF = 0.0001f;
					Element.REPRODUCTIONCHANCE = 0.05f;
					Element.CHILDCOST = 0.5f;
					displayBmp = new Bitmap(panel1.Width, panel1.Height);
					elements = new List<Element>();
					resources = new List<Resource>();

					/* COMMENTED OUT IN LIEU OF PROGRAMMATIC VERSION FOR DEBUGGING SPEED */
					//for (int i = 0; i < settings.NaturalResourcesDataGridView.Rows.Count; i++)
					//{
					//	float resourceVolume;
					//	int nodeCount;
					//	try
					//	{
					//		if (!Single.TryParse(settings.NaturalResourcesDataGridView.Rows[i].Cells[0].Value.ToString(), out resourceVolume)
					//			|| !Int32.TryParse(settings.NaturalResourcesDataGridView.Rows[i].Cells[2].Value.ToString(), out nodeCount))
					//		{
					//			continue;
					//		}
					//	}
					//	catch (NullReferenceException)
					//	{
					//		continue;
					//	}
					//	resources.Add(new Resource(Color.FromName(settings.NaturalResourcesDataGridView.Rows[i].Cells[1].Value.ToString()), resourceVolume));

					//	float[] pcts = new float[nodeCount];
					//	float sum = 0.0f;
					//	for (int j = 0; j < pcts.Length; j++)
					//	{
					//		pcts[j] = (float)GLOBALRANDOM.NextDouble();
					//		sum += pcts[j];
					//	}
					//	for (int j = 0; j < pcts.Length; j++)
					//	{
					//		resources[i].Add(pcts[j] / sum, new PointF((float)(GLOBALRANDOM.NextDouble() * SCALE), (float)(GLOBALRANDOM.NextDouble() * SCALE)));
					//	}

					//	DataGridViewCheckBoxCell isFoodCell = settings.NaturalResourcesDataGridView.Rows[i].Cells[3] as DataGridViewCheckBoxCell;
					//	if (bool.Parse(isFoodCell.Value.ToString()))
					//	{
					//		Element.FoodResources.Add(new FoodResourceData(i, 1.0f - (float)GLOBALRANDOM.NextDouble()));
					//	}
					//}

					// Debugging auto-resource-populate
					resources.Add(new Resource(Color.Blue, 80000000));
					Element.FoodResources.Add(new FoodResourceData(0, 1.0f - (float)GLOBALRANDOM.NextDouble()));
					resources.Add(new Resource(Color.Red, 50000000));
					resources.Add(new Resource(Color.White, 12000000));
					resources.Add(new Resource(Color.Black, 30000000));
					for (int i = 0; i < resources.Count; i++)
					{
						float[] pcts = new float[3];
						float sum = 0.0f;
						for (int j = 0; j < pcts.Length; j++)
						{
							pcts[j] = (float)GLOBALRANDOM.NextDouble();
							sum += pcts[j];
						}
						for (int j = 0; j < pcts.Length; j++)
						{
							resources[i].Add(pcts[j] / sum, new PointF((float)(GLOBALRANDOM.NextDouble() * SCALE), (float)(GLOBALRANDOM.NextDouble() * SCALE)));
						}
					}
				}
			}
			// Populate initial generation of elements
			while (elements.Count < ELEMENTCOUNT)
			{
				elements.Add(new Element(GLOBALRANDOM, resources));
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
			int n = 0;
			List<Element> children = new List<Element>();
			while (n < elements.Count)
			{
				elements[n].CheckForDeath((float)Math.Exp(DEATHCHANCE * (elements.Count + children.Count - ELEMENTCOUNT)));
				if (elements[n].IsDead)
				{
					elements.RemoveAt(n);
					continue;
				}
				elements[n].Eat();
				bool? newResource = elements[n].DoAction(resources, elements);
				if (newResource.HasValue)
				{
					for (int i = 0; i < elements.Count; i++)
					{
						if (elements[n].IsDead)
						{
							elements.RemoveAt(n);
						}
						else
						{
							elements[i].AddResource(newResource.Value);
						}
					}
					for (int i = 0; i < children.Count; i++)
					{
						children[i].AddResource(newResource.Value);
					}
				}
				children.AddRange(elements[n].DoInteraction(GLOBALRANDOM, elements));
				elements[n].Move();
				n++;
			}
			elements.AddRange(children);
			// Update and draw resources
			for (int i = 0; i < resources.Count; i++)
			{
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
				int size = element.Size;
				using (Brush b = new SolidBrush(Color.FromArgb(element.Opacity, element.ElementColor.R, element.ElementColor.G, element.ElementColor.B)))
				{
					g.FillEllipse(b,
						(int)(element.Position.X * panel1.ClientRectangle.Width / SCALE) - size / 2,
						(int)(element.Position.Y * panel1.ClientRectangle.Height / SCALE) - size / 2,
						size,
						size);
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
