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
		// Private fields
		private PointF position = new PointF();
		private Kinematics kinematics = new Kinematics(2);
		private Vector deltaResources;
		private Vector localResourceLevels;
		private Vector ownedResourceVolumes;
		private Vector reproductionCost;
		private Matrix resourceExchangeRules;
		private Matrix moveRules;

		// Public accessors
		public Color ElementColor { get; private set; }
		public PointF Position { get => position; private set => position = value; }
		public readonly int Size = 10;

		/// <summary>
		/// Default class constructor
		/// </summary>
		public Element()
		{
			deltaResources = new Vector();
			localResourceLevels = new Vector();
			ownedResourceVolumes = new Vector();
			reproductionCost = new Vector();
			resourceExchangeRules = new Matrix();
			moveRules = new Matrix();
			ElementColor = Color.Blue;
		}

		/// <summary>
		/// Basic class constructor with initial physics configuration
		/// </summary>
		/// <param name="random"> Randomizer. </param>
		/// <param name="resourceTypesCount"> Specifies number of different types of resources in environment. </param>
		/// <param name="maxInitialHoldings"> Controls amount of resources initially provided to element. </param>
		/// <param name="resourceExchangeRate"> Controls rate of resource pickup/drop. </param>
		/// <param name="speed"> Controls element movement speed. </param>
		public Element(Random random, int resourceTypesCount, float maxInitialHoldings, float resourceExchangeRate, float speed)
		{
			deltaResources = new Vector(resourceTypesCount);
			localResourceLevels = new Vector(resourceTypesCount);

			position.X = (float)(random.NextDouble() * DisplayForm.SCALE);
			position.Y = (float)(random.NextDouble() * DisplayForm.SCALE);

			ElementColor = Color.FromArgb(255, random.Next(256), random.Next(256), random.Next(256));

			ownedResourceVolumes = new Vector(resourceTypesCount);
			reproductionCost = new Vector(resourceTypesCount);
			for (int i = 0; i < resourceTypesCount; i++)
			{
				ownedResourceVolumes[i] = maxInitialHoldings * (float)random.NextDouble();
				reproductionCost[i] = 1.1f * ownedResourceVolumes[i];
			}

			resourceExchangeRules = new Matrix(resourceTypesCount, resourceTypesCount);
			for (int i = 0; i < resourceTypesCount; i++)
			{
				for (int j = 0; j < resourceTypesCount; j++)
				{
					resourceExchangeRules[i][j] = resourceExchangeRate * ((float)random.NextDouble() - 0.5f);
				}
			}

			moveRules = new Matrix(2, resourceTypesCount);
			for (int i = 0; i < resourceTypesCount; i++)
			{
				moveRules[0][i] = speed * ((float)random.NextDouble() - 0.5f);
				moveRules[1][i] = speed * ((float)random.NextDouble() - 0.5f);
			}
		}

		/// <summary>
		/// Method to update the local resource level vector
		/// </summary>
		/// <param name="nodes"> Collection of resources in environment. </param>
		public void UpdateLocalResourceLevels(List<Resource> nodes)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				deltaResources[i] = localResourceLevels[i];
				localResourceLevels[i] = 0.0f;
				for (int j = 0; j < nodes[i].Count; j++)
				{
					localResourceLevels[i] += nodes[i][j].GetResourceLevelAt(this.Position);
				}
				localResourceLevels.Magnitude = 1.0f;
				deltaResources = localResourceLevels - deltaResources;
			}
		}

		/// <summary>
		/// Method to exchange resources with environment based on local concentration levels
		/// </summary>
		/// <returns> List of dropped kernels. Resource pickups reflected as negative-volume drops. </returns>
		public List<ResourceKernel> ExchangeResources()
		{
			Vector exchangeVolumes = resourceExchangeRules * localResourceLevels;
			List<ResourceKernel> drops = new List<ResourceKernel>(localResourceLevels.Count);
			for (int i = 0; i < exchangeVolumes.Count; i++)
			{
				if (exchangeVolumes[i] + ownedResourceVolumes[i] < 0.0f)
				{
					exchangeVolumes[i] = -ownedResourceVolumes[i];
				}
				drops.Add(new ResourceKernel(-exchangeVolumes[i], this.Position));
				drops[i].ZeroMoveMatrix();
			}
			ownedResourceVolumes = ownedResourceVolumes + exchangeVolumes;

			return drops;
		}

		/// <summary>
		/// Method to updte the position of this element
		/// </summary>
		public void Move()
		{
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
				temp[1] = Math.Sign(moveRules[1][1]) * moveRules[1].Magnitude;
			}
			if (ownedResourceVolumes.Magnitude <= 0.0f)
			{
				temp = kinematics.GetDisplacement(temp, Single.Epsilon).ToArray();
			}
			else
			{
				temp = kinematics.GetDisplacement(temp, ownedResourceVolumes.Magnitude).ToArray();
			}
			position.X += temp[0];
			position.Y += temp[1];
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
		/// Checks whether or not this element should dia.
		/// </summary>
		/// <param name="deathBaseLikelihood"> Specifies how close to being able to 
		/// reproduce this element must be in order to remain alive.
		/// </param>
		/// <returns> Boolean indicating true (should die) or false (shouldn't). </returns>
		public bool CheckForDeath(float deathBaseLikelihood)
		{
			if ((ownedResourceVolumes * reproductionCost) < (deathBaseLikelihood * ownedResourceVolumes * ownedResourceVolumes))
			{
				return true;
			}
			else if (ownedResourceVolumes.Magnitude <= 0.0f)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Method to kill this element.
		/// </summary>
		public void Die()
		{

		}

		/// <summary>
		/// Method to determine whether or not this element can reproduce
		/// </summary>
		/// <returns> Boolean indicating true (can reproduce) or false (cannot). </returns>
		public bool CheckForReproduction()
		{
			for (int i = 0; i < ownedResourceVolumes.Count; i++)
			{
				if (ownedResourceVolumes[i] < reproductionCost[i])
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Method to make this element reproduce
		/// </summary>
		/// <param name="random"> Random variable for mutation. </param>
		/// <param name="mutationChance"> Decimal chance of random mutation. </param>
		/// <returns> The progeny element. </returns>
		public Element Reproduce(Random random, float mutationChance)
		{
			this.ownedResourceVolumes = this.ownedResourceVolumes - reproductionCost;

			Vector newReproductionCost = new Vector(reproductionCost);
			for (int i = 0; i < reproductionCost.Count; i++)
			{
				if (random.NextDouble() < mutationChance)
				{
					newReproductionCost[i] *= 0.1f * (float)random.NextDouble() + 1.0f;
				}
			}
			Matrix newResourceExchangeRules = new Matrix(resourceExchangeRules);
			for (int i = 0; i < resourceExchangeRules.Count; i++)
			{
				for (int j = 0; j < resourceExchangeRules[i].Count; j++)
				{
					if (random.NextDouble() < mutationChance)
					{
						newResourceExchangeRules[i][j] *= 0.1f * (float)random.NextDouble() + 1.0f;
					}
				}
			}
			Matrix newMoveRules = new Matrix(moveRules);
			for (int i = 0; i < moveRules[0].Count; i++)
			{
				if (random.NextDouble() < mutationChance)
				{
					newMoveRules[0][i] *= 0.1f * (float)random.NextDouble() + 1.0f;
				}
				if (random.NextDouble() < mutationChance)
				{
					newMoveRules[1][i] *= 0.1f * (float)random.NextDouble() + 1.0f;
				}
			}

			return new Element(this, reproductionCost, newReproductionCost, newResourceExchangeRules, newMoveRules);
		}

		/// <summary>
		/// Class constructor for reproduction method
		/// </summary>
		private Element(Element parent, Vector newOwnedResourceVolumes, Vector newReproductionCost, Matrix newResourceExchangeRules, Matrix newMoveRules)
		{
			position.X = parent.Position.X;
			position.Y = parent.Position.Y;
			ElementColor = parent.ElementColor;
			deltaResources = new Vector(newOwnedResourceVolumes);
			localResourceLevels = new Vector(newOwnedResourceVolumes);
			ownedResourceVolumes = new Vector(newOwnedResourceVolumes);
			reproductionCost = new Vector(newReproductionCost);
			resourceExchangeRules = new Matrix(newResourceExchangeRules);
			moveRules = new Matrix(newMoveRules);
		}
	}
}
