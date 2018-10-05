using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace EvoMod2
{
	public class Resource : List<ResourceKernel>
	{
		// Private objects
		private float volume;

		// Public objects
		public Color Color { get; }
		public float Volume { get => volume; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public Resource() : base()
		{
		}

		/// <summary>
		/// Constructor with input resource color and global volume
		/// </summary>
		/// <param name="color"> Resource color for graphics. </param>
		/// <param name="glovalVolume"> Initial global volume of resource. </param>
		public Resource(Color color, float glovalVolume) : base()
		{
			Color = color;
			volume = glovalVolume;
		}

		/// <summary>
		/// Method to add a new ResourceKernel with specified percentage of global resource volume 
		/// and specified initial position.
		/// </summary>
		/// <param name="pctVol"></param>
		/// <param name="position"></param>
		public void Add(float pctVol, PointF position)
		{
			base.Add(new ResourceKernel(pctVol * this.Volume, position));
		}

		/// <summary>
		/// Replaces base Add method. Includes volume updates for any direct add of new kernels
		/// </summary>
		/// <param name="kernel"> New ResourceKernel to add. </param>
		new public void Add(ResourceKernel kernel)
		{
			base.Add(kernel);
			volume += kernel.Volume;
		}

		/// <summary>
		/// Replaces base Remove method. Includes volume updates for any direct remove of kernels
		/// </summary>
		/// <param name="kernel"> ResourceKernel to remove. </param>
		new public void Remove(ResourceKernel kernel)
		{
			base.Remove(kernel);
			volume -= kernel.Volume;
		}

		/// <summary>
		/// Replaces base RemoveAt method. Includes volume updates for any direct remove of kernels
		/// </summary>
		/// <param name="index"> Index of ResourceKernel to remove. </param>
		new public void RemoveAt(int index)
		{
			if (index == 0)
			{
				return;
			}
			volume -= this[index].Volume;
			base.RemoveAt(index);
		}

		/// <summary>
		/// Method to consolidate this resource by combining nearby nodes
		/// </summary>
		public void Consolidate()
		{
			int i = 0;
			bool deleteFlag = false;
			while (i < this.Count)
			{
				for (int k = 1; k < i; k++)
				{
					if ((this[i].PositionVector - this[k].PositionVector).Magnitude
						< this[i].Smoothing / this[i].Volume + this[k].Smoothing / this[k].Volume)
					{
						this[k].Volume += this[i].Volume;
						PointF newPosition = new PointF();
						newPosition.X = (this[i].Volume * this[i].Position.X + this[k].Volume * this[k].Position.X)
							/ (this[i].Volume + this[k].Volume);
						newPosition.Y = (this[i].Volume * this[i].Position.Y + this[k].Volume * this[k].Position.Y)
							/ (this[i].Volume + this[k].Volume);
						this[k].Position = newPosition;
						this.RemoveAt(i);
						deleteFlag = true;
						break;
					}
				}
				if (!deleteFlag)
				{
					i++;
				}
				deleteFlag = false;
			}
		}

		/// <summary>
		/// Gets the center opacity of the graphics for the kernel at entered index.
		/// </summary>
		/// <param name="index"> Index of kernel. </param>
		/// <returns> Center opacity of graphic for kernel. Value is between 0 and 255, inclusive. </returns>
		public int GetPeakOpacity(int index)
		{
			float percentTotal = this[index].Volume / volume;
			return (int)(255.0f * percentTotal);
		}
	}
}
