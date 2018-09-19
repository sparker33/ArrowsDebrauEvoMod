using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatrixMath;
using System.Drawing;

namespace EvoMod2
{
	public class ResourceKernel : GaussKernel
	{
		// Global values
		public static float RESOURCESPEED = 1.0f;
		public static float SPREADRATE = 0.01f;

		// Private objects
		private Matrix moveMatrix;

		// Public objects
		public float Volume { get; set; }
		public float Smoothing { get { return H[0][0]; } }
		public Vector PositionVector
		{
			get
			{
				Vector v = new Vector(2);
				v[0] = mu[0];
				v[1] = mu[1];
				return v;
			}
		}
		public PointF Position
		{
			get
			{
				return new PointF(mu[0], mu[1]);
			}
			set
			{
				mu[0] = value.X;
				mu[1] = value.Y;
			}
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public ResourceKernel() : base(2)
		{
			H[0][0] = 1.0f;
			H[1][1] = 1.0f;
			H[1][0] = 0.0f;
			moveMatrix = new Matrix(2, 2);
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					moveMatrix[i][j] = 0.0f;
				}
			}
			Volume = 10.0f;
		}

		/// <summary>
		/// Constructor with initial volume and position specified
		/// </summary>
		/// <param name="volume"> Initial resource volume. </param>
		/// <param name="position"> Initial node center position. </param>
		public ResourceKernel(float volume, PointF position) : base(2)
		{
			Volume = volume;
			mu[0] = position.X;
			mu[1] = position.Y;
			H[0][0] = 0.01f * volume;
			H[1][1] = 0.01f * volume;
			H[0][1] = 0.0f;
			H[1][0] = 0.0f;
			moveMatrix = new Matrix(2, 2);
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					moveMatrix[i][j] = RESOURCESPEED * (2.0f * (float)DisplayForm.GLOBALRANDOM.NextDouble() - 1.0f);
				}
			}
		}

		public void ZeroMoveMatrix()
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					moveMatrix[i][j] = 0.0f;
				}
			}
		}

		public float GetResourceLevelAt(PointF location)
		{
			Vector v = new Vector(2);
			v[0] = location.X;
			v[1] = location.Y;
			return (this.Volume * this.ProbabilityAt(v));
		}

		public void Update(Random random)
		{
			Vector v = new Vector(2);
			v[0] = 2.0f * (float)random.NextDouble() - 1.0f;
			v[1] = 2.0f * (float)random.NextDouble() - 1.0f;
			this.Move(v);
			this.H = (1.0f + SPREADRATE / Volume) * this.H;
		}

		private void Move(Vector v)
		{
			Vector dX = moveMatrix * v;
			mu[0] += dX[0];
			mu[1] += dX[1];
			if (mu[0] < 0.0f)
			{
				mu[0] = 5.0f;
				moveMatrix[0] = -1.0f * moveMatrix[0];
			}
			if (mu[0] > DisplayForm.SCALE)
			{
				mu[0] = DisplayForm.SCALE - 5.0f;
				moveMatrix[0] = -1.0f * moveMatrix[0];
			}
			if (mu[1] < 0.0f)
			{
				mu[1] = 5.0f;
				moveMatrix[1] = -1.0f * moveMatrix[1];
			}
			if (mu[1] > DisplayForm.SCALE)
			{
				mu[1] = DisplayForm.SCALE - 5.0f;
				moveMatrix[1] = -1.0f * moveMatrix[1];
			}
		}
	}
}
