using System;
using System.Collections.Generic;

namespace EvoMod2
{
	public class Kinematics
    {
        // Private fields
        private double[] acceleration;
        private double[] velocity;

		// Public objects
		public static double DAMPING;
		public static double TIMESTEP;

        // Public properties
        public double Speed { get { return Math.Sqrt(velocity[0] * velocity[0] + velocity[1] * velocity[1]); } }

        // Class instantiation method
        public Kinematics(int dimension)
        {
            acceleration = new double[dimension];
            velocity = new double[dimension];
        }

        public IEnumerable<double> GetDisplacement(IEnumerable<double> forceVector, double mass)
        {
            List<double> displacement = new List<double>();
            IEnumerator<double> forcesEnumerator = forceVector.GetEnumerator();
            for (int i = 0; forcesEnumerator.MoveNext(); i++)
            {
                if (Double.IsInfinity(forcesEnumerator.Current)
                    || Double.IsNaN(forcesEnumerator.Current))
                {
                    acceleration[i] = -(DAMPING * velocity[i]) / mass;
                }
                else
                {
                    acceleration[i] = (forcesEnumerator.Current - DAMPING * velocity[i]) / mass;
                }
                displacement.Add((velocity[i] * TIMESTEP + 0.5 * acceleration[i] * TIMESTEP * TIMESTEP));
                velocity[i] += acceleration[i] * TIMESTEP;
            }

            return displacement;
        }
    }
}
