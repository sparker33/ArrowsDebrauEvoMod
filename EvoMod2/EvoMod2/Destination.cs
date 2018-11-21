using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace EvoMod2
{
	public class Destination
	{
		// Private objects
		private float startDistance;
		private PointF destination;

		// Public objects
		public float X { get => destination.X; }
		public float Y { get => destination.Y; }
		public bool IsEmpty { get; private set; }

		/// <summary>
		/// Default empty constructor
		/// </summary>
		public Destination()
		{
			Clear();
		}

		/// <summary>
		/// Constructor taking inputs of current and intended destinations
		/// </summary>
		/// <param name="currentLocation"> Current location PointF. </param>
		/// <param name="destination"> Desired destination PointF. </param>
		public Destination(PointF currentLocation, PointF destination)
		{
			if (!currentLocation.IsEmpty && !destination.IsEmpty)
			{
				Set(currentLocation, destination);
			}
			else
			{
				Clear();
			}
		}

		/// <summary>
		/// Method to get percent of trip yet to be completed.
		/// </summary>
		/// <param name="currentLocation"> Current location PointF. </param>
		/// <returns> Float between 0.0 (arrived at destination) and 1.0 (at starting point). </returns>
		public float GetProgress(PointF currentLocation)
		{
			float progress = 0.0f;
			if (startDistance > 1.0f)
			{
				float xDist = destination.X - currentLocation.X;
				float yDist = destination.Y - currentLocation.Y;
				progress = (float)Math.Sqrt(xDist * xDist + yDist * yDist) / startDistance;
			}
			else
			{
				Clear();
			}
			return progress;
		}

		// Method to empty this Destination
		public void Clear()
		{
			startDistance = 0.0f;
			destination = new PointF();
			IsEmpty = true;
		}

		/// <summary>
		/// Method to establish a destination.
		/// </summary>
		/// <param name="currentLocation"> Current location PointF. </param>
		/// <param name="destination"> Desired destination PointF. </param>
		public void Set(PointF currentLocation, PointF newDestination)
		{
			destination = new PointF(newDestination.X, newDestination.Y);
			startDistance = (float)Math.Sqrt((newDestination.X - currentLocation.X) * (newDestination.X - currentLocation.X) + (newDestination.Y - currentLocation.Y) * (newDestination.Y - currentLocation.Y));
			IsEmpty = false;
		}
	}
}
