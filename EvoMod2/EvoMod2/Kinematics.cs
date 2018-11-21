using System;
using System.Collections.Generic;

namespace EvoMod2
{
	public class Kinematics
    {
		// Static objects
		public static float DEFAULTDAMPING;
		public static float TIMESTEP;
		public static float SPEEDLIMIT;

		// Private fields
		private float[] acceleration;
        private float[] velocity;

        // Public properties
        public float Speed { get { return (float)Math.Sqrt(velocity[0] * velocity[0] + velocity[1] * velocity[1]); } }
		public float GetVelocity(int dimension) { return velocity[dimension]; }
		public float Damping { get; set; }

        // Class instantiation method
        public Kinematics(int dimension)
        {
            acceleration = new float[dimension];
            velocity = new float[dimension];
			Damping = DEFAULTDAMPING;
        }

		/// <summary>
		/// Method to get displacement to next time step
		/// </summary>
		/// <param name="forceVector"> Forces </param>
		/// <param name="mass"> Mass </param>
		/// <returns> Displacement array (ordered by dimensions). </returns>
        public List<float> GetDisplacement(IEnumerable<float> forceVector, float mass)
        {
			float currentSpeed = Speed;
            List<float> displacement = new List<float>();
            IEnumerator<float> forcesEnumerator = forceVector.GetEnumerator();
            for (int i = 0; forcesEnumerator.MoveNext(); i++)
            {
                if (Double.IsInfinity(forcesEnumerator.Current)
                    || Double.IsNaN(forcesEnumerator.Current)
					|| 0.5f * Math.Abs(acceleration[i]) * TIMESTEP > SPEEDLIMIT)
				{
					acceleration[i] = 0.0f;
                }
				else if (currentSpeed > SPEEDLIMIT)
				{
					acceleration[i] = -(DEFAULTDAMPING * velocity[i]) / mass;
				}
                else
                {
                    acceleration[i] = (forcesEnumerator.Current - Damping * velocity[i]) / mass;
                }
                displacement.Add((velocity[i] * TIMESTEP + 0.5f * acceleration[i] * TIMESTEP * TIMESTEP));
                velocity[i] += acceleration[i] * TIMESTEP;
            }

            return displacement;
        }

		public void ReverseDirection(int dimension)
		{
			velocity[dimension] *= -1.0f;
		}
    }
}
