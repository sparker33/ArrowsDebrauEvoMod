using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EvoMod2
{
	public partial class SettingsForm : Form
	{
		/* Accessors for inputs */
		// General settings
		public int RandomSeed { get => Int32.Parse(randomSeedBox.Text); }
		public int DomainScale { get => Int32.Parse(scaleBox.Text); }
		public int EleCount { get => Int32.Parse(eleCountBox.Text); }
		public float PopEnforcement { get => Single.Parse(popEnforceBox.Text); }
		public float DeathChance { get => Single.Parse(deathChanceBox.Text); }
		public float SizeScaling { get => Single.Parse(sizeScaleBox.Text); }
		public float OpacityScaling { get => Single.Parse(opacityScaleBox.Text); }
		// Kinematics settings
		public bool BoundCollisions { get => boundaryCollisionsCheckBox.Checked; }
		public float Damping { get => Single.Parse(dampingBox.Text); }
		public float TimeStep { get => Single.Parse(timeStepBox.Text); }
		public float SpeedLimit { get => Single.Parse(speedLimitBox.Text); }
		public float EleSpeed { get => Single.Parse(eleSpeedBox.Text); }
		public float DestinationAccel { get => Single.Parse(destinationAccelBox.Text); }
		// Resources settings
		public float ResourceSpeed { get => Single.Parse(resourceSpeedBox.Text); }
		public float ResourceSpread { get => Single.Parse(resourceSpreadBox.Text); }
		public DataGridView NaturalResourcesDataGridView { get => natResourcesDataGrid; }
		public int MaxResourceCount { get => Int32.Parse(maxResourcesCountBox.Text); }
		// Element settings
		public float ColorMutation { get => Single.Parse(colorMutationRateBox.Text); }
		public float TraitSpread { get => Single.Parse(traitSpreadBox.Text); }
		public float InteractCount { get => Single.Parse(interactCountBox.Text); }
		public float InteractRange { get => Single.Parse(interactRangeBox.Text); }
		public float InteractionChoiceScale { get => Single.Parse(interactChoiceScaleBox.Text); }
		public float RelationShipScale { get => Single.Parse(relationshipScaleBox.Text); }
		public float FoodRequirement { get => Single.Parse(foodRequirementBox.Text); }
		public float StartResources { get => Single.Parse(startResourcesBox.Text); }
		public int MaxRelationships { get => Int32.Parse(maxRelationshipsBox.Text); }
		public int MaxLocations { get => Int32.Parse(maxLocationsCountBox.Text); }
		public int MaxActions { get => Int32.Parse(maxActionsCountBox.Text); }
		public float DiscoveryRate { get => Single.Parse(discoveryRateBox.Text); }
		public float KnowledgeTransferRate { get => Single.Parse(knowledgeTransferRateBox.Text); }
		public int MiddleAge { get => Int32.Parse(middleAgeBox.Text); }
		public float TradeRoundoff { get => Single.Parse(tradeRoundoffBox.Text); }
		public float ReproductionChance { get => Single.Parse(reproductionChanceBox.Text); }
		public float MingleChance { get => Single.Parse(mingleChanceBox.Text); }
		public float TradeChance { get => Single.Parse(tradeChanceBox.Text); }
		public float AttackChance { get => Single.Parse(attackChanceBox.Text); }
		public float ChildCost { get => Single.Parse(childCostBox.Text); }
		public float InfantMortality { get => Single.Parse(infantMortalityBox.Text); }
		public float Inheritance { get => Single.Parse(inheritanceBox.Text); }
		public bool IncestAllowed { get => incestAllowedCheckBox.Checked; }

		// Form constructor
		public SettingsForm()
		{
			InitializeComponent();
			natResourcesDataGrid.Rows[0].Cells[1] = new DataGridViewColorDropDownCell();
			natResourcesDataGrid.UserAddedRow += NatResourcesDataGrid_UserAddedRow;
		}

		private void NatResourcesDataGrid_UserAddedRow(object sender, DataGridViewRowEventArgs e)
		{
			e.Row.Cells[1] = new DataGridViewColorDropDownCell();
			e.Row.Cells[3].Value = false;
		}

		public class DataGridViewColorDropDownCell : DataGridViewComboBoxCell
		{
			public DataGridViewColorDropDownCell() : base()
			{
				this.DataSource = Enum.GetValues(typeof(KnownColor))
					.Cast<KnownColor>()
					.Where(c => !System.Drawing.Color.FromKnownColor(c).IsSystemColor)
					.Select(kc => Enum.GetName(typeof(KnownColor), kc))
					.ToList();
			}
		}
	}
}
