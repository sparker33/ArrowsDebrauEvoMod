using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace EvoMod2
{
    public class Element
    {
        // Private fields
		private Kinematics kinematics;
		private List<float> resources;

        // Public accessors
        public Color ElementColor { get; private set; }
        public PointF Position { get; private set; }
        public double Mass;
        public readonly int Size;
		
		public Element(Random random)
        {

        }
        
        public List<ResourceKernel> Update(List<float> resourceLevels)
        {
			// Function to update element for each step
			List<ResourceKernel> drops = new List<ResourceKernel>(resourceLevels.Count);

			return drops;
		}
	}
}
