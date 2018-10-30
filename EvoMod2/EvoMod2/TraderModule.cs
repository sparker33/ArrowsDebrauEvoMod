using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatrixMath;

namespace EvoMod2
{
	public class TraderModule
	{
		// Statics
		//reserved

		// Private objects
		private float successCenter;
		private float successSpread;
		private float successShift;

		private float happinessCenter;
		private float happinessSpread;
		private float happinessShift;

		private MatrixMath.Vector targetInventory;
		private MatrixMath.Matrix brain;

		// Public objects
		public float Value;
		public MatrixMath.Vector Inventory { get; set; }
		public MatrixMath.Vector DesiredTrade { get => (targetInventory - Inventory); }

		// Methods to add and remove resourcess from this TraderModule
		public void AddResource()
		{
			targetInventory.Add(0.0f);

			brain.InsertColumn(brain.ColumnCount);
			brain.Add(new Vector(brain.ColumnCount));
		}
		public void RemoveResourceAt(int index)
		{
			targetInventory.Add(0.0f);

			brain.InsertColumn(brain.ColumnCount);
			brain.Add(new Vector(brain.ColumnCount));
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public TraderModule()
		{

		}

		/// <summary>
		/// Generate new module with randomizer
		/// </summary>
		/// <param name="random"></param>
		public TraderModule(Random random)
		{
			successCenter = 0.5f - (float)random.NextDouble();
			successSpread = 1.0f - (float)random.NextDouble();
			successShift = 1.0f / (1.0f + (float)Math.Exp(-(1.0f / successSpread) * (1.0 - successCenter)));

			happinessCenter = 0.5f - (float)random.NextDouble();
			happinessSpread = 1.0f - (float)random.NextDouble();
			happinessShift = 1.0f / (1.0f + (float)Math.Exp((1.0f / happinessSpread) * (1.0 - happinessCenter)));
		}

		/// <summary>
		/// Determines trade willingness
		/// </summary>
		/// <param name="tradeProposal"> Trade to evaluate. </param>
		/// <returns> Willingness to trade. Values greater than one indicate good direction but too high magnitude. </returns>
		public float GetTradeWillingness(Vector tradeProposal)
		{
			Vector desiredTrade = this.DesiredTrade;
			float desiredTradeMagnitude = desiredTrade.Magnitude;
			return (-(tradeProposal * desiredTrade) / (desiredTradeMagnitude * desiredTradeMagnitude));
		}

		/// <summary>
		/// Adds the traded resources to this TraderModule's inventory.
		/// </summary>
		/// <param name="trade"> Vector of resources being exchanged. </param>
		public void Trade(Vector trade)
		{
			Inventory += trade;
		}

		/// <summary>
		/// Updates the inventory target vector based on a preferences input.
		/// </summary>
		/// <param name="resourcePreferences"> Resource preferences. </param>
		public void UpdateTargetVector(MatrixMath.Matrix resourcePreferences)
		{
			targetInventory = MatrixMath.Matrix.Transpose(brain * resourcePreferences)[0];
		}

		/// <summary>
		/// Train this TraderModule to ascend the happiness gradient
		/// </summary>
		/// <param name="preferenceHistory"> Matrix history of resource preference inputs by agent. Rows: resources, Columns: number of turns ago. </param>
		/// <param name="happinessHistory"> Vector history of cumulative percent change in happiness. Each 'i'th entry is change since i turns ago. </param>
		/// <param name="tradeSuccessHistory"> Vector history of percent, [0.0,1.0], proposed trades that were accepted. Each 'i'th entry is success i turns ago. </param>
		public void Train(MatrixMath.Matrix preferenceHistory, Vector happinessHistory, Vector tradeSuccessHistory)
		{
			Vector learnCoefficients = new Vector(brain.RowCount);
			for (int i = 0; i < learnCoefficients.Count; i++)
			{
				learnCoefficients[i] = (1.0f / (1.0f + (float)Math.Exp(-(1.0f / successSpread) * (tradeSuccessHistory[i] - successCenter))) - successShift)
					* (1.0f / (1.0f + (float)Math.Exp((1.0f / happinessSpread) * (happinessHistory[i] - happinessCenter))) - happinessShift);
			}

			for (int i = 0; i < brain.ColumnCount; i++)
			{
				for (int j = 0; j < brain.RowCount; j++)
				{
					if (brain[i][j] == 0.0f)
					{
						brain[i][j] += preferenceHistory[i] * learnCoefficients;
					}
					else
					{
						brain[i][j] *= (1.0f + preferenceHistory[i] * learnCoefficients);
					}
				}
			}
		}
	}
}
