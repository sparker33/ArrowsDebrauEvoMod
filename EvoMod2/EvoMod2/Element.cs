using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using MatrixMath;

namespace EvoMod2
{
	public class Element
	{
		// Public static fields

		// Private fields
		private PointF position = new PointF();
		private Kinematics kinematics = new Kinematics(2);
		private PointF destination;
		private float health;
		private float mobility;
		private float intelligence;
		private float conscientiousness;
		private float agreeableness;
		private float openness;
		private float extraversion;
		private float neuroticism;
		private float happinessBonus;
		private float[] happinessWeights = new float[2]; // 0: wealth, 1: health
		private TraderModule trader;
		private MatrixMath.Matrix resourceUseHistory;
		private Vector happinessPercentChangeHistory;
		private Vector percentTradesSuccessfulHistory;

		// Public accessors
		public float Happiness { get => (happinessBonus + happinessWeights[0] * trader.Value + happinessWeights[1] * health); }
		public int Age { get; private set; }
		public List<PointF> KnownLocations { get; set; }
		public PointF Position { get => position; private set => position = value; }
		public int Size { get => 10; }
		public Color ElementColor { get; private set; }

		/// <summary>
		/// Default class constructor. Not intended for use
		/// </summary>
		private Element()
		{

		}

		/// <summary>
		/// Basic class constructor with initial physics configuration
		/// </summary>
		/// <param name="random"> Randomizer. </param>
		public Element(Random random)
		{
			Age = 0;

			position.X = (float)(random.NextDouble() * DisplayForm.SCALE);
			position.Y = (float)(random.NextDouble() * DisplayForm.SCALE);
			KnownLocations.Add(position);
			int r = (int)(255.0 / (1.0 + Math.Exp((15.0 / DisplayForm.SCALE) * (position.X - DisplayForm.SCALE / 2.0))));
			int g = (int)(255.0 / (1.0 + Math.Exp((15.0 / DisplayForm.SCALE) * (position.X * position.Y / (2 * DisplayForm.SCALE * DisplayForm.SCALE)))));
			int b = (int)(255.0 / (1.0 + Math.Exp((15.0 / DisplayForm.SCALE) * (position.Y - DisplayForm.SCALE / 2.0))));
			ElementColor = Color.FromArgb(r, g, b);
		}

		/// <summary>
		/// Method to updte the position of this element
		/// </summary>
		public void Move()
		{
			// Determine driving force vector
			float[] temp = new float[2];
			if (kinematics.GetVelocity(0) != 0.0f)
			{
				temp[0] = (1.0f / kinematics.GetVelocity(0)) * moveRules[0] * deltaResources;
			}
			if (kinematics.GetVelocity(1) != 0.0f)
			{
				temp[1] = (1.0f / kinematics.GetVelocity(1)) * moveRules[1] * deltaResources;
			}
			if (temp[0] == 0.0f && temp[1] == 0.0f)
			{
				temp[0] = Math.Sign(moveRules[0][0]) * moveRules[0].Magnitude;
				temp[1] = Math.Sign(moveRules[1][0]) * moveRules[1].Magnitude;
			}

			// Apply force vector to kinematics; get and apply displacements
			if (ownedResourceVolumes.Magnitude != 0.0f)
			{
				temp = kinematics.GetDisplacement(temp, ownedResourceVolumes.Magnitude).ToArray();
			}
			position.X += temp[0];
			position.Y += temp[1];

			// Handle domain boundary collisions
			if (position.X < 0.0f)
			{
				position.X = 0.0f;
				kinematics.ReverseDirection(0);
			}
			if (position.X > DisplayForm.SCALE)
			{
				position.X = (float)DisplayForm.SCALE;
				kinematics.ReverseDirection(0);
			}
			if (position.Y < 0.0f)
			{
				position.Y = 0.0f;
				kinematics.ReverseDirection(1);
			}
			if (position.Y > DisplayForm.SCALE)
			{
				position.Y = (float)DisplayForm.SCALE;
				kinematics.ReverseDirection(1);
			}
		}

		/// <summary>
		/// Method to make this element reproduce
		/// </summary>
		/// <param name="random"> Random variable for mutation. </param>
		/// <param name="mutationChance"> Decimal chance of random mutation. </param>
		/// <returns> The progeny element. </returns>
		public Element Reproduce(Element mate, Random random, float mutationChance)
		{
			return new Element(this, mate);
		}

		/// <summary>
		/// Class constructor for reproduction method
		/// </summary>
		public Element(Element parent1, Element parent2)
		{
			position.X = parent1.Position.X;
			position.Y = parent1.Position.Y;
		}
	}
}
