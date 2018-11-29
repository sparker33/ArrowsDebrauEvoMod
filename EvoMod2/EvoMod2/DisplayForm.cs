using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		public static Random GLOBALRANDOM; // Global random number generator
		public static int SCALE;   // Scale of domain for positions
		public static double SIZESCALING; // Scales rate of change of dot sizes wrt wealth
		public static double OPACITYSCALING; // Scales rate of change of dot opacity wrt health
		public static bool BOUNDARYCOLLISIONS; // Forces elements to remain within viewable area when true
		public static int ELEMENTCOUNT; // Number of elements
		public static float POPULATIONENFORCEMENT; // Scales how strictly the maximum population is enforced
		public static float DEATHCHANCE; // Scales the health treashold for death

		// Private objects
		private static List<Element> elements = new List<Element>();
		private static List<Resource> resources = new List<Resource>();
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
					// Read in simulation inputs
					GLOBALRANDOM = new Random(settings.RandomSeed);
					SCALE = settings.DomainScale;
					BOUNDARYCOLLISIONS = settings.BoundCollisions;
					ELEMENTCOUNT = settings.EleCount;
					POPULATIONENFORCEMENT = settings.PopEnforcement;
					DEATHCHANCE = settings.DeathChance;
					SIZESCALING = settings.SizeScaling;
					OPACITYSCALING = settings.OpacityScaling;
					Kinematics.DEFAULTDAMPING = settings.Damping;
					Kinematics.TIMESTEP = settings.TimeStep;
					Kinematics.SPEEDLIMIT = settings.SpeedLimit;
					ResourceKernel.RESOURCESPEED = settings.ResourceSpeed;
					ResourceKernel.SPREADRATE = settings.ResourceSpread;
					Element.COLORMUTATIONRATE = settings.ColorMutation;
					Element.TRAITSPREAD = settings.TraitSpread;
					Element.INTERACTCOUNT = settings.InteractCount;
					Element.INTERACTRANGE = settings.InteractRange;
					Element.ELESPEED = settings.EleSpeed;
					Element.DESTINATIONACCEL = settings.DestinationAccel;
					Action.ACTIONLEARNRATE = settings.ActionLearnRate;
					Element.INTERACTIONCHOICESCALE = settings.InteractionChoiceScale;
					Element.RELATIONSHIPSCALE = settings.RelationShipScale;
					Element.FOODREQUIREMENT = settings.FoodRequirement;
					Element.STARTRESOURCES = settings.StartResources;
					Element.MAXRELATIONSHIPS = settings.MaxRelationships;
					Element.MAXLOCATIONSCOUNT = settings.MaxLocations;
					Element.MAXACTIONSCOUNT = settings.MaxActions;
					Element.MAXRESOURCECOUNT = settings.MaxResourceCount;
					Element.DISCOVERYRATE = settings.DiscoveryRate;
					Element.DISCOVERYRATE = settings.KnowledgeTransferRate;
					Element.MIDDLEAGE = settings.MiddleAge;
					Element.TRADEROUNDOFF = settings.TradeRoundoff;
					Element.REPRODUCTIONCHANCE = settings.ReproductionChance;
					Element.MINGLECHANCE = settings.MingleChance;
					Element.TRADECHANCE = settings.TradeChance;
					Element.ATTACKCHANCE = settings.AttackChance;
					Element.CHILDCOST = settings.ChildCost;
					Element.INFANTMORTALITY = settings.InfantMortality;
					Element.INHERITANCE = settings.Inheritance;
					Element.INCESTALLOWED = settings.IncestAllowed;

					// Set up natural resources according to inputs table
					for (int i = 0; i < settings.NaturalResourcesDataGridView.Rows.Count; i++)
					{
						float resourceVolume;
						int nodeCount;
						try
						{
							if (!Single.TryParse(settings.NaturalResourcesDataGridView.Rows[i].Cells[0].Value.ToString(), out resourceVolume)
								|| !Int32.TryParse(settings.NaturalResourcesDataGridView.Rows[i].Cells[2].Value.ToString(), out nodeCount))
							{
								continue;
							}
						}
						catch (NullReferenceException)
						{
							continue;
						}
						resources.Add(new Resource(Color.FromName(settings.NaturalResourcesDataGridView.Rows[i].Cells[1].Value.ToString()), resourceVolume));

						float[] pcts = new float[nodeCount];
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

						DataGridViewCheckBoxCell isFoodCell = settings.NaturalResourcesDataGridView.Rows[i].Cells[3] as DataGridViewCheckBoxCell;
						if (bool.Parse(isFoodCell.Value.ToString()))
						{
							Element.FoodResources.Add(new FoodResourceData(i, 1.0f - (float)GLOBALRANDOM.NextDouble()));
						}
					}
				}
				else
				{
					// Use default values
					GLOBALRANDOM = new Random();
					SCALE = 5000;
					BOUNDARYCOLLISIONS = true;
					ELEMENTCOUNT = 600;
					POPULATIONENFORCEMENT = 1.0f;
					DEATHCHANCE = 0.0235f;
					SIZESCALING = 7.0f;
					OPACITYSCALING = 2.0f;
					Kinematics.DEFAULTDAMPING = 0.175f;
					Kinematics.TIMESTEP = 0.05f;
					Kinematics.SPEEDLIMIT = SCALE / 17.5f;
					ResourceKernel.RESOURCESPEED = 3.0f;
					ResourceKernel.SPREADRATE = 0.0f;
					Element.COLORMUTATIONRATE = 0.15f;
					Element.TRAITSPREAD = 3.75f;
					Element.INTERACTCOUNT = ELEMENTCOUNT / 10.0f;
					Element.INTERACTRANGE = SCALE / 750.0f;
					Element.ELESPEED = SCALE / 5.0f;
					Element.DESTINATIONACCEL = 5.0f;
					Action.ACTIONLEARNRATE = 5.0;
					Element.INTERACTIONCHOICESCALE = 10.0f;
					Element.RELATIONSHIPSCALE = 25.0f;
					Element.FOODREQUIREMENT = 0.25f;
					Element.STARTRESOURCES = 15.0f;
					Element.MAXRELATIONSHIPS = ELEMENTCOUNT;
					Element.MAXLOCATIONSCOUNT = 30;
					Element.MAXRESOURCECOUNT = 15;
					Element.MAXACTIONSCOUNT = 15;
					Element.DISCOVERYRATE = 0.0001f;
					Element.KNOWLEDGETRANSFERRATE = 0.075f;
					Element.MIDDLEAGE = 500;
					Element.TRADEROUNDOFF = 0.0001f;
					Element.REPRODUCTIONCHANCE = 0.1f;
					Element.MINGLECHANCE = 1.3f;
					Element.TRADECHANCE = 1.5f;
					Element.ATTACKCHANCE = 0.075f;
					Element.CHILDCOST = 0.5f;
					Element.INFANTMORTALITY = 0.0175f;
					Element.INHERITANCE = 1.0f;
					Element.INCESTALLOWED = false;

					// Set up default background resources
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
			// Begin simulation
			worker.RunWorkerAsync();
			timer1.Start();
		}

		private void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			Bitmap display;
			try
			{
				display = new Bitmap(panel1.Width, panel1.Height);
			}
			catch (Exception)
			{
				display = new Bitmap(10, 10);
			}
			Graphics g = Graphics.FromImage(display);
			g.Clear(Color.DarkGray);

			// Update elements
			int n = 0;
			List<Element> children = new List<Element>();
			float deathHealthThreshold = (DEATHCHANCE * Element.MIDDLEAGE) * (float)Math.Exp(POPULATIONENFORCEMENT * (elements.Count - ELEMENTCOUNT));
			while (n < elements.Count)
			{
				elements[n].CheckForDeath((float)GLOBALRANDOM.NextDouble() * deathHealthThreshold);
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
